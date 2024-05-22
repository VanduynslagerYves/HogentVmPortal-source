using HogentVmPortal.Shared.Model;
using System.ComponentModel.DataAnnotations;

namespace HogentVmPortal.Shared.ViewModel
{
    public class VirtualMachineTemplateEdit : ViewModelBase<VirtualMachineTemplateEdit, VirtualMachineTemplate>
    {
        [Required]
        public Guid Id { get; private set; }
        [Required]
        public required string Name { get; set; }
        [Required]
        public required string OperatingSystem { get; set; }
        [Required]
        [Range(100, 999)]
        public int ProxmoxId { get; set; }
        public required string? Description { get; set; }

        public static VirtualMachineTemplateEdit ToViewModel(VirtualMachineTemplate model)
        {
            return new VirtualMachineTemplateEdit
            {
                Id = model.Id,
                Name = model.Name,
                OperatingSystem = model.OperatingSystem,
                Description = model.Description,
                ProxmoxId = model.ProxmoxId,
            };
        }
    }
}
