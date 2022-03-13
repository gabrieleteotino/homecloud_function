namespace Homecloud.Contracts.Messages
{
    public class UpdatePipelines
    {
        public string ProjectHash { get; init; }
        public string ApiUrl { get; init; }

        public UpdatePipelines(string projectHash, string apiUrl)
        {
            ProjectHash = projectHash;
            ApiUrl = apiUrl;
        }
    }
}