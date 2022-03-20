using System;

namespace Homecloud.Models.Devops
{
    public record Pipeline
    {
        public string Id { get; init; }
        public DateTime? LastRefresh { get; init; }
    }
}