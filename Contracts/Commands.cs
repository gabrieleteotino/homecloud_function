using System;
using System.Security.Cryptography;
using System.Text;

namespace Homecloud.Contracts.Commands
{
    public static class Extensions
    {
        public static string CalculateProjectHash(this CreateProjectCommand createProjectCommand)
        {
            using SHA256 hash = SHA256.Create();
            var op = createProjectCommand.Organization + createProjectCommand.Project;
            var byteArray = hash.ComputeHash(Encoding.UTF8.GetBytes(op));
            return Convert.ToHexString(byteArray).ToLower();
        }
    }

    public record CreateProjectCommand(string Organization, string Project);
    public record RefreshPipelinesCommand(string ProjectHash, string ApiUrl);
    public record RefreshPipelineRunsCommand(string ProjectHash, string ApiUrl, int PipelineId);
    public record RefreshPipelineRunCommand(string ProjectHash, string ApiUrl, int PipelineId, int RunId);
}