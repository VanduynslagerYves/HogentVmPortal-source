using HogentVmPortal.Shared.Model;

namespace HogentVmPortal.Shared.ViewModel
{
    public class VirtualMachineListItem : ViewModelBase<VirtualMachineListItem, VirtualMachine>
    {
        public Guid Id { get; private set; }
        public int? ProxmoxId { get; private set; }
        public string? Name { get; private set; }
        public string? IpAddress { get; private set; }
        public string? Login { get; private set; }
        public string? OwnerName { get; private set; }
        public string? OwnerEmail { get; private set; }
        public string? OwnerId { get; private set; }

        public static VirtualMachineListItem ToViewModel(VirtualMachine virtualMachine)
        {
            return new VirtualMachineListItem
            {
                Id = virtualMachine.Id,
                ProxmoxId = virtualMachine.ProxmoxId,
                IpAddress = virtualMachine.IpAddress,
                Name = virtualMachine.Name,
                Login = virtualMachine.Login,
                OwnerName = virtualMachine.Owner.UserName,
                OwnerEmail = virtualMachine.Owner.Email,
                OwnerId = virtualMachine.Owner.Id,
            };
        }

        public static List<VirtualMachineListItem> ToViewModel(List<VirtualMachine> virtualMachines)
        {
            return virtualMachines.Select(ToViewModel).ToList();
        }
    }
}
