using HogentVmPortal.Data.Repositories;
using HogentVmPortal.Shared.Model;
using HogentVmPortal.Shared.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HogentVmPortal.Controllers
{
    public class CourseController : Controller
    {
        private readonly IAppUserRepository _appUserRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IVirtualMachineTemplateRepository _templateRepository;

        public CourseController(ICourseRepository courseRepository, IAppUserRepository appUserRepository, IVirtualMachineTemplateRepository templateRepository)
        {
            _appUserRepository = appUserRepository;
            _courseRepository = courseRepository;
            _templateRepository = templateRepository;
        }

        // GET: Courses
        public async Task<ActionResult> Index()
        {
            var courses = await _courseRepository.GetAll();
            return View(CourseListItem.ToViewModel(courses));
        }

        // GET: CourseController/Create
        public ActionResult Create()
        {
            ViewData["Students"] = GetStudentsAsSelectList();
            ViewData["Templates"] = GetTemplatesAsSelectList();
            return View(new CourseCreate());
        }

        // POST: CourseController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind("Id, Name, StudentId, TemplateId")] CourseCreate courseViewModel)
        {
            if (!string.IsNullOrEmpty(courseViewModel.Name) && _courseRepository.CourseNameExists(courseViewModel.Name))
            {
                ModelState.AddModelError("Name", $"{courseViewModel.Name} is taken");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _appUserRepository.GetById(courseViewModel.StudentId!);                    

                    var course = new Course
                    {
                        Id = Guid.NewGuid(),
                        Name = courseViewModel.Name!,
                    };
                    course.Students.Add(user);

                    if (courseViewModel.TemplateId != Guid.Empty && courseViewModel.TemplateId != null)
                    {
                        var template = await _templateRepository.GetById(courseViewModel.TemplateId.Value);
                        course.VirtualMachineTemplates.Add(template);
                    }

                    await _courseRepository.Add(course);
                    await _courseRepository.SaveChangesAsync();

                    TempData["Message"] = string.Format("Course {0} has been added", course.Name);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = ex.Message;
                    return View(courseViewModel);
                }
            }
            return View(courseViewModel);
        }

        // GET: CourseController/Delete/5
        public async Task<ActionResult> Delete(Guid id)
        {
            CourseDelete courseDelete;

            try
            {
                var course = await _courseRepository.GetById(id);
                courseDelete = CourseDelete.ToViewModel(course);
            }
            catch (VirtualMachineTemplateNotFoundException e)
            {
                TempData["Error"] = e.Message;
                return RedirectToAction(nameof(Index));
            }

            return View(courseDelete);
        }

        // POST: CourseController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var course = await _courseRepository.GetById(id);

                _courseRepository.Delete(course);
                await _courseRepository.SaveChangesAsync();

                TempData["Message"] = string.Format("Course {0} has been deleted", course.Name);
            }
            catch (VirtualMachineTemplateNotFoundException e)
            {
                TempData["Error"] = e.Message;
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        private SelectList GetStudentsAsSelectList()
        {
            //MultiSelectList select multiple="multiple"
            //https://stackoverflow.com/questions/40555543/how-do-i-implement-a-checkbox-list-in-asp-net-core
            var users = _appUserRepository.GetAll().Result;

            var studentListItems = HogentUserListItem.ToViewModel(users);

            var selectList = new SelectList(studentListItems.OrderBy(x => x.Name),
                nameof(CourseListItem.Id),
                nameof(CourseListItem.Name));

            return selectList;
        }

        private SelectList GetTemplatesAsSelectList()
        {
            //MultiSelectList select multiple="multiple"
            //https://stackoverflow.com/questions/40555543/how-do-i-implement-a-checkbox-list-in-asp-net-core
            var templates = _templateRepository.GetAll().Result;

            var templateListItems = VirtualMachineTemplateListItem.ToViewModel(templates);

            var selectList = new SelectList(templateListItems.OrderBy(x => x.Name),
                nameof(VirtualMachineTemplateListItem.Id),
                nameof(VirtualMachineTemplateListItem.Name));

            return selectList;
        }
    }
}
