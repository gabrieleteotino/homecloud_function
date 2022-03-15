namespace Homecloud.Contracts.Messages
{
    public class UpdatePipelinesCommand
    {
        public string ProjectHash { get; init; }
        public string ApiUrl { get; init; }

        public UpdatePipelinesCommand(string projectHash, string apiUrl)
        {
            ProjectHash = projectHash;
            ApiUrl = apiUrl;
        }
    }
}