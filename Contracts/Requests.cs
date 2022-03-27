namespace Homecloud.Contracts.Requests
{
    public record RefreshPipelineRunsRequest(string ProjectHash, string ApiUrl, int PipelineId);
    public record RefreshPipelineRequest(string ProjectHash, string ApiUrl);
}