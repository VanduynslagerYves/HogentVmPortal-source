using HogentVmPortal.Shared.Model;
using System.ComponentModel.DataAnnotations;

namespace HogentVmPortal.Shared.ViewModel
{
    public class VirtualMachineTemplateDelete : ViewModelBase<VirtualMachineTemplateDelete, VirtualMachineTemplate>
    {
        [Required]
        public Guid Id { get; private set; }
        public string? Name { get; private set; }
        public int ProxmoxId { get; private set; }
        public string? Description { get; private set; }

        public static VirtualMachineTemplateDelete ToViewModel(VirtualMachineTemplate template)
        {
            return new VirtualMachineTemplateDelete
            {
                Id = template.Id,
                Name = template.Name,
                ProxmoxId = template.ProxmoxId,
                Description = template.Description,
            };
        }
    }
}
