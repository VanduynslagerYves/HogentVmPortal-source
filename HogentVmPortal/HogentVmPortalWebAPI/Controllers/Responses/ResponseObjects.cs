namespace HogentVmPortalWebAPI.Controllers
{
    internal class StatusResponse
    {
        public required string TaskId { get; set; }
        public required string Status { get; set; }
    }

    internal class TaskResponse
    {
        public required string TaskId { get; set; }
    }
}
