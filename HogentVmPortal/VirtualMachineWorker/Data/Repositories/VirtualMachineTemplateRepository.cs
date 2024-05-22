using HogentVmPortal.Shared.Data;
using HogentVmPortal.Shared.Model;
using Microsoft.EntityFrameworkCore;

namespace VirtualMachineWorker.Data.Repositories
{
    public class VirtualMachineTemplateRepository : IVirtualMachineTemplateRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<VirtualMachineTemplate> _templates;
        public VirtualMachineTemplateRepository(ApplicationDbContext context)
        {
            _context = context;
            _templates = _context.VirtualMachineTemplates;
        }

        public async Task<VirtualMachineTemplate> GetByCloneId(int id)
        {
            var template = await _templates.SingleOrDefaultAsync(t => t.ProxmoxId == id);
            if (template == null) throw new VirtualMachineTemplateNotFoundException();
            return template;
        }
    }
}
