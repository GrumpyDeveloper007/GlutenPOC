using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Frodo.Service
{
    public class HttpService
    {
        private readonly HttpClient _client;

        public HttpService()
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.All
            };

            _client = new HttpClient();
        }

        public async Task<string> GetAsync2(string url, CancellationToken token = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
            request.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml");
            request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
            request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
            request.Headers.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");

            using var response = await _client.SendAsync(request, token).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            await using var responseStream = await response.Content.ReadAsStreamAsync(token).ConfigureAwait(false);
            await using var decompressedStream = new GZipStream(responseStream, CompressionLevel.Optimal);
            using var streamReader = new StreamReader(decompressedStream);
            return await streamReader.ReadToEndAsync(token).ConfigureAwait(false);
        }

        public async Task<string> GetAsync(string uri)
        {
            using HttpResponseMessage response = await _client.GetAsync(uri);

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> PostAsync(string uri, string data, string contentType)
        {
            using HttpContent content = new StringContent(data, Encoding.UTF8, contentType);

            HttpRequestMessage requestMessage = new HttpRequestMessage()
            {
                Content = content,
                Method = HttpMethod.Post,
                RequestUri = new Uri(uri),
            };

            using HttpResponseMessage response = await _client.SendAsync(requestMessage);

            return await response.Content.ReadAsStringAsync();
        }
    }
}
