namespace Homecloud.Contracts.Messages
{
    public record PipelineUpdatedMessage(string ProjectHash, int Count);
    public record ProjectCreatedMessage(string ProjectHash);
}