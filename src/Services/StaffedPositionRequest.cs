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
    internal class StaffedPositionRequest : RequestBase, IStaffedPositionRequest
    {
        public StaffedPositionRequest(IRequests requests, IHttpClientFactory factory, IOptions<FireManagerOptions> options)
            : base(requests, factory, options) { }

        public Task<Stream> StreamStaffedPositionsAsync(DateTime RequestDate) =>
            SendAsync(Requests.AllSchedulesByDate(RequestDate));

        public Task<Stream> StreamStaffedPositionsAsync(DateTime StartDate, DateTime EndDate) =>
            SendAsync(Requests.AllSchedulesByDateRange(StartDate, EndDate));

        public Task<Stream> StreamStaffedPositionsAsync(int Month, int Year) =>
            SendAsync(Requests.AllSchedulesByMonth(new DateTime(Year, Month, 1)));

        public Task<IList<FireManagerStaffedPosition>> GetStaffedPositionsAsync(DateTime RequestDate) =>
            ReadPositionsAsync(StreamStaffedPositionsAsync(RequestDate));

        public Task<IList<FireManagerStaffedPosition>> GetStaffedPositionsAsync(DateTime StartDate, DateTime EndDate) =>
            ReadPositionsAsync(StreamStaffedPositionsAsync(StartDate, EndDate));

        public Task<IList<FireManagerStaffedPosition>> GetStaffedPositionsAsync(int Month, int Year) =>
            ReadPositionsAsync(StreamStaffedPositionsAsync(Month, Year));

        private static async Task<IList<FireManagerStaffedPosition>> ReadPositionsAsync(Task<Stream> request)
        {
            var results = await ReadResultsAsync(request);
            var ranges = results.ResultsRanges ?? throw new InvalidDataException("The response is missing ranges.");
            var positions = new List<FireManagerStaffedPosition>();
            foreach (var range in ranges.Range ?? Array.Empty<ResultRange>())
                positions.Add(FireManagerStaffedPosition.Instance(
                    range.Schedule, range.Position, range.Member, range.Begin, range.End));
            return positions;
        }
    }
}
