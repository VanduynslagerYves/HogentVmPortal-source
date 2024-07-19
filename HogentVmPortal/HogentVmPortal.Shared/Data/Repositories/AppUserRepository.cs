using HogentVmPortal.Shared.Data;
using HogentVmPortal.Shared.Model;
using Microsoft.EntityFrameworkCore;

namespace HogentVmPortal.Shared.Repositories
{
    public interface IAppUserRepository
    {
        public Task<HogentUser> GetById(string id);
    }

    public class AppUserRepository : IAppUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<HogentUser> _appUsers;

        public AppUserRepository(ApplicationDbContext context)
        {
            _context = context;
            _appUsers = _context.Users;
        }

        public async Task<HogentUser> GetById(string id)
        {
            var user = await _appUsers.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) throw new KeyNotFoundException();

            return user;
        }
    }
}
