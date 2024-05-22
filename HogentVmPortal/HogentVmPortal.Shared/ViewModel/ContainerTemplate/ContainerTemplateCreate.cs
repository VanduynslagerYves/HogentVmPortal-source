using System.ComponentModel.DataAnnotations;

namespace HogentVmPortal.Shared.ViewModel
{
    public class ContainerTemplateCreate
    {
        public ContainerTemplateCreate()
        {
            ProxmoxId = 100;
        }

        [Required]
        public string? Name { get; set; }

        [Required]
        [Range(100, 999)]
        public int ProxmoxId { get; set; }

        public string? Description { get; set; }

        [Required]
        public Guid? CourseId { get; set; }
    }
}
