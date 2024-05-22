using HogentVmPortal.Shared.Model;
using System.ComponentModel.DataAnnotations;

namespace HogentVmPortal.Shared.ViewModel
{
    public class VirtualMachineEdit : ViewModelBase<VirtualMachineEdit, VirtualMachine>
    {
        [Required]
        public Guid Id { get; private set; }
        public required string Name { get; set; }

        public static VirtualMachineEdit ToViewModel(VirtualMachine virtualMachine)
        {
            return new VirtualMachineEdit
            {
                Id = virtualMachine.Id,
                Name = virtualMachine.Name,
            };
        }
    }
}
