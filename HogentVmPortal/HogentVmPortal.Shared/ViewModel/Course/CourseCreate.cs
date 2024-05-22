using System.ComponentModel.DataAnnotations;

namespace HogentVmPortal.Shared.ViewModel
{
    public class CourseCreate
    {
        [Required]
        //[DisplayName("Name")]
        public string? Name { get; set; }
        [Required]
        public string? StudentId { get; set; }

        public Guid? TemplateId { get; set; }
    }
}
