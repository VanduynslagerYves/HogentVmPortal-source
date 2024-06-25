using HogentVmPortal.Shared.Data;
using HogentVmPortal.Shared.Model;
using Microsoft.EntityFrameworkCore;

namespace HogentVmPortalWebAPI.Data.Repositories
{
    public class ContainerTemplateRepository : IContainerTemplateRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<ContainerTemplate> _templates;
        public ContainerTemplateRepository(ApplicationDbContext context)
        {
            _context = context;
            _templates = _context.ContainerTemplates;
        }

        public async Task<ContainerTemplate> GetByCloneId(int id)
        {
            var template = await _templates.SingleOrDefaultAsync(t => t.ProxmoxId == id);
            if (template == null) throw new ContainerTemplateNotFoundException();
            return template;
        }
    }
}
