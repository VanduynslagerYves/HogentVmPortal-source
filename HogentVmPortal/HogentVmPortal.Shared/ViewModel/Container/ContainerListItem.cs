using HogentVmPortal.Shared.DTO;

namespace HogentVmPortal.Shared.ViewModel
{
    public class ContainerListItem : ViewModelBase<ContainerListItem, ContainerDTO>
    {
        public required Guid Id { get; set; }
        public required int ProxmoxId { get; set; }
        public required string Name { get; set; }
        public required string IpAddress { get; set; }
        public required string OwnerName { get; set; }
        public required string OwnerEmail { get; set; }
        public required string OwnerId { get; set; }

        public static ContainerListItem ToViewModel(ContainerDTO container)
        {
            if(container == null) throw new ArgumentNullException(nameof(container));

            return new ContainerListItem
            {
                Id = container.Id,
                ProxmoxId = container.ProxmoxId,
                IpAddress = container.IpAddress,
                Name = container.Name,
                OwnerName = container.OwnerName,
                OwnerEmail = container.OwnerEmail,
                OwnerId = container.OwnerId,
            };
        }

        public static List<ContainerListItem> ToViewModel(List<ContainerDTO> containers)
        {
            if (containers == null) throw new ArgumentNullException(nameof(containers));
            return containers.Select(ToViewModel).ToList();
        }
    }
}
