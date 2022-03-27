namespace Homecloud.Contracts.Requests
{
    public record RefreshPipelineRunsRequest(string ProjectHash, string ApiUrl, int PipelineId);
    public record UpdatePipelineRequest(string ProjectHash, string ApiUrl);
}