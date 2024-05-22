using Microsoft.AspNetCore.Identity;

namespace HogentVmPortal.Shared.Model
{
    public class HogentUser : IdentityUser
    {
        public List<VirtualMachine> VirtualMachines { get; } = new List<VirtualMachine>();
        public List<Container> Containers { get; } = new List<Container>();
        public List<Course> Courses { get; } = new List<Course>();
    }
}
