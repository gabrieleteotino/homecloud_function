using System;
using System.Security.Cryptography;
using System.Text;

namespace Homecloud.Contracts.Messages
{
    public class CreateProjectCommand
    {
        public string Organization { get; init; }
        public string Project { get; init; }
        public string Hash { get; init; }

        public CreateProjectCommand(string organization, string project)
        {
            if (string.IsNullOrWhiteSpace(organization)) throw new ArgumentException("Parameter cannot be null", nameof(organization));
            if (string.IsNullOrWhiteSpace(project)) throw new ArgumentException("Parameter cannot be null", nameof(project));

            Organization = organization;
            Project = project;

            using SHA256 hash = SHA256.Create();
            var op = organization + project;
            var byteArray = hash.ComputeHash(Encoding.UTF8.GetBytes(op));
            Hash = Convert.ToHexString(byteArray).ToLower();
        }
    }
}