using Microsoft.AspNetCore.Identity;

namespace HogentVmPortal.Shared.Model
{
    //Derives from the .NET IdentityUser class, so we can extend this with our own navigation properties
    public class HogentUser : IdentityUser
    {
        public List<VirtualMachine> VirtualMachines { get; } = new List<VirtualMachine>();
        public List<Container> Containers { get; } = new List<Container>();
        public List<Course> Courses { get; } = new List<Course>();
    }
}
