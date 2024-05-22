namespace HogentVmPortal.Shared.DTO
{
    public class ContainerEditRequest
    {
        public Guid Id { get; set; }
        public required DateTime TimeStamp { get; set; }
        public Guid VmId { get; set; }
    }
}
