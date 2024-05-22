namespace HogentVmPortal.Shared.DTO
{
    public class VirtualMachineRemoveRequest
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required DateTime TimeStamp { get; set; }
        public required Guid VmId { get; set; }
    }
}
