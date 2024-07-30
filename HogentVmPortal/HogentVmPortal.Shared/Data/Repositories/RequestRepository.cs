using HogentVmPortal.Shared.Data;
using HogentVmPortal.Shared.Model;
using Microsoft.EntityFrameworkCore;

namespace HogentVmPortal.Shared.Repositories
{
    public interface IRequestRepository
    {
        Task<bool> NameExistsAsync(string name);
        Task<Request?> GetById(Guid id);
        Task<List<RequestDbContext>> GetByUserId(Guid userId);
        Task Add(Request request);
        Task Update(Request request);
        Task Delete(Guid id);
    }
    public class RequestRepository : IRequestRepository
    {
        private readonly RequestDbContext _context;
        private readonly DbSet<Request> _requests;

        public RequestRepository(RequestDbContext context)
        {
            _context = context;
            _requests = _context.Requests;
        }

        public async Task<Request?> GetById(Guid id)
        {
            var request = await _requests.SingleOrDefaultAsync(x => x.Id == id);
            return request;
        }

        public async Task Add(Request request)
        {
            await _requests.AddAsync(request);
            await SaveChangesAsync();
        }

        public async Task Update(Request request)
        {
            _requests.Update(request);
            await SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            var request = await _requests.SingleOrDefaultAsync(x => x.Id == id);

            if (request != null)
            {
                _requests.Remove(request);
                await SaveChangesAsync();
            }
        }

        public Task<List<RequestDbContext>> GetByUserId(Guid userId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> NameExistsAsync(string name)
        {
            return await _requests.Where(r => r.Status == Status.Pending).AnyAsync(e => e.Name.Equals(name));
        }

        private async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
