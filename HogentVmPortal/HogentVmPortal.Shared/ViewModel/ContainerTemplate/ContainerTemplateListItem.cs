using HogentVmPortal.Shared.Model;

namespace HogentVmPortal.Shared.ViewModel
{
    public class ContainerTemplateListItem : ViewModelBase<ContainerTemplateListItem, ContainerTemplate>
    {
        public Guid Id { get; private set; }
        public required string Name { get; set; }
        public required string? Description { get; set; }
        public int ProxmoxId { get; set; }

        public static ContainerTemplateListItem ToViewModel(ContainerTemplate template)
        {
            return new ContainerTemplateListItem
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description,
                ProxmoxId = template.ProxmoxId,
            };
        }

        public static List<ContainerTemplateListItem> ToViewModel(List<ContainerTemplate> templates)
        {
            return templates.Select(ToViewModel).ToList();
        }
    }
}
