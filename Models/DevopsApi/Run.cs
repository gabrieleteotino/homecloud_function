using System;
using Newtonsoft.Json;

namespace Homecloud.Models.DevopsApi
{
    internal class RunsResponse : ListResponse<Run> { }

    internal enum RunResult
    {
        Canceled,
        Failed,
        Succeeded,
        Unknown
    }

    internal enum RunState
    {
        Canceling,
        Completed,
        InProgress,
        Unknown
    }

    internal class Run
    {
        [JsonProperty(PropertyName = "_links")]
        PipelineLinks Links { get; set; }
        Pipeline Pipeline { get; set; }
        RunState State { get; set; }
        RunResult Result { get; set; }
        DateTime CreatedDate { get; set; }
        DateTime FinishedDate { get; set; }
        string Url { get; set; }
        int Id { get; set; }
        string Name { get; set; }
    }
}