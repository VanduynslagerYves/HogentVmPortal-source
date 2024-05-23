using HogentVmPortal.Shared.DTO;

namespace HogentVmPortal.Data.Repositories
{
    public interface IContainerRequestRepository
    {
        Task Add(ContainerCreateRequest request);
        Task Add(ContainerRemoveRequest request);
        Task SaveChangesAsync();
    }
}
