using HogentVmPortal.Shared.Data;
using HogentVmPortal.Shared.Model;
using HogentVmPortal.Shared.DTO;
using Microsoft.EntityFrameworkCore;

namespace HogentVmPortal.Shared.Repositories
{
    public interface IVirtualMachineRepository
    {
        Task<bool> NameExistsAsync(string name);
        Task<List<VirtualMachineDTO>> GetAll(bool includeUsers = false);
        Task<VirtualMachineDTO> GetById(Guid id, bool includeUsers = false);
        Task Add(VirtualMachine virtualMachine);
        Task Update(VirtualMachine virtualMachine);
        Task Delete(Guid id);
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

        public async Task<List<VirtualMachineDTO>> GetAll(bool includeUsers = false)
        {
            var virtualMachines = new List<VirtualMachineDTO>();

            if (includeUsers)
            {
                virtualMachines = await _virtualMachines
                    .Include(vm => vm.Owner)
                    .Select(vm => VirtualMachine.ToDTO(vm)) //map to DTO object to avoid circular dependencies: vm -> owner -> vms -> owner ...
                    //this does not need to be flattened (see ToDTO), maybe refactor to usage of a OwnerDTO object
                    .ToListAsync();
            }
            else
            {
                virtualMachines = await _virtualMachines
                    .Select(vm => VirtualMachine.ToDTO(vm))
                    .ToListAsync();
            }

            return virtualMachines;
        }

        //TODO: map to DTO, template non required in DTO
        public async Task<VirtualMachineDTO> GetById(Guid id, bool includeUsers = false)
        {
            VirtualMachineDTO? virtualMachineDTO = null;

            if (includeUsers)
            {
                var virtualMachine = await _virtualMachines
                    .Include(x => x.Owner)
                    .Include(x => x.Template)
                    .SingleOrDefaultAsync(x => x.Id == id);

                virtualMachineDTO = virtualMachine != null ? VirtualMachine.ToDTO(virtualMachine) : null;
            }
            else
            {
                var virtualMachine = await _virtualMachines
                    .Include(x => x.Template)
                    .SingleOrDefaultAsync(x => x.Id == id);

                virtualMachineDTO = virtualMachine != null ? VirtualMachine.ToDTO(virtualMachine) : null;
            }

            if (virtualMachineDTO == null) throw new VirtualMachineNotFoundException(id.ToString());
            return virtualMachineDTO;
        }

        public async Task Add(VirtualMachine virtualMachine)
        {
            await _virtualMachines.AddAsync(virtualMachine);
            await SaveChangesAsync();
        }

        public async Task Update(VirtualMachine virtualMachine)
        {
            _virtualMachines.Update(virtualMachine);
            await SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
           var virtualMachine = await _virtualMachines.SingleOrDefaultAsync(x => x.Id == id);

            if (virtualMachine == null) throw new VirtualMachineNotFoundException(id.ToString());

            _virtualMachines.Remove(virtualMachine);
            await SaveChangesAsync();
        }

        public async Task<bool> NameExistsAsync(string name)
        {
            return await _virtualMachines.AnyAsync(e => e.Name.Equals(name));
        }

        private async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
