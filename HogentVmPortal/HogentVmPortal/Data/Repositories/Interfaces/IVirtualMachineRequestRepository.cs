using HogentVmPortal.Shared.DTO;

namespace HogentVmPortal.Data.Repositories
{
    public interface IVirtualMachineRequestRepository
    {
        Task Add(VirtualMachineCreateRequest request);
        Task Add(VirtualMachineRemoveRequest request);
        //Task Add(VirtualMachineEditRequest request);
        Task SaveChangesAsync();
    }
}
