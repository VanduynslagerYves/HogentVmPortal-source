using HogentVmPortal.Shared.Model;

namespace HogentVmPortal.Data.Repositories
{
    public interface IContainerRepository
    {
        public Task<List<Container>> GetAll(bool includeUsers = false);
        public Task<Container> GetById(Guid id, bool includeUsers = false);
        public bool ContainerNameExists(string name);
    }
}
