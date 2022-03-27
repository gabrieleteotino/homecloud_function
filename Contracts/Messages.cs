namespace Homecloud.Contracts.Messages
{
    public record ProjectCreatedMessage(string ProjectHash);
    public record PipelineRefreshedMessage(string ProjectHash, int Count);
}