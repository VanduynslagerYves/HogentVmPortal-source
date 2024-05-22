namespace HogentVmPortal.Shared.DTO
{
    public class VirtualMachineEditRequest
    {
        public Guid Id { get; set; }
        public required DateTime TimeStamp { get; set; }
        public Guid VmId { get; set; }
    }
}
