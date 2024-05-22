using HogentVmPortal.Shared.Model;

namespace HogentVmPortal.Shared.ViewModel
{
    public class VirtualMachineTemplateListItem : ViewModelBase<VirtualMachineTemplateListItem, VirtualMachineTemplate>
    {
        public Guid Id { get; private set; }
        public required string Name { get; set; }
        public required string OperatingSystem { get; set; }
        public required string? Description { get; set; }
        public int ProxmoxId { get; set; }
        public string FullDescription => $"{Name} - {OperatingSystem} - {Description}";

        public static VirtualMachineTemplateListItem ToViewModel(VirtualMachineTemplate template)
        {
            return new VirtualMachineTemplateListItem
            {
                Id = template.Id,
                Name = template.Name,
                OperatingSystem = template.OperatingSystem,
                Description = template.Description,
                ProxmoxId = template.ProxmoxId,
            };
        }

        public static List<VirtualMachineTemplateListItem> ToViewModel(List<VirtualMachineTemplate> templates)
        {
            return templates.Select(ToViewModel).ToList();
        }
    }
}
