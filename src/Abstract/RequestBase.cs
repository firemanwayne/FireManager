using FireManager.Concrete;
using FireManager.Extensions;
using FireManager.Interface;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace FireManager.Abstract
{
    public abstract class RequestBase
    {
        protected readonly IRequests Requests;
        protected readonly IHttpClientFactory Factory;
        protected readonly FireManagerOptions Options;

        protected RequestBase(
            IRequests Requests,
            IHttpClientFactory Factory,
            IOptions<FireManagerOptions> Options)
        {
            this.Requests = Requests;
            this.Options = Options.Value;
            this.Factory = Factory;
        }

        protected async Task<Stream> SendAsync(IDictionary<string, string> values)
        {
            using var client = Factory.CreateClient();
            using var message = CreatePostMessage(Options.Url, new FormUrlEncodedContent(values));
            var response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead);
            try
            {
                response.EnsureSuccessStatusCode();
                return new ResponseStream(await response.Content.ReadAsStreamAsync(), response);
            }
            catch
            {
                response.Dispose();
                throw;
            }
        }

        protected static async Task<Results> ReadResultsAsync(Task<Stream> request)
        {
            using var stream = await request;
            using var reader = XmlReader.Create(stream);
            var results = (Results)new XmlSerializer(typeof(Results)).Deserialize(reader);
            if (results == null)
                throw new InvalidDataException("The FireManager response was empty.");
            if (results.Error != null)
                throw new InvalidDataException($"FireManager API error {results.Error.Code}: {results.Error.Value}");
            return results;
        }

        // The public streaming methods transfer ownership of the HTTP response to the caller.
        private sealed class ResponseStream : Stream
        {
            private readonly Stream stream;
            private readonly HttpResponseMessage response;

            public ResponseStream(Stream stream, HttpResponseMessage response)
            {
                this.stream = stream;
                this.response = response;
            }

            public override bool CanRead => stream.CanRead;
            public override bool CanSeek => stream.CanSeek;
            public override bool CanWrite => stream.CanWrite;
            public override long Length => stream.Length;
            public override long Position { get => stream.Position; set => stream.Position = value; }
            public override void Flush() => stream.Flush();
            public override int Read(byte[] buffer, int offset, int count) => stream.Read(buffer, offset, count);
            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
                => stream.ReadAsync(buffer, offset, count, cancellationToken);
            public override long Seek(long offset, SeekOrigin origin) => stream.Seek(offset, origin);
            public override void SetLength(long value) => stream.SetLength(value);
            public override void Write(byte[] buffer, int offset, int count) => stream.Write(buffer, offset, count);
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    response.Dispose();
                base.Dispose(disposing);
            }
        }

        public static HttpRequestMessage CreatePostMessage(string Path, FormUrlEncodedContent Content)
        {
            return new HttpRequestMessage()
            {
                RequestUri = new Uri(Path),
                Method = HttpMethod.Post,
                Content = Content
            };
        }
    }

    public abstract class RequestBase<T> : RequestBase
    {
        protected RequestBase(
            IRequests Requests,
            IHttpClientFactory Factory,
            IOptions<FireManagerOptions> Options) : base(Requests, Factory, Options)
        { }
    }
}
