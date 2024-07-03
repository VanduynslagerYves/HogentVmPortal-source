using HogentVmPortal.Shared.DTO;

namespace HogentVmPortal.Shared.Model
{
    public class Container
    {
        public Guid Id { get; set; }
        public required int ProxmoxId { get; set; }
        public required string Name { get; set; }
        public required string IpAddress { get; set; }
        public required HogentUser Owner { get; set; }
        public required ContainerTemplate Template { get; set; }

        public static ContainerDTO ToDTO(Container container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            return new ContainerDTO
            {
                Id = container.Id,
                ProxmoxId = container.ProxmoxId,
                IpAddress = container.IpAddress,
                Name = container.Name,
                OwnerName = container.Owner.UserName!,
                OwnerEmail = container.Owner.Email!,
                OwnerId = container.Owner.Id,
            };
        }

        public static List<ContainerDTO> ToDTO(List<Container> containers)
        {
            if (containers == null) throw new ArgumentNullException(nameof(containers));
            return containers.Select(ToDTO).ToList();
        }
    }

    public class ContainerNotFoundException : Exception
    {
        public ContainerNotFoundException() { }

        public ContainerNotFoundException(string message) : base(message)
        {
        }
    }
}
