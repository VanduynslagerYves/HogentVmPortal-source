using HogentVmPortal.Shared.Model;
using System.ComponentModel.DataAnnotations;

namespace HogentVmPortal.Shared.ViewModel
{
    public class ContainerTemplateDelete : ViewModelBase<ContainerTemplateDelete, ContainerTemplate>
    {
        [Required]
        public Guid Id { get; private set; }
        public string? Name { get; private set; }
        public int ProxmoxId { get; private set; }
        public string? Description { get; private set; }

        public static ContainerTemplateDelete ToViewModel(ContainerTemplate template)
        {
            return new ContainerTemplateDelete
            {
                Id = template.Id,
                Name = template.Name,
                ProxmoxId = template.ProxmoxId,
                Description = template.Description,
            };
        }
    }
}
