using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FireManager.Concrete;
using FireManager.Entities;
using FireManager.Extensions;
using FireManager.Interface;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class ApiRegressionTests
{
    private sealed class Handler : HttpMessageHandler
    {
        public string Xml = "<results><ranges /></results>";
        public HttpStatusCode Status = HttpStatusCode.OK;
        public Exception Failure;
        public TrackingStream Body;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            if (Failure != null)
                return Task.FromException<HttpResponseMessage>(Failure);
            Body = new TrackingStream(System.Text.Encoding.UTF8.GetBytes(Xml));
            return Task.FromResult(new HttpResponseMessage(Status) { Content = new StreamContent(Body) });
        }
    }

    private sealed class TrackingStream : MemoryStream
    {
        public bool Disposed;
        public TrackingStream(byte[] bytes) : base(bytes) { }
        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }
    }

    private sealed class Factory : IHttpClientFactory
    {
        private readonly Handler handler;
        public Factory(Handler handler) { this.handler = handler; }
        public HttpClient CreateClient(string name) => new HttpClient(handler, false);
    }

    private static ServiceProvider Provider(Handler handler = null, Action<FireManagerOptions> configure = null)
    {
        var services = new ServiceCollection();
        services.AddFireManager(options =>
        {
            options.AccountId("test");
            options.AccountKey("test");
            options.AccountUrl("https://example.invalid/api");
            configure?.Invoke(options);
        }, false);
        services.AddSingleton<IHttpClientFactory>(new Factory(handler ?? new Handler()));
        return services.BuildServiceProvider();
    }

    private static async Task<List<T>> Collect<T>(IAsyncEnumerable<T> source)
    {
        var values = new List<T>();
        await foreach (var value in source) values.Add(value);
        return values;
    }

    [Fact]
    public async Task MissingMemberAttributesDoNotAbortEnumeration()
    {
        var handler = new Handler { Xml = "<results><members><member id='1'><name>A</name></member><member id='2'><name>B</name><attributes /></member></members></results>" };
        using var provider = Provider(handler);
        var members = await Collect(provider.GetRequiredService<IMemberRequest>().GetMembersAsync(false));
        Assert.Equal(2, members.Count);
        Assert.All(members, member => Assert.Equal("Unknown", member.Rank));
        Assert.All(members, member => Assert.Null(member.EmployeeTypeId));
    }

    [Fact]
    public async Task EmailAndPhoneUseDifferentAttributes()
    {
        var handler = new Handler { Xml = "<results><members><member id='1'><name>A</name><attributes><attribute id='9'><value>a@example.com</value></attribute><attribute id='7'><value>555-0100</value></attribute></attributes></member></members></results>" };
        using var provider = Provider(handler);
        var member = Assert.Single(await Collect(provider.GetRequiredService<IMemberRequest>().GetMembersAsync(false)));
        Assert.Equal("a@example.com", member.Email);
        Assert.Equal("555-0100", member.PhoneNumber);
    }

    [Theory]
    [InlineData(1, "2026-01-15T11:00:00Z")]
    [InlineData(7, "2026-07-15T10:00:00Z")]
    public void DailyQueriesUseDepartmentTimezone(int month, string expected)
    {
        using var provider = Provider();
        var request = provider.GetRequiredService<IRequests>().AllSchedulesByDate(new DateTime(2026, month, 15));
        Assert.Equal(expected, request["bt"]);
    }

    [Fact]
    public void MonthlyQueryConvertsEachBoundaryUsingItsOwnOffset()
    {
        using var provider = Provider();
        var request = provider.GetRequiredService<IRequests>().AllSchedulesByMonth(new DateTime(2026, 3, 1));
        Assert.Equal("2026-03-01T06:00:00Z", request["bt"]);
        Assert.Equal("2026-04-01T05:00:00Z", request["et"]);
    }

    [Fact]
    public void CustomTimezoneAndShiftStartAreHonored()
    {
        using var provider = Provider(configure: o => { o.DepartmentTimeZone = TimeZoneInfo.Utc; o.ShiftStart = TimeSpan.FromHours(7); });
        var request = provider.GetRequiredService<IRequests>().AllSchedulesByDate(new DateTime(2026, 3, 8));
        Assert.Equal("2026-03-08T07:00:00Z", request["bt"]);
        Assert.Equal("2026-03-09T07:00:00Z", request["et"]);
    }

    [Theory]
    [InlineData(3, 8, 2)]
    [InlineData(11, 1, 1)]
    public void AmbiguousOrInvalidDepartmentTimesRequireUtc(int month, int day, int hour)
    {
        using var provider = Provider();
        var requests = provider.GetRequiredService<IRequests>();
        Assert.Throws<ArgumentException>(() => requests.AllSchedulesByDateRange(new DateTime(2026, month, day, hour, 30, 0), new DateTime(2026, month, day, 6, 0, 0)));
    }

    [Fact]
    public void ExplicitUtcRangeIsPreserved()
    {
        using var provider = Provider();
        var request = provider.GetRequiredService<IRequests>().AllSchedulesByDateRange(new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 2, 0, 0, 0, DateTimeKind.Utc));
        Assert.Equal("2026-07-01T00:00:00Z", request["bt"]);
        Assert.Equal("2026-07-02T00:00:00Z", request["et"]);
    }

    [Theory]
    [InlineData("2026-03-08T06:00:00Z", "2026-03-08T09:00:00Z", 3)]
    [InlineData("2026-11-01T05:00:00Z", "2026-11-01T10:00:00Z", 5)]
    public async Task DstShiftDurationUsesElapsedTime(string begin, string end, int hours)
    {
        var handler = new Handler { Xml = $"<results><ranges><range><schedule id='1'/><position id='2'/><member id='3'/><begin>{begin}</begin><end>{end}</end></range></ranges></results>" };
        using var provider = Provider(handler);
        var shift = Assert.Single(await provider.GetRequiredService<IStaffedPositionRequest>().GetStaffedPositionsAsync(new DateTime(2026, 3, 8)));
        Assert.Equal(TimeSpan.FromHours(hours), shift.TotalHours);
        Assert.Equal(DateTimeKind.Utc, shift.StartShift.Kind);
        Assert.Equal(DateTimeKind.Utc, shift.EndShift.Kind);
    }

    [Fact]
    public async Task SchedulesRetainPositions()
    {
        var handler = new Handler { Xml = "<results><schedules><schedule id='1'><name>Station</name><positions><position id='2'><name>Driver</name></position></positions></schedule></schedules></results>" };
        using var provider = Provider(handler);
        var schedule = Assert.Single(await Collect(provider.GetRequiredService<IScheduleRequest>().GetSchedulesAsync()));
        Assert.Equal(1, schedule.PositionCount);
        Assert.Equal("Driver", Assert.Single(schedule.Positions).Name);
        Assert.Equal(1, schedule.Positions[0].ScheduleId);
    }

    [Fact]
    public async Task EmptyRangesWorkForEveryOverload()
    {
        using var provider = Provider();
        var request = provider.GetRequiredService<IStaffedPositionRequest>();
        Assert.Empty(await request.GetStaffedPositionsAsync(new DateTime(2026, 1, 1)));
        Assert.Empty(await request.GetStaffedPositionsAsync(1, 2026));
        Assert.Empty(await request.GetStaffedPositionsAsync(new DateTime(2026, 1, 1), new DateTime(2026, 1, 2)));
    }

    [Fact]
    public async Task HttpErrorsPreserveStatusAndDisposeResponse()
    {
        var handler = new Handler { Status = HttpStatusCode.Unauthorized, Xml = "not XML" };
        using var provider = Provider(handler);
        var error = await Assert.ThrowsAsync<HttpRequestException>(() => Collect(provider.GetRequiredService<IScheduleRequest>().GetSchedulesAsync()));
        Assert.Equal(HttpStatusCode.Unauthorized, error.StatusCode);
        Assert.True(handler.Body.Disposed);
    }

    [Fact]
    public async Task TransportErrorsAreNotReplaced()
    {
        var failure = new HttpRequestException("connection failed");
        using var provider = Provider(new Handler { Failure = failure });
        var error = await Assert.ThrowsAsync<HttpRequestException>(() => Collect(provider.GetRequiredService<IMemberRequest>().GetMembersAsync(false)));
        Assert.Same(failure, error);
    }

    [Fact]
    public async Task ApiErrorsPreserveCodeAndMessage()
    {
        using var provider = Provider(new Handler { Xml = "<results><error code='123'>Access denied</error></results>" });
        var error = await Assert.ThrowsAsync<InvalidDataException>(() => provider.GetRequiredService<IStaffedPositionRequest>().GetStaffedPositionsAsync(1, 2026));
        Assert.Contains("123", error.Message);
        Assert.Contains("Access denied", error.Message);
    }

    [Fact]
    public async Task MissingResultSectionIsNotTreatedAsEmptySuccess()
    {
        using var provider = Provider(new Handler { Xml = "<results />" });
        await Assert.ThrowsAsync<InvalidDataException>(() => provider.GetRequiredService<IStaffedPositionRequest>().GetStaffedPositionsAsync(1, 2026));
    }

    [Fact]
    public async Task DisposingReturnedStreamDisposesResponseBody()
    {
        var handler = new Handler();
        using var provider = Provider(handler);
        var stream = await provider.GetRequiredService<IStaffedPositionRequest>().StreamStaffedPositionsAsync(1, 2026);
        Assert.False(handler.Body.Disposed);
        stream.Dispose();
        Assert.True(handler.Body.Disposed);
    }
}
