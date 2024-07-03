using HogentVmPortal.Shared.Data;
using HogentVmPortal.Shared.Model;
using Microsoft.EntityFrameworkCore;

namespace HogentVmPortal.Data.Repositories
{
    public interface IVirtualMachineTemplateRepository
    {
        Task<List<VirtualMachineTemplate>> GetAll();
        Task<VirtualMachineTemplate> GetById(Guid id);
        Task<List<VirtualMachineTemplate>> GetByUserId(string userId);
        Task Add(VirtualMachineTemplate template);
        void Update(VirtualMachineTemplate template);
        Task Delete(Guid id);
        void Delete(VirtualMachineTemplate template);
        Task SaveChangesAsync();
        bool TemplateNameExists(string name);
        bool ProxmoxIdExists(int id);
    }

    public class VirtualMachineTemplateRepository : IVirtualMachineTemplateRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<VirtualMachineTemplate> _templates;

        public VirtualMachineTemplateRepository(ApplicationDbContext context)
        {
            _context = context;
            _templates = _context.VirtualMachineTemplates;
        }
        public async Task Add(VirtualMachineTemplate template)
        {
            await _templates.AddAsync(template);
        }

        public async Task Delete(Guid id)
        {
            var template = await GetById(id) ?? throw new VirtualMachineTemplateNotFoundException(id.ToString());
            Delete(template);
        }

        public void Delete(VirtualMachineTemplate template)
        {
            _templates.Remove(template);
        }

        public async Task<List<VirtualMachineTemplate>> GetAll()
        {
            return await _templates.ToListAsync();
        }

        //Determining the available templates for a user: uses the navigation to Course and then the navigation to Template per HogentUser
        public async Task<List<VirtualMachineTemplate>> GetByUserId(string userId)
        {
            var filteredTemplates = await _templates
                .Where(template => template.Courses.Any(c => c.Students.Any(s => s.Id.Equals(userId))))
                .ToListAsync();

            return filteredTemplates;
        }

        public async Task<VirtualMachineTemplate> GetById(Guid id)
        {
            var template = await _templates.SingleOrDefaultAsync(x => x.Id == id);
            return template ?? throw new VirtualMachineTemplateNotFoundException(id.ToString());
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Update(VirtualMachineTemplate template)
        {
            _context.Update(template);
        }

        public bool TemplateNameExists(string name)
        {
            return _templates.Any(e => e.Name.Equals(name));
        }

        public bool ProxmoxIdExists(int id)
        {
            //TODO: also check containers, and vm's. place this method in a seperate repository
            return _templates.Any(e => e.ProxmoxId == id);
        }
    }
}
