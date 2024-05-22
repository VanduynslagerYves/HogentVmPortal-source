using HogentVmPortal.Shared.Model;

namespace HogentVmPortal.Shared.ViewModel
{
    public class CourseListItem : ViewModelBase<CourseListItem, Course>
    {
        public Guid Id { get; private set; }
        public required string Name { get; set; }

        public static CourseListItem ToViewModel(Course course)
        {
            return new CourseListItem
            {
                Id = course.Id,
                Name = course.Name,
            };
        }

        public static List<CourseListItem> ToViewModel(List<Course> courses)
        {
            return courses.Select(ToViewModel).ToList();
        }
    }
}
