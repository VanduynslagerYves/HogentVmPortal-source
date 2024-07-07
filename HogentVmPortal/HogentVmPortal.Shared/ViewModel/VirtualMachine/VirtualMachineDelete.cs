using HogentVmPortal.Shared.DTO;
using System.ComponentModel.DataAnnotations;

namespace HogentVmPortal.Shared.ViewModel
{
    public class VirtualMachineDelete : ViewModelBase<VirtualMachineDelete, VirtualMachineDTO>
    {
        [Required]
        public Guid Id { get; set; }
        public string? Name { get; private set; }

        public static VirtualMachineDelete ToViewModel(VirtualMachineDTO virtualMachine)
        {
            return new VirtualMachineDelete
            {
                Id = virtualMachine.Id,
                Name = virtualMachine.Name
            };
        }
    }
}
