using HogentVmPortal.Shared.Data;
using HogentVmPortal.Shared.Model;
using Microsoft.EntityFrameworkCore;

namespace HogentVmPortal.Data.Repositories
{
    public interface IVirtualMachineRepository
    {
        public Task<VirtualMachine> GetById(Guid id, bool includeUsers = false);
        public bool VirtualMachineNameExists(string name);
    }

    public class VirtualMachineRepository : IVirtualMachineRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<VirtualMachine> _virtualMachines;

        public VirtualMachineRepository(ApplicationDbContext context)
        {
            _context = context;
            _virtualMachines = _context.VirtualMachines;
        }

        public async Task<VirtualMachine> GetById(Guid id, bool includeUsers = false)
        {
            VirtualMachine? virtualMachine = null;

            if (includeUsers)
            {
                virtualMachine = await _virtualMachines
                    .Include(x => x.Template)
                    .Include(x => x.Owner)
                    .SingleOrDefaultAsync(x => x.Id == id);
            }
            else
            {
                virtualMachine = await _virtualMachines
                    .Include(x => x.Template)
                    .SingleOrDefaultAsync(x => x.Id == id);
            }

            if (virtualMachine == null) throw new VirtualMachineNotFoundException(id.ToString());
            return virtualMachine;
        }

        public bool VirtualMachineNameExists(string name)
        {
            return _virtualMachines.Any(e => e.Name.Equals(name));
        }
    }
}
