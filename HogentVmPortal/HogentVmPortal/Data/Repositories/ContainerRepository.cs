using HogentVmPortal.Shared.Data;
using HogentVmPortal.Shared.Model;
using Microsoft.EntityFrameworkCore;

namespace HogentVmPortal.Data.Repositories
{
    public interface IContainerRepository
    {
        public Task<Container> GetById(Guid id, bool includeUsers = false);
        public bool ContainerNameExists(string name);
    }

    public class ContainerRepository : IContainerRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Container> _containers;

        public ContainerRepository(ApplicationDbContext context)
        {
            _context = context;
            _containers = _context.Containers;
        }

        public async Task<Container> GetById(Guid id, bool includeUsers = false)
        {
            Container? container = null;

            if (includeUsers)
            {
                container = await _containers
                    .Include(x => x.Template)
                    .Include(x => x.Owner)
                    .SingleOrDefaultAsync(x => x.Id == id);
            }
            else
            {
                container = await _containers
                    .Include(x => x.Template)
                    .SingleOrDefaultAsync(x => x.Id == id);
            }

            if (container == null) throw new ContainerNotFoundException(id.ToString());
            return container;
        }

        public bool ContainerNameExists(string name)
        {
            return _containers.Any(e => e.Name.Equals(name));
        }
    }
}
