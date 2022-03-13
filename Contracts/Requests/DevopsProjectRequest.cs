namespace Homecloud.Contracts.Requests
{
    public class DevopsProjectRequest
    {
        public string Organization { get; set; }
        public string Project { get; set; }

        public DevopsProjectRequest()
        {

        }
    }
}