using HogentVmPortal.Shared.Model;
using System.ComponentModel.DataAnnotations;

namespace HogentVmPortal.Shared.ViewModel
{
    public class VirtualMachineDelete : ViewModelBase<VirtualMachineDelete, VirtualMachine>
    {
        [Required]
        public Guid Id { get; set; }
        public string? Name { get; private set; }

        public static VirtualMachineDelete ToViewModel(VirtualMachine virtualMachine)
        {
            return new VirtualMachineDelete
            {
                Id = virtualMachine.Id,
                Name = virtualMachine.Name
            };
        }
    }
}
