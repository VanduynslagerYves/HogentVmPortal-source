using HogentVmPortal.Shared.Model;
using System.ComponentModel.DataAnnotations;

namespace HogentVmPortal.Shared.ViewModel
{
    public class CourseDelete : ViewModelBase<CourseDelete, Course>
    {
        [Required]
        public Guid Id { get; private set; }
        public string? Name { get; private set; }

        public static CourseDelete ToViewModel(Course course)
        {
            return new CourseDelete
            {
                Id = course.Id,
                Name = course.Name,
            };
        }
    }
}
