using HogentVmPortal.Shared.Data;
using HogentVmPortal.Shared.Model;
using Microsoft.EntityFrameworkCore;

namespace HogentVmPortalWebAPI.Data.Repositories
{
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
                    .Include(x => x.Owner)
                    .Include(x => x.Template)
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

        public async Task Add(VirtualMachine virtualMachine)
        {
            await _virtualMachines.AddAsync(virtualMachine);
        }

        public void Update(VirtualMachine virtualMachine)
        {
            _context.Update(virtualMachine);
        }

        public async Task Delete(Guid id)
        {
            var virtualMachine = await GetById(id);

            if (virtualMachine == null) throw new VirtualMachineNotFoundException(id.ToString());

            _virtualMachines.Remove(virtualMachine);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
