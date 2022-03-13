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
    public static class FunctionsOrganizationProject
    {
        [FunctionName("CreateProjectRest")]
        public static DevopsProjectCreating SendCreateCommand(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Project")] CreateDevopsProjectRequest createDevopsProjectRequest,
            [Queue("create-project")] out CreateOrganizationProject createOrganizationProject,
            ILogger logger)
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");
            logger.LogDebug($"Request: {JsonConvert.SerializeObject(createDevopsProjectRequest)}");

            createOrganizationProject = new(createDevopsProjectRequest.Organization, createDevopsProjectRequest.Project);
            logger.LogDebug($"Command: {JsonConvert.SerializeObject(createOrganizationProject)}");

            return new DevopsProjectCreating { Hash = createOrganizationProject.Hash };
        }

        [FunctionName("CreateProject")]
        [return: Queue("project-created")]
        public static async Task<OrganizationProjectCreated> ProcessCreateCommand(
            [QueueTrigger("create-project")] Homecloud.Contracts.Messages.CreateOrganizationProject createOrganizationProject,
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
            var organizationProject = new Models.Devops.OrganizationProject(createOrganizationProject);
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(organizationProject)));
            await blobClient.UploadAsync(ms);
            return new OrganizationProjectCreated(organizationProject.Hash);
        }

        [FunctionName("ProjectCreated")]
        public static async Task ProcessProjectCreatedMessage(
            [QueueTrigger("project-created")] OrganizationProjectCreated organizationProjectCreated,
            [Blob("organization-project/{ProjectHash}.json")] Models.Devops.OrganizationProject organizationProject,
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