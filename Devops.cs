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
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            if (data.organization is null || data.project is null)
            {
                logger.LogError("Invalid request. Request body: {RequestBody}", requestBody);
                return new BadRequestResult();
            }

            var pipes = await ListPipelines(data.organization, data.project, logger);
            return new OkObjectResult(pipes);
        }

        private static async Task<List<string>> ListPipelines(string organization, string project, ILogger logger)
        {
            var personalaccesstoken = Environment.GetEnvironmentVariable("DevOpsPat", EnvironmentVariableTarget.Process);
            logger.LogInformation(personalaccesstoken);
            var uri = $"GET https://dev.azure.com/{organization}/{project}/_apis/pipelines?api-version=6.0-preview.1";
            logger.LogInformation(uri);
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", personalaccesstoken))));

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);

            //Read Server Response
            HttpResponseMessage response = await client.SendAsync(request);
            logger.LogInformation($"Status code: {response.StatusCode} reason: {response.ReasonPhrase}");
            var results = await response.Content.ReadAsAsync<List<dynamic>>();
            logger.LogInformation($"Results len: {results.Count}");
            return new List<string>();
        }
    }
}
