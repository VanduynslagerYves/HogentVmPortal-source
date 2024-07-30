namespace HogentVmPortal.Shared.DTO
{
    public class ContainerCreateRequest
    {
        public required Guid Id { get; set; }
        public required DateTime TimeStamp { get; set; }
        public required string Name { get; set; }
        public required string OwnerId { get; set; }
        public required int CloneId { get; set; }
    }
}
