using HogentVmPortal.Shared.Model;

namespace HogentVmPortal.Data.Repositories
{
    public interface ICourseRepository
    {
        Task Add(Course course);
        Task Delete(Guid id);
        void Delete(Course course);
        Task<Course> GetById(Guid id);
        Task<List<Course>> GetAll();
        void Update(Course course);
        Task SaveChangesAsync();
        bool CourseNameExists(string name);
    }
}
