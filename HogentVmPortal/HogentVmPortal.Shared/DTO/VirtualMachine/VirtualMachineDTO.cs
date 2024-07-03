namespace HogentVmPortal.Shared.DTO
{
    public class VirtualMachineDTO
    {
        //[JsonProperty("id")]
        public Guid Id { get; set; }
        public required int ProxmoxId { get; set; }
        public required string Name { get; set; }
        public required string IpAddress { get; set; }
        public required string Login { get; set; }
        public required string OwnerName { get; set; }
        public required string OwnerEmail { get; set; }
        public required string OwnerId { get; set; }
    }
}
