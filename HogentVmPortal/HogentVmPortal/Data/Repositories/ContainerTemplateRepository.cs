using HogentVmPortal.Shared.Data;
using HogentVmPortal.Shared.Model;
using Microsoft.EntityFrameworkCore;

namespace HogentVmPortal.Data.Repositories
{
    public interface IContainerTemplateRepository
    {
        Task<List<ContainerTemplate>> GetAll();
        Task<ContainerTemplate> GetById(Guid id);
        Task<List<ContainerTemplate>> GetByUserId(string userId);
        Task Add(ContainerTemplate template);
        void Update(ContainerTemplate template);
        Task Delete(Guid id);
        void Delete(ContainerTemplate template);
        Task SaveChangesAsync();
        bool TemplateNameExists(string name);
        bool ProxmoxIdExists(int id);
    }

    public class ContainerTemplateRepository : IContainerTemplateRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<ContainerTemplate> _templates;

        public ContainerTemplateRepository(ApplicationDbContext context)
        {
            _context = context;
            _templates = _context.ContainerTemplates;
        }
        public async Task Add(ContainerTemplate template)
        {
            await _templates.AddAsync(template);
        }

        public async Task Delete(Guid id)
        {
            var template = await GetById(id) ?? throw new ContainerTemplateNotFoundException(id.ToString());
            Delete(template);
        }

        public void Delete(ContainerTemplate template)
        {
            _templates.Remove(template);
        }

        public async Task<List<ContainerTemplate>> GetAll()
        {
            return await _templates.ToListAsync();
        }

        public async Task<List<ContainerTemplate>> GetByUserId(string userId)
        {
            var filteredTemplates = await _templates
                .Where(template => template.Courses.Any(c => c.Students.Any(s => s.Id.Equals(userId))))
                .ToListAsync();

            return filteredTemplates;
        }

        public async Task<ContainerTemplate> GetById(Guid id)
        {
            var template = await _templates.SingleOrDefaultAsync(x => x.Id == id);
            return template ?? throw new ContainerTemplateNotFoundException(id.ToString());
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Update(ContainerTemplate template)
        {
            _context.Update(template);
        }

        public bool TemplateNameExists(string name)
        {
            return _templates.Any(e => e.Name.Equals(name));
        }

        public bool ProxmoxIdExists(int id)
        {
            return _templates.Any(e => e.ProxmoxId == id);
        }
    }
}
