using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace homecloud_function
{
    public static class Devops
    {
        [FunctionName("Devops")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger logger)
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            logger.LogDebug("Request body: {body}", requestBody);
            var data = JsonConvert.DeserializeObject<DevopsProjectRequest>(requestBody);

            if (data.Organization is null || data.Project is null)
            {
                logger.LogError("Invalid request. Request body: {RequestBody}", requestBody);
                return new BadRequestResult();
            }

            var pipes = await ListPipelines(data.Organization, data.Project, logger);
            return new OkObjectResult(pipes);
        }

        private static async Task<IEnumerable<Models.DevopsApi.Pipeline>> ListPipelines(string organization, string project, ILogger logger)
        {
            var personalaccesstoken = Environment.GetEnvironmentVariable("DevOpsPat", EnvironmentVariableTarget.Process);
            logger.LogDebug("PAT: {pat}", personalaccesstoken);
            var uri = $"https://dev.azure.com/{Uri.EscapeDataString(organization)}/{Uri.EscapeDataString(project)}/_apis/pipelines?api-version=6.0-preview.1";
            logger.LogDebug("URI: {uri}", uri);
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", personalaccesstoken))));

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);

            // Read Server Response
            HttpResponseMessage response = await client.SendAsync(request);
            logger.LogInformation($"Status code: {response.StatusCode} reason: {response.ReasonPhrase}");
            var result = await response.Content.ReadAsAsync<Models.DevopsApi.PipelineResponse>();
            logger.LogInformation($"Results len: {result.Count}");

            var list = result.Value;
            return list;
        }

    }
}
