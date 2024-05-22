using HogentVmPortal.Shared.Model;
using System.ComponentModel.DataAnnotations;

namespace HogentVmPortal.Shared.ViewModel
{
    public class CourseEdit : ViewModelBase<CourseEdit, Course>
    {
        [Required]
        public Guid Id { get; private set; }
        [Required]
        public required string Name { get; set; }

        public static CourseEdit ToViewModel(Course model)
        {
            return new CourseEdit
            {
                Id = model.Id,
                Name = model.Name,
            };
        }
    }
}
