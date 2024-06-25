using HogentVmPortal.Shared.Model;

namespace HogentVmPortalWebAPI.Data.Repositories
{
    public interface IVirtualMachineRepository
    {
        Task<VirtualMachine> GetById(Guid id, bool includeUsers = false);
        Task Add(VirtualMachine virtualMachine);
        void Update(VirtualMachine virtualMachine);
        Task Delete(Guid id);
        Task SaveChangesAsync();
    }
}
