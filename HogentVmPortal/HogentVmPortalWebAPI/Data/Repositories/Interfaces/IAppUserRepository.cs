using HogentVmPortal.Shared.Model;

namespace HogentVmPortalWebAPI.Data.Repositories
{
    public interface IAppUserRepository
    {
        public Task<HogentUser> GetById(string id);
    }
}
