namespace Homecloud.Models.Devops
{
    public record Run(string ProjectHash, int PipelineId, int Id, string Name, string State, string Result);
}