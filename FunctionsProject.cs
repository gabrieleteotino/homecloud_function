using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Homecloud.Contracts.Commands;
using Homecloud.Contracts.Messages;
using System.Text;
using System;
using Microsoft.AspNetCore.Mvc;

namespace Homecloud
{
    [StorageAccount("DataStorage")]
    public static class FunctionsProject
    {
        [FunctionName("CreateProjectRest")]
        public static IActionResult SendCreateProjectCommand(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Project")] CreateProjectCommand createProjectCommand,
            [Queue("create-project")] out CreateProjectCommand message,
            ILogger logger)
        {
            logger.LogInformation("SendCreateProjectCommand - C# HTTP trigger function processed a request.");
            logger.LogDebug($"Request: {JsonConvert.SerializeObject(createProjectCommand)}");

            message = createProjectCommand;

            return new OkResult();
        }

        [FunctionName("CreateProject")]
        [return: Queue("project-created")]
        public static async Task<ProjectCreatedMessage> ProcessCreateProjectCommand(
            [QueueTrigger("create-project")] CreateProjectCommand createProject,
            IBinder binder,
            //[Blob("projects/{Hash}.json", FileAccess.ReadWrite)] Azure.Storage.Blobs.Specialized.BlockBlobClient blobClient,
            ILogger logger
        )
        {
            logger.LogInformation("ProcessCreateProjectCommand C# Queue trigger processed a command");
            var blobClient = await binder.BindAsync<Azure.Storage.Blobs.BlobClient>(new BlobAttribute($"projects/{createProject.CalculateProjectHash()}", FileAccess.ReadWrite));

            if (await blobClient.ExistsAsync())
            {
                throw new InvalidOperationException("The blob already exists");
            }
            logger.LogDebug("Creating organizationproject entity");
            var organizationProject = Models.Devops.ProjectFactory.CreateProjectFromCommand(createProject);
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(organizationProject)));
            await blobClient.UploadAsync(ms);
            return new ProjectCreatedMessage(organizationProject.Hash);
        }

        [FunctionName("ProjectCreated")]
        public static async Task ProcessProjectCreatedMessage(
            [QueueTrigger("project-created")] ProjectCreatedMessage projectCreated,
            [Blob("projects/{ProjectHash}.json", FileAccess.Read)] string projectString,
            [Queue("refresh-pipelines")] IAsyncCollector<RefreshPipelinesCommand> pipelineMessages,
            ILogger logger
        )
        {
            logger.LogInformation("ProcessProjectCreatedMessage - C# Queue trigger processing a command");
            var project = JsonConvert.DeserializeObject<Models.Devops.Project>(projectString);
            if (projectCreated.ProjectHash != project.Hash) throw new InvalidDataException($"Project blob {projectCreated.ProjectHash}.json contains a project with invalid hash");

            var pipelineMessage = new RefreshPipelinesCommand(project.Hash, project.ApiUrl);
            await pipelineMessages.AddAsync(pipelineMessage);
        }
    }
}