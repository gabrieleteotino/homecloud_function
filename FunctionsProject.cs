using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Homecloud.Contracts.Requests;
using Homecloud.Contracts.Messages;
using System.Text;
using System;

namespace Homecloud
{
    [StorageAccount("DataStorage")]
    public static class FunctionsProject
    {
        [FunctionName("CreateProjectRest")]
        public static DevopsProjectCreatingResponse SendCreateCommand(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Project")] CreateDevopsProjectRequest createDevopsProjectRequest,
            [Queue("create-project")] out CreateProjectCommand createOrganizationProject,
            ILogger logger)
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");
            logger.LogDebug($"Request: {JsonConvert.SerializeObject(createDevopsProjectRequest)}");

            createOrganizationProject = new(createDevopsProjectRequest.Organization, createDevopsProjectRequest.Project);
            logger.LogDebug($"Command: {JsonConvert.SerializeObject(createOrganizationProject)}");

            return new DevopsProjectCreatingResponse { Hash = createOrganizationProject.Hash };
        }

        [FunctionName("CreateProject")]
        [return: Queue("project-created")]
        public static async Task<ProjectCreatedMessage> ProcessCreateCommand(
            [QueueTrigger("create-project")] Homecloud.Contracts.Messages.CreateProjectCommand createOrganizationProject,
            [Blob("organization-project/{Hash}.json", FileAccess.ReadWrite)] Azure.Storage.Blobs.Specialized.BlockBlobClient blobClient,
            ILogger logger
        )
        {
            logger.LogInformation("C# Queue trigger processed a command");
            if (await blobClient.ExistsAsync())
            {
                throw new InvalidOperationException("The blob already exists");
            }
            logger.LogDebug("Creating organizationproject entity");
            var organizationProject = new Models.Devops.Project(createOrganizationProject);
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(organizationProject)));
            await blobClient.UploadAsync(ms);
            return new ProjectCreatedMessage(organizationProject.Hash);
        }

        [FunctionName("ProjectCreated")]
        public static async Task ProcessProjectCreatedMessage(
            [QueueTrigger("project-created")] ProjectCreatedMessage organizationProjectCreated,
            [Blob("organization-project/{ProjectHash}.json")] Models.Devops.Project organizationProject,
            [Queue("update-pipelines")] IAsyncCollector<UpdatePipelines> pipelineMessages,
            ILogger logger
        )
        {
            logger.LogInformation("C# Queue trigger processing a command");
            var pipelineMessage = new UpdatePipelines(organizationProjectCreated.Hash, organizationProject.ApiUrl);
            await pipelineMessages.AddAsync(pipelineMessage);
        }
    }
}