using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Homecloud
{
    internal class DevopsApiClient
    {
        private readonly string _pat;
        private readonly string _baseUrl;
        private readonly ILogger _logger;

        internal DevopsApiClient(string pat, string baseUrl, ILogger logger)
        {
            _pat = pat;
            _baseUrl = baseUrl;
            _logger = logger;
        }

        internal async Task<IEnumerable<Models.DevopsApi.Pipeline>> ListPipelines()
        {
            var uri = $"{_baseUrl}/pipelines?api-version=6.0-preview.1";
            _logger.LogDebug("URI: {uri}", uri);
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _pat))));

            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            // Read Server Response
            HttpResponseMessage response = await client.SendAsync(request);
            _logger.LogInformation($"Status code: {response.StatusCode} reason: {response.ReasonPhrase}");

            var result = await response.Content.ReadAsAsync<Models.DevopsApi.PipelineResponse>();
            _logger.LogInformation($"Results len: {result.Count}");

            return result.Value;
        }
    }
}