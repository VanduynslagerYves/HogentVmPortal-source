using HogentVmPortal.Shared.DTO;

namespace HogentVmPortal.Shared.ViewModel
{
    public class VirtualMachineListItem : ViewModelBase<VirtualMachineListItem, VirtualMachineDTO>
    {
        public required Guid Id { get; set; }
        public required int ProxmoxId { get; set; }
        public required string Name { get; set; }
        public required string IpAddress { get; set; }
        public required string Login { get; set; }
        public required string OwnerName { get; set; }
        public required string OwnerEmail { get; set; }
        public required string OwnerId { get; set; }

        public static VirtualMachineListItem ToViewModel(VirtualMachineDTO virtualMachine)
        {
            if(virtualMachine == null) throw new ArgumentNullException(nameof(virtualMachine));

            return new VirtualMachineListItem
            {
                Id = virtualMachine.Id,
                ProxmoxId = virtualMachine.ProxmoxId,
                IpAddress = virtualMachine.IpAddress,
                Name = virtualMachine.Name,
                Login = virtualMachine.Login,
                OwnerName = virtualMachine.OwnerName,
                OwnerEmail = virtualMachine.OwnerEmail,
                OwnerId = virtualMachine.OwnerId,
            };
        }

        public static List<VirtualMachineListItem> ToViewModel(List<VirtualMachineDTO> virtualMachines)
        {
            if(virtualMachines == null) throw new ArgumentNullException(nameof (virtualMachines));
            return virtualMachines.Select(ToViewModel).ToList();
        }
    }
}
