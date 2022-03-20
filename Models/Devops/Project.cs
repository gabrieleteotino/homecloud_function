using System;
using Homecloud.Contracts.Commands;
using Homecloud.Contracts.Messages;
using Newtonsoft.Json;

namespace Homecloud.Models.Devops
{
    public record Project(Guid Id, string OrganizationName, string ProjectName, string Hash, DateTime CreationDate)
    {
        public int PipelinesCount { get; init; }

        [JsonIgnore]
        public string ApiUrl => $"https://dev.azure.com/{Uri.EscapeDataString(OrganizationName)}/{Uri.EscapeDataString(ProjectName)}/_apis";
    };

    public static class ProjectFactory
    {
        public static Project CreateProjectFromCommand(CreateProjectCommand commandMessage) =>
            new(Id: Guid.NewGuid(),
                OrganizationName: commandMessage.Organization,
                ProjectName: commandMessage.Project,
                Hash: commandMessage.CalculateProjectHash(),
                CreationDate: DateTime.UtcNow);
    }

    public static class ProjectExtensions
    {
        public static Project ProcessPipelineUpdated(this Project project, PipelineUpdatedMessage message) =>
            project with
            {
                PipelinesCount = message.Count
            };
    }
}