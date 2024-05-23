using HogentVmPortal.Shared.Data;
using HogentVmPortal.Shared.DTO;
using Microsoft.EntityFrameworkCore;

namespace HogentVmPortal.Data.Repositories
{
    public class VirtualMachineRequestRepository : IVirtualMachineRequestRepository
    {
        private readonly ApplicationDbContext _context;

        private readonly DbSet<VirtualMachineCreateRequest> _createRequests;
        private readonly DbSet<VirtualMachineRemoveRequest> _removeRequests;

        public VirtualMachineRequestRepository(ApplicationDbContext context)
        {
            _context = context;

            _createRequests = _context.VirtualMachineCreateRequests;
            _removeRequests = _context.VirtualMachineRemoveRequests;
        }

        public async Task Add(VirtualMachineCreateRequest request)
        {
            await _createRequests.AddAsync(request);
        }

        public async Task Add(VirtualMachineRemoveRequest request)
        {
            await _removeRequests.AddAsync(request);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
