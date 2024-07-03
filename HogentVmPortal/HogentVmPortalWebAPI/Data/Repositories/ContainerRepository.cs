using HogentVmPortal.Shared.Data;
using HogentVmPortal.Shared.DTO;
using HogentVmPortal.Shared.Model;
using Microsoft.EntityFrameworkCore;

namespace HogentVmPortalWebAPI.Data.Repositories
{
    public interface IContainerRepository
    {
        Task<List<ContainerDTO>> GetAll(bool includeUsers = false);
        Task<Container> GetById(Guid id, bool includeUsers = false);
        Task Add(Container container);
        void Update(Container container);
        Task Delete(Guid id);
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

        public async Task<List<ContainerDTO>> GetAll(bool includeUsers = false)
        {
            var containers = new List<ContainerDTO>();

            if (includeUsers)
            {
                containers = await _containers
                    .Include(ct => ct.Owner)
                    .Select(ct => Container.ToDTO(ct))
                    .ToListAsync();
            }
            else
            {
                containers = await _containers
                    .Select(ct => Container.ToDTO(ct))
                    .ToListAsync();
            }

            return containers;
        }

        //TODO: map to DTO, template non required
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
            await _context.SaveChangesAsync();
        }

        public async void Update(Container container)
        {
            _containers.Update(container);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            var container = await GetById(id);

            if (container == null) throw new ContainerNotFoundException(id.ToString());

            _containers.Remove(container);
            await SaveChangesAsync();
        }

        private async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
