using HogentVmPortal.Shared.Model;

namespace VirtualMachineWorker.Data.Repositories
{
    public interface IVirtualMachineTemplateRepository
    {
        Task<VirtualMachineTemplate> GetByCloneId(int id);
    }
}
