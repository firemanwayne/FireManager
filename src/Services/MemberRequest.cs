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
    internal class MemberRequest : RequestBase, IMemberRequest
    {
        public MemberRequest(IRequests requests, IHttpClientFactory factory, IOptions<FireManagerOptions> options)
            : base(requests, factory, options) { }

        public Task<Stream> StreamMembersAsync(bool ActiveOnly) =>
            SendAsync(ActiveOnly ? Requests.AllActiveMembersRequest : Requests.AllMembersRequest);

        public async IAsyncEnumerable<FireManagerMember> GetMembersAsync(bool IsActive)
        {
            var results = await ReadResultsAsync(StreamMembersAsync(IsActive));
            var members = results.Members ?? throw new InvalidDataException("The response is missing members.");
            foreach (var member in members.Member ?? Array.Empty<Member>())
                yield return member;
        }
    }
}
