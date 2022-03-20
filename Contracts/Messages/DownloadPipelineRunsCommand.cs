namespace Homecloud.Contracts.Messages
{
    public record DownloadPipelineRunsCommand(string ProjectHash, string ApiUrl, int PipelineId);
}