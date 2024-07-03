using HogentVmPortal.Shared.Data;
using HogentVmPortal.Shared.Model;
using Microsoft.EntityFrameworkCore;

namespace HogentVmPortal.Data.Repositories
{
    public interface IAppUserRepository
    {
        public Task<List<HogentUser>> GetAll();
        public Task<HogentUser> GetById(string id);
    }

    public class AppUserRepository : IAppUserRepository
    {
        private readonly DbSet<HogentUser> _appUsers;
        private readonly ApplicationDbContext _context;

        public AppUserRepository(ApplicationDbContext context)
        {
            _context = context;
            _appUsers = _context.Users;
        }

        public async Task<List<HogentUser>> GetAll()
        {
            return await _appUsers.ToListAsync();
        }

        public async Task<HogentUser> GetById(string id)
        {
            var user = await _appUsers.FirstOrDefaultAsync(u => u.Id == id);
            return user ?? throw new KeyNotFoundException();
        }
    }
}
