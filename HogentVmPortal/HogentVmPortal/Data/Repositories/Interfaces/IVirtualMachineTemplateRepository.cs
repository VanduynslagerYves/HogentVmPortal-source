using HogentVmPortal.Shared.Model;

namespace HogentVmPortal.Data.Repositories
{
    public interface IVirtualMachineTemplateRepository
    {
        Task<List<VirtualMachineTemplate>> GetAll();
        Task<VirtualMachineTemplate> GetById(Guid id);
        Task<List<VirtualMachineTemplate>> GetByUserId(string userId);
        Task Add(VirtualMachineTemplate template);
        void Update(VirtualMachineTemplate template);
        Task Delete(Guid id);
        void Delete(VirtualMachineTemplate template);
        Task SaveChangesAsync();
        bool TemplateNameExists(string name);
        bool ProxmoxIdExists(int id);
    }
}
