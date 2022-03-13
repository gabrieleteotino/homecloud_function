using System;
using System.Security.Cryptography;
using System.Text;

namespace Homecloud.Contracts.Messages
{
    public class ProjectCreatedMessage
    {
        public string Hash { get; init; }

        public ProjectCreatedMessage(string hash)
        {
            Hash = hash;
        }
    }
}