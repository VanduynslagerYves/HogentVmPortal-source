using HogentVmPortal.Shared.Model;

namespace HogentVmPortalWebAPI.Data.Repositories
{
    public interface IContainerRepository
    {
        Task<Container> GetById(Guid id, bool includeUsers = false);
        Task Add(Container container);
        void Update(Container container);
        Task Delete(Guid id);
        Task SaveChangesAsync();
    }
}
