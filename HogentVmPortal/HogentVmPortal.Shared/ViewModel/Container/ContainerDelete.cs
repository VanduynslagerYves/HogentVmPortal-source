using HogentVmPortal.Shared.Model;
using System.ComponentModel.DataAnnotations;

namespace HogentVmPortal.Shared.ViewModel
{
    public class ContainerDelete : ViewModelBase<ContainerDelete, Container>
    {
        [Required]
        public Guid Id { get; set; }
        public string? Name { get; private set; }

        public static ContainerDelete ToViewModel(Container container)
        {
            return new ContainerDelete
            {
                Id = container.Id,
                Name = container.Name
            };
        }
    }
}
