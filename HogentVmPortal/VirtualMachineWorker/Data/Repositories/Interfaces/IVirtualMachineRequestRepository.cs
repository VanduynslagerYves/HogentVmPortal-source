using HogentVmPortal.Shared.DTO;

namespace VirtualMachineWorker.Data.Repositories
{
    public interface IVirtualMachineRequestRepository
    {
        Task<List<VirtualMachineCreateRequest>> GetAllCreateRequests();
        Task<List<VirtualMachineRemoveRequest>> GetAllRemoveRequests();

        void Delete(VirtualMachineCreateRequest request);
        void Delete(VirtualMachineRemoveRequest request);

        Task SaveChangesAsync();
    }
}
