using HogentVmPortal.Shared.Model;

namespace VirtualMachineWorker.Data.Repositories
{
    public interface IContainerTemplateRepository
    {
        Task<ContainerTemplate> GetByCloneId(int id);
    }
}
