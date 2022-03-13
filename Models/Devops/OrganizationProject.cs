using System;
using System.Collections.Generic;

namespace Homecloud.Models.Devops
{
    internal class OrganizationProject
    {
        public Guid Id {get; set;}
        public string Organization { get; set; }
        public string Project { get; set; }
    }
}