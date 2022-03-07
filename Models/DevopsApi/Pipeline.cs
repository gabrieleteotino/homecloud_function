using System.Collections.Generic;
using Newtonsoft.Json;

namespace Models.DevopsApi
{
    internal class ListResponse<T>
    {
        public int Count { get; set; }
        public IEnumerable<T> Value { get; set; }
    }

    internal class PipelineResponse : ListResponse<Pipeline> { }

    internal class Pipeline
    {
        [JsonProperty(PropertyName = "_links")]
        PipelineLinks Links { get; set; }
        PipelineConfiguration Configuration { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Folder { get; set; }
        public string Url { get; set; }
        public int Revision { get; set; }
    }

    internal class PipelineLinks
    {
        public Link Self { get; set; }
        public Link Web { get; set; }
    }

    internal class Link
    {
        public string Href { get; set; }
    }

    internal class PipelineConfiguration
    {
    }
}