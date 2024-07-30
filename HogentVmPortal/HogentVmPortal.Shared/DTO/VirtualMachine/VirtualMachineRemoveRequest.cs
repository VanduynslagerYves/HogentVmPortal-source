namespace HogentVmPortal.Shared.DTO
{
    public class VirtualMachineRemoveRequest
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
        public required DateTime TimeStamp { get; set; }
        public required Guid VmId { get; set; }
        public required string OwnerId { get; set; }
    }
}
