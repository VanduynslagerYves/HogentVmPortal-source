using HogentVmPortal.Shared.Data;
using HogentVmPortal.Shared.Model;
using Microsoft.EntityFrameworkCore;

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

    public class CourseRepository : ICourseRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Course> _courses;

        public CourseRepository(ApplicationDbContext context)
        {
            _context = context;
            _courses = _context.Courses;
        }
        
        public async Task Add(Course course)
        {
            await _courses.AddAsync(course);
        }

        public async Task Delete(Guid id)
        {
            var course = await GetById(id) ?? throw new CourseNotFoundException(id.ToString());
            Delete(course);
        }

        public void Delete(Course course)
        {
            _courses.Remove(course);
        }

        public async Task<Course> GetById(Guid id)
        {
            var course = await _courses.SingleOrDefaultAsync(x => x.Id == id);
            return course ?? throw new CourseNotFoundException(id.ToString());
        }

        public async Task<List<Course>> GetAll()
        {
            return await _courses.ToListAsync();
        }

        public void Update(Course course)
        {
            _context.Update(course);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public bool CourseNameExists(string name)
        {
            return _courses.Any(e => e.Name.Equals(name));
        }
    }
}
