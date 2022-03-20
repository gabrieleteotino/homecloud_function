namespace Homecloud.Contracts.Requests
{
    public record RefreshPipelineRunsRequest(string ProjectHash, string ApiUrl, int PipelineId);
}