namespace HogentVmPortal.Shared.DTO
{
    public class VirtualMachineCreateRequest
    {
        public Guid Id { get; set; }
        public required DateTime TimeStamp { get; set; }
        public required string Name { get; set; }
        public required string OwnerId { get; set; }
        public required string Login { get; set; }
        public required string Password { get; set; }
        public required string SshKey { get; set; }
        public required int CloneId { get; set; }
    }
}
