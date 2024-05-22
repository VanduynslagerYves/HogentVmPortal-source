namespace HogentVmPortal.Shared.Model
{
    public class VirtualMachineTemplate
    {
        public Guid Id { get; set; }
        public required int ProxmoxId { get; set; }
        public required string Name { get; set; }
        public required string OperatingSystem { get; set; }
        public required string? Description { get; set; }
        public List<VirtualMachine> VirtualMachines { get; } = new List<VirtualMachine>();
        public List<Course> Courses { get; } = new List<Course>();
    }

    public class VirtualMachineTemplateNotFoundException : Exception
    {
        public VirtualMachineTemplateNotFoundException() { }
        public VirtualMachineTemplateNotFoundException(string message) : base(message) { }
    }
}
