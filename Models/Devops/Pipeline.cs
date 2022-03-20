using System;

namespace Homecloud.Models.Devops
{
    public sealed class Pipeline
    {
        public string Id { get; init; }
        public DateTime? LastRefresh { get; init; }
    }
}