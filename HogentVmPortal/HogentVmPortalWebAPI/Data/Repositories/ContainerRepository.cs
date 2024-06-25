using HogentVmPortal.Shared.Data;
using HogentVmPortal.Shared.Model;
using Microsoft.EntityFrameworkCore;

namespace HogentVmPortalWebAPI.Data.Repositories
{
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
                    .Include(x => x.Owner)
                    .Include(x => x.Template)
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

        public async Task Add(Container container)
        {
            await _containers.AddAsync(container);
        }

        public void Update(Container container)
        {
            _context.Update(container);
        }

        public async Task Delete(Guid id)
        {
            var container = await GetById(id);

            if (container == null) throw new ContainerNotFoundException(id.ToString());

            _containers.Remove(container);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
