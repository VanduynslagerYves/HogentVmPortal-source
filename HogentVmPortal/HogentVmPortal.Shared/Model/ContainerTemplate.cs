namespace HogentVmPortal.Shared.Model
{
    public class ContainerTemplate
    {
        public Guid Id { get; set; }
        public required int ProxmoxId { get; set; }
        public required string Name { get; set; }
        public required string? Description { get; set; }
        public List<Container> Containers { get; } = new List<Container>();
        public List<Course> Courses { get; } = new List<Course>();
    }

    public class ContainerTemplateNotFoundException : Exception
    {
        public ContainerTemplateNotFoundException() { }
        public ContainerTemplateNotFoundException(string message) : base(message) { }
    }
}
