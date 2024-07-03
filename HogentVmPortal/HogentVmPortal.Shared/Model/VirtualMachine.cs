using HogentVmPortal.Shared.DTO;

namespace HogentVmPortal.Shared.Model
{
    public class VirtualMachine
    {
        public Guid Id { get; set; }
        public required int ProxmoxId { get; set; }
        public required string Name { get; set; }
        public required string IpAddress { get; set; }
        public required string Login { get; set; }
        public required HogentUser Owner { get; set; }
        public required VirtualMachineTemplate Template { get; set; }

        public static VirtualMachineDTO ToDTO(VirtualMachine virtualMachine)
        {
            if (virtualMachine == null) throw new ArgumentNullException(nameof(virtualMachine));

            return new VirtualMachineDTO
            {
                Id = virtualMachine.Id,
                ProxmoxId = virtualMachine.ProxmoxId,
                IpAddress = virtualMachine.IpAddress,
                Name = virtualMachine.Name,
                Login = virtualMachine.Login,
                OwnerName = virtualMachine.Owner.UserName!,
                OwnerEmail = virtualMachine.Owner.Email!,
                OwnerId = virtualMachine.Owner.Id,
            };
        }

        public static List<VirtualMachineDTO> ToDTO(List<VirtualMachine> virtualMachines)
        {
            if (virtualMachines == null) throw new ArgumentNullException(nameof(virtualMachines));
            return virtualMachines.Select(ToDTO).ToList();
        }
    }

    public class VirtualMachineNotFoundException : Exception
    {
        public VirtualMachineNotFoundException() { }

        public VirtualMachineNotFoundException(string message) : base(message)
        {
        }
    }
}