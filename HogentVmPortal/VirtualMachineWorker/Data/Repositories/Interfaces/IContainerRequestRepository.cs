using HogentVmPortal.Shared.DTO;

namespace VirtualMachineWorker.Data.Repositories
{
    public interface IContainerRequestRepository
    {
        Task<List<ContainerCreateRequest>> GetAllCreateRequests();
        //Task<List<ContainerEditRequest>> GetAllEditRequests();
        Task<List<ContainerRemoveRequest>> GetAllRemoveRequests();

        void Delete(ContainerCreateRequest request);
        //void Delete(ContainerEditRequest request);
        void Delete(ContainerRemoveRequest request);

        Task SaveChangesAsync();
    }
}
