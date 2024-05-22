using HogentVmPortal.Shared.Model;

namespace HogentVmPortal.Data.Repositories
{
    public interface IAppUserRepository
    {
        public Task<List<HogentUser>> GetAll();
        public Task<HogentUser> GetById(string id);
    }
}
