using HogentVmPortal.Shared.Data;
using HogentVmPortal.Shared.DTO;
using Microsoft.EntityFrameworkCore;

namespace VirtualMachineWorker.Data.Repositories
{
    public class ContainerRequestRepository : IContainerRequestRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<ContainerCreateRequest> _createRequests;
        //private readonly DbSet<ContainerEditRequest> _editRequests;
        private readonly DbSet<ContainerRemoveRequest> _removeRequests;

        public ContainerRequestRepository(ApplicationDbContext context)
        {
            _context = context;

            _createRequests = _context.ContainerCreateRequests;
            //_editRequests = _context.ContainerEditRequests;
            _removeRequests = _context.ContainerRemoveRequests;
        }
        public void Delete(ContainerCreateRequest request)
        {
            _createRequests.Remove(request);
        }

        //public void Delete(ContainerEditRequest request)
        //{
        //    _editRequests.Remove(request);
        //}

        public void Delete(ContainerRemoveRequest request)
        {
            _removeRequests.Remove(request);
        }

        public async Task<List<ContainerCreateRequest>> GetAllCreateRequests()
        {
            return await _createRequests.ToListAsync();
        }

        //public async Task<List<ContainerEditRequest>> GetAllEditRequests()
        //{
        //    return await _editRequests.ToListAsync();
        //}

        public async Task<List<ContainerRemoveRequest>> GetAllRemoveRequests()
        {
            return await _removeRequests.ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
