using HogentVmPortal.Shared.Data;
using HogentVmPortal.Shared.DTO;
using Microsoft.EntityFrameworkCore;

namespace HogentVmPortal.Data.Repositories
{
    public class ContainerRequestRepository : IContainerRequestRepository
    {
        private readonly ApplicationDbContext _context;

        private readonly DbSet<ContainerCreateRequest> _createRequests;
        private readonly DbSet<ContainerRemoveRequest> _removeRequests;
        //private readonly DbSet<ContainerEditRequest> _editRequests;

        public ContainerRequestRepository(ApplicationDbContext context)
        {
            _context = context;

            _createRequests = _context.ContainerCreateRequests;
            _removeRequests = _context.ContainerRemoveRequests;
            //_editRequests = _context.ContainerEditRequests;
        }

        public async Task Add(ContainerCreateRequest request)
        {
            await _createRequests.AddAsync(request);
        }

        public async Task Add(ContainerRemoveRequest request)
        {
            await _removeRequests.AddAsync(request);
        }

        //public async Task Add(ContainerEditRequest request)
        //{
        //    await _editRequests.AddAsync(request);
        //}

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
