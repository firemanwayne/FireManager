using FireManager.Abstract;
using FireManager.Concrete;
using FireManager.Entities;
using FireManager.Extensions;
using FireManager.Interface;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace FireManager.Services
{
    internal class ScheduleRequest : RequestBase, IScheduleRequest
    {
        public ScheduleRequest(IRequests requests, IHttpClientFactory factory, IOptions<FireManagerOptions> options)
            : base(requests, factory, options) { }

        public Task<Stream> StreamSchedulesAsync() => SendAsync(Requests.AllSchedulesRequest);

        public async IAsyncEnumerable<FireManagerSchedule> GetSchedulesAsync()
        {
            var results = await ReadResultsAsync(StreamSchedulesAsync());
            var schedules = results.Schedules ?? throw new InvalidDataException("The response is missing schedules.");
            foreach (var schedule in schedules.Schedule ?? Array.Empty<Schedule>())
                yield return FireManagerSchedule.Instance(schedule);
        }
    }
}
