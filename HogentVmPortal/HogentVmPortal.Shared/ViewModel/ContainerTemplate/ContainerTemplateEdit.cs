using HogentVmPortal.Shared.Model;
using System.ComponentModel.DataAnnotations;

namespace HogentVmPortal.Shared.ViewModel
{
    public class ContainerTemplateEdit : ViewModelBase<ContainerTemplateEdit, ContainerTemplate>
    {
        [Required]
        public Guid Id { get; private set; }
        [Required]
        public required string Name { get; set; }
        [Required]
        [Range(100, 999)]
        public int ProxmoxId { get; set; }
        public required string? Description { get; set; }

        public static ContainerTemplateEdit ToViewModel(ContainerTemplate model)
        {
            return new ContainerTemplateEdit
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                ProxmoxId = model.ProxmoxId,
            };
        }
    }
}
