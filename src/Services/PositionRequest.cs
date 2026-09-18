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
    internal class PositionRequest : RequestBase, IPositionRequest
    {
        public PositionRequest(IRequests requests, IHttpClientFactory factory, IOptions<FireManagerOptions> options)
            : base(requests, factory, options) { }

        public Task<Stream> StreamPositionsAsync() => SendAsync(Requests.AllSchedulesRequest);

        public async IAsyncEnumerable<FireManagerPosition> GetPositionsAsync()
        {
            var results = await ReadResultsAsync(StreamPositionsAsync());
            var schedules = results.Schedules ?? throw new InvalidDataException("The response is missing schedules.");
            foreach (var schedule in schedules.Schedule ?? Array.Empty<Schedule>())
                foreach (var positions in schedule.Positions ?? Array.Empty<Positions>())
                    foreach (var position in positions.Position ?? Array.Empty<Position>())
                        yield return FireManagerPosition.Instance(schedule, position);
        }
    }
}
