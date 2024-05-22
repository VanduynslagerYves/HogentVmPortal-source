using HogentVmPortal.Shared.Model;

namespace HogentVmPortal.Shared.ViewModel
{
    public class ContainerListItem : ViewModelBase<ContainerListItem, Container>
    {
        public Guid Id { get; private set; }
        public int? ProxmoxId { get; private set; }
        public string? Name { get; private set; }
        public string? IpAddress { get; private set; }
        public string? OwnerName { get; private set; }
        public string? OwnerEmail { get; private set; }
        public string? OwnerId { get; private set; }

        public static ContainerListItem ToViewModel(Container container)
        {
            return new ContainerListItem
            {
                Id = container.Id,
                ProxmoxId = container.ProxmoxId,
                IpAddress = container.IpAddress,
                Name = container.Name,
                OwnerName = container.Owner.UserName,
                OwnerEmail = container.Owner.Email,
                OwnerId = container.Owner.Id,
            };
        }

        public static List<ContainerListItem> ToViewModel(List<Container> containers)
        {
            return containers.Select(ToViewModel).ToList();
        }
    }
}
