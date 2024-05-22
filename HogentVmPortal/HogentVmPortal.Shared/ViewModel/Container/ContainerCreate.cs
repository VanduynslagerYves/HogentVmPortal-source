using System.ComponentModel.DataAnnotations;

namespace HogentVmPortal.Shared.ViewModel
{
    public class ContainerCreate
    {
        [Required]
        public required string Name { get; set; }
        [Required]
        public required int CloneId { get; set; }
    }
}
