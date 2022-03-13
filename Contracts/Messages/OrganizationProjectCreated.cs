using System;
using System.Security.Cryptography;
using System.Text;

namespace Homecloud.Contracts.Messages
{
    public class OrganizationProjectCreated
    {
        public string Hash { get; init; }

        public OrganizationProjectCreated(string hash)
        {
            Hash = hash;
        }
    }
}