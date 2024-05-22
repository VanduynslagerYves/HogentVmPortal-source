using HogentVmPortal.Shared.Model;

namespace HogentVmPortal.Data.Repositories
{
    public interface IVirtualMachineRepository
    {
        public Task<List<VirtualMachine>> GetAll(bool includeUsers = false);
        public Task<VirtualMachine> GetById(Guid id, bool includeUsers = false);
        public bool VirtualMachineNameExists(string name);
    }
}
