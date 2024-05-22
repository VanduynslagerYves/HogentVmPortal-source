using HogentVmPortal.Shared.Model;

namespace HogentVmPortal.Data.Repositories
{
    public interface IContainerTemplateRepository
    {
        Task<List<ContainerTemplate>> GetAll();
        Task<ContainerTemplate> GetById(Guid id);
        Task<List<ContainerTemplate>> GetByUserId(string userId);
        Task Add(ContainerTemplate template);
        void Update(ContainerTemplate template);
        Task Delete(Guid id);
        void Delete(ContainerTemplate template);
        Task SaveChangesAsync();
        bool TemplateNameExists(string name);
        bool ProxmoxIdExists(int id);
    }
}
