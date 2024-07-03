namespace HogentVmPortal.Shared.DTO
{
    public class ContainerDTO
    {
        public Guid Id { get; set; }
        public required int ProxmoxId { get; set; }
        public required string Name { get; set; }
        public required string IpAddress { get; set; }
        public required string OwnerName { get; set; }
        public required string OwnerEmail { get; set; }
        public required string OwnerId { get; set; }
    }
}
