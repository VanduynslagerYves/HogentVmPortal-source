using HogentVmPortal.Shared.Model;

namespace HogentVmPortalWebAPI.Data.Repositories
{
    public interface IContainerTemplateRepository
    {
        Task<ContainerTemplate> GetByCloneId(int id);
    }
}
