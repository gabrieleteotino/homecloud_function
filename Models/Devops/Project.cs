using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Homecloud.Models.Devops
{
    public sealed class Project
    {
        public Guid Id { get; init; }
        public string OrganizationName { get; init; }
        public string ProjectName { get; init; }
        public string Hash { get; init; }

        public DateTime CreationDate { get; init; }

        public Project(Contracts.Messages.CreateProjectCommand commandMessage)
        {
            this.Id = Guid.NewGuid();
            OrganizationName = commandMessage.Organization;
            ProjectName = commandMessage.Project;
            Hash = commandMessage.Hash;
            CreationDate = DateTime.UtcNow;
        }

        public Project(Guid id, string organization, string project)
        {
            Id = id;
            OrganizationName = organization;
            ProjectName = project;
        }

        [JsonIgnore]
        public string ApiUrl
        {
            get
            {
                return $"https://dev.azure.com/{Uri.EscapeDataString(OrganizationName)}/{Uri.EscapeDataString(ProjectName)}/_apis";
            }
        }
    }
}