using HogentVmPortal.Shared.Model;
using System.ComponentModel.DataAnnotations;

namespace HogentVmPortal.Shared.ViewModel
{
    public class ContainerEdit : ViewModelBase<ContainerEdit, Container>
    {
        [Required]
        public Guid Id { get; private set; }
        public required string Name { get; set; }

        public static ContainerEdit ToViewModel(Container container)
        {
            return new ContainerEdit
            {
                Id = container.Id,
                Name = container.Name,
            };
        }
    }
}
