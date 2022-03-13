using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Homecloud.Models.Devops
{
    public sealed class OrganizationProject
    {
        public Guid Id { get; init; }
        public string Organization { get; init; }
        public string Project { get; init; }
        public string Hash { get; init; }

        public DateTime CreationDate { get; init; }

        public OrganizationProject(Contracts.Messages.CreateOrganizationProject commandMessage)
        {
            this.Id = Guid.NewGuid();
            Organization = commandMessage.Organization;
            Project = commandMessage.Project;
            Hash = commandMessage.Hash;
            CreationDate = DateTime.UtcNow;
        }

        public OrganizationProject(Guid id, string organization, string project)
        {
            Id = id;
            Organization = organization;
            Project = project;
        }

        [JsonIgnore]
        public string ApiUrl
        {
            get
            {
                return $"https://dev.azure.com/{Uri.EscapeDataString(Organization)}/{Uri.EscapeDataString(Project)}/_apis";
            }
        }
    }
}