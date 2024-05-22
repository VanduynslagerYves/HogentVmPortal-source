namespace HogentVmPortal.Shared.Model
{
    public class Container
    {
        public Guid Id { get; set; }
        public int? ProxmoxId { get; set; }
        public required string Name { get; set; }
        public string? IpAddress { get; set; }
        public required HogentUser Owner { get; set; }
        public required ContainerTemplate Template { get; set; }
    }

    public class ContainerNotFoundException : Exception
    {
        public ContainerNotFoundException() { }

        public ContainerNotFoundException(string message) : base(message)
        {
        }
    }
}
