using HogentVmPortal.Shared.Model;

namespace VirtualMachineWorker.Data.Repositories
{
    public interface IAppUserRepository
    {
        public Task<HogentUser> GetById(string id);
    }
}
