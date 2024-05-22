namespace HogentVmPortal.Shared.Model
{
    public class Course
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public List<HogentUser> Students { get; } = new List<HogentUser>();
        public List<VirtualMachineTemplate> VirtualMachineTemplates { get; } = new List<VirtualMachineTemplate>();
        public List<ContainerTemplate> ContainerTemplates { get; } = new List<ContainerTemplate>();
    }

    public class CourseNotFoundException : Exception
    {
        public CourseNotFoundException() { }
        public CourseNotFoundException(string message) : base(message) { }
    }
}
