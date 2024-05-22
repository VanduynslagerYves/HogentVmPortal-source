namespace HogentVmPortal.Shared.Model
{
    public class VirtualMachine
    {
        public Guid Id { get; set; }
        public int? ProxmoxId { get; set; }
        public required string Name { get; set; }
        public string? IpAddress { get; set; }
        public string? Login { get; set; }
        public required HogentUser Owner { get; set; }
        public required VirtualMachineTemplate Template { get; set; }
    }

    public class VirtualMachineNotFoundException : Exception
    {
        public VirtualMachineNotFoundException() { }

        public VirtualMachineNotFoundException(string message) : base(message)
        {
        }
    }
}