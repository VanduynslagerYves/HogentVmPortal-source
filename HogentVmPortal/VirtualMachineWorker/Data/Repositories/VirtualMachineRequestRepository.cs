using HogentVmPortal.Shared.Data;
using HogentVmPortal.Shared.DTO;
using Microsoft.EntityFrameworkCore;

namespace VirtualMachineWorker.Data.Repositories
{
    public class VirtualMachineRequestRepository : IVirtualMachineRequestRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<VirtualMachineCreateRequest> _createRequests;
        //private readonly DbSet<VirtualMachineEditRequest> _editRequests;
        private readonly DbSet<VirtualMachineRemoveRequest> _removeRequests;

        public VirtualMachineRequestRepository(ApplicationDbContext context)
        {
            _context = context;

            _createRequests = _context.VirtualMachineCreateRequests;
            //_editRequests = _context.VirtualMachineEditRequests;
            _removeRequests = _context.VirtualMachineRemoveRequests;
        }
        public void Delete(VirtualMachineCreateRequest request)
        {
            _createRequests.Remove(request);
        }

        //public void Delete(VirtualMachineEditRequest request)
        //{
        //    _editRequests.Remove(request);
        //}

        public void Delete(VirtualMachineRemoveRequest request)
        {
            _removeRequests.Remove(request);
        }

        public async Task<List<VirtualMachineCreateRequest>> GetAllCreateRequests()
        {
            return await _createRequests.ToListAsync();
        }

        //public async Task<List<VirtualMachineEditRequest>> GetAllEditRequests()
        //{
        //    return await _editRequests.ToListAsync();
        //}

        public async Task<List<VirtualMachineRemoveRequest>> GetAllRemoveRequests()
        {
            return await _removeRequests.ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
