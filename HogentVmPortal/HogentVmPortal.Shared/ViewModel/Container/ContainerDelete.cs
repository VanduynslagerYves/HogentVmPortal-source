using HogentVmPortal.Shared.DTO;
using System.ComponentModel.DataAnnotations;

namespace HogentVmPortal.Shared.ViewModel
{
    public class ContainerDelete : ViewModelBase<ContainerDelete, ContainerDTO>
    {
        [Required]
        public Guid Id { get; set; }
        public string? Name { get; private set; }

        public static ContainerDelete ToViewModel(ContainerDTO container)
        {
            return new ContainerDelete
            {
                Id = container.Id,
                Name = container.Name
            };
        }
    }
}
