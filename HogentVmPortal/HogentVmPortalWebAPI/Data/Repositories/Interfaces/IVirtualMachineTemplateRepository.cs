using HogentVmPortal.Shared.Model;

namespace HogentVmPortalWebAPI.Data.Repositories
{
    public interface IVirtualMachineTemplateRepository
    {
        Task<VirtualMachineTemplate> GetByCloneId(int id);
    }
}
