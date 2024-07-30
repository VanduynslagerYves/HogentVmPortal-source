namespace HogentVmPortal.Shared.Model
{
    public class Request
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
        public required InfraType Type { get; set; }
        public required RequestType RequestType{ get; set; }
        public required Status Status { get; set; }
        public required string RequesterId { get; set; }
        public required DateTime TimeStamp { get; set; }
    }

    public enum InfraType
    {
        Container,
        VirtualMachine,
    }

    public enum RequestType
    {
        Create,
        Remove,
    }

    public enum Status
    {
        Pending,
        Complete,
        Failed,
    }

    public class RequestNotFoundException : Exception
    {
        public RequestNotFoundException() { }

        public RequestNotFoundException(string message) : base(message)
        {
        }
    }
}
