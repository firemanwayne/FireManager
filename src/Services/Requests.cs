using FireManager.Extensions;
using FireManager.Interface;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace FireManager.Services
{
    internal class Requests : IRequests
    {
        private readonly FireManagerOptions Options;

        public Requests(IOptions<FireManagerOptions> Options)
        {
            this.Options = Options.Value;
        }

        private string AccountId => Options.Accid;
        private string AccountKey => Options.AccKey;

        private DateTime ToUtc(DateTime value)
        {
            if (value.Kind == DateTimeKind.Utc)
                return value;
            if (value.Kind == DateTimeKind.Local)
                return value.ToUniversalTime();

            var zone = Options.DepartmentTimeZone
                ?? throw new InvalidOperationException("DepartmentTimeZone must be configured.");
            if (zone.IsInvalidTime(value) || zone.IsAmbiguousTime(value))
                throw new ArgumentException("The department time is invalid or ambiguous; supply an explicit UTC instant.", nameof(value));
            return TimeZoneInfo.ConvertTimeToUtc(value, zone);
        }

        public IDictionary<string, string> AllSchedulesByMonth(DateTime RequestDate)
        {
            IDictionary<string, string> values = new Dictionary<string, string>()
            {
                ["accid"] = AccountId,
                ["acckey"] = AccountKey,
                ["cmd"] = "getScheduledTimeRanges",
                ["bt"] = ToUtc(RequestDate).ToString("s") + "Z",
                ["et"] = ToUtc(RequestDate.AddMonths(1)).ToString("s") + "Z",
                ["sch"] = "all",
                ["isp"] = "1",
                ["itt"] = "0"
            };
            return values;
        }
        public IDictionary<string, string> AllSchedulesByDate(DateTime RequestDate)
        {
            if (Options.ShiftStart < TimeSpan.Zero || Options.ShiftStart >= TimeSpan.FromDays(1))
                throw new ArgumentOutOfRangeException(nameof(Options.ShiftStart));

            DateTime Request = DateTime.SpecifyKind(RequestDate.Date, DateTimeKind.Unspecified)
                .Add(Options.ShiftStart);
            IDictionary<string, string> values = new Dictionary<string, string>()
            {
                ["accid"] = AccountId,
                ["acckey"] = AccountKey,
                ["cmd"] = "getScheduledTimeRanges",
                ["bt"] = ToUtc(Request).ToString("s") + "Z",
                ["et"] = ToUtc(Request.AddDays(1)).ToString("s") + "Z",
                ["sch"] = "all",
                ["isp"] = "1",
                ["itt"] = "0"
            };
            return values;
        }
        public IDictionary<string, string> AllSchedulesByDateRange(DateTime StartDate, DateTime EndDate)
        {
            return new Dictionary<string, string>()
            {
                ["accid"] = AccountId,
                ["acckey"] = AccountKey,
                ["cmd"] = "getScheduledTimeRanges",
                ["bt"] = ToUtc(StartDate).ToString("s") + "Z",
                ["et"] = ToUtc(EndDate).ToString("s") + "Z",
                ["sch"] = "all",
                ["isp"] = "1",
                ["itt"] = "0"
            };
        }
        public IDictionary<string, string> AllSchedulesRequest => new Dictionary<string, string>()
        {
            ["accid"] = AccountId,
            ["acckey"] = AccountKey,
            ["cmd"] = "getSchedules",
            ["isp"] = "1"
        };
        public IDictionary<string, string> AllMembersRequest => new Dictionary<string, string>()
        {
            ["accid"] = AccountId,
            ["acckey"] = AccountKey,
            ["cmd"] = "getMembers",
            ["ia"] = "all",
            ["only_active"] = "0"
        };
        public IDictionary<string, string> AllActiveMembersRequest =>
            new Dictionary<string, string>()
            {
                ["accid"] = AccountId,
                ["acckey"] = AccountKey,
                ["cmd"] = "getMembers",
                ["ia"] = "all",
                ["only_active"] = "1"
            };
    }
}