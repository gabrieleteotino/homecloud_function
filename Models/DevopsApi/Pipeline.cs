using Newtonsoft.Json;

namespace Homecloud.Models.DevopsApi
{
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