using HogentVmPortal.Data.Repositories;
using HogentVmPortal.Shared.Model;
using HogentVmPortal.Shared.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HogentVmPortal.Controllers
{
    public class ContainerTemplateController : Controller
    {
        private readonly IContainerTemplateRepository _templateRepository;
        private readonly ICourseRepository _courseRepository;

        public ContainerTemplateController(IContainerTemplateRepository templateRepository, ICourseRepository courseRepository)
        {
            _templateRepository = templateRepository;
            _courseRepository = courseRepository;
        }

        // GET: ContainerTemplate
        public async Task<IActionResult> Index()
        {
            var templates = await _templateRepository.GetAll();
            return View(ContainerTemplateListItem.ToViewModel(templates));
        }

        // GET: ContainerTemplate/Create
        public IActionResult Create()
        {
            ViewData["Courses"] = GetCoursesAsSelectList();
            return View(new ContainerTemplateCreate());
        }

        // POST: ContainerTemplate/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProxmoxId,Name,Description,CourseId")] ContainerTemplateCreate templateCreate)
        {
            if (!string.IsNullOrEmpty(templateCreate.Name) && _templateRepository.TemplateNameExists(templateCreate.Name))
            {
                ModelState.AddModelError("Name", $"{templateCreate.Name} is taken");
            }
            //TODO: check here for proxmoxId exists for containers and for vm's!
            if (_templateRepository.ProxmoxIdExists(templateCreate.ProxmoxId))
            {
                ModelState.AddModelError("ProxmoxId", $"{templateCreate.ProxmoxId} is taken");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var template = new ContainerTemplate
                    {
                        Id = Guid.NewGuid(),
                        Name = templateCreate.Name!,
                        ProxmoxId = templateCreate.ProxmoxId,
                        Description = templateCreate.Description!
                    };

                    if (templateCreate.CourseId != Guid.Empty && templateCreate.CourseId != null)
                    {
                        var course = await _courseRepository.GetById(templateCreate.CourseId.Value);
                        template.Courses.Add(course);
                    }

                    await _templateRepository.Add(template);
                    await _templateRepository.SaveChangesAsync();

                    TempData["Message"] = string.Format("Template {0} has been added", template.Name);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = ex.Message;
                    return View(templateCreate);
                }
            }
            return View(templateCreate);
        }

        // GET: ContainerTemplate/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            ContainerTemplateDelete templateDelete;

            try
            {
                var template = await _templateRepository.GetById(id);
                templateDelete = ContainerTemplateDelete.ToViewModel(template);
            }
            catch (ContainerTemplateNotFoundException e)
            {
                TempData["Error"] = e.Message;
                return RedirectToAction(nameof(Index));
            }

            return View(templateDelete);
        }

        // POST: ContainerTemplate/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var template = await _templateRepository.GetById(id);

                _templateRepository.Delete(template);
                await _templateRepository.SaveChangesAsync();

                TempData["Message"] = string.Format("Template {0} has been deleted", template.Name);
            }
            catch (ContainerTemplateNotFoundException e)
            {
                TempData["Error"] = e.Message;
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        private SelectList GetCoursesAsSelectList()
        {
            //MultiSelectList select multiple="multiple"
            //https://stackoverflow.com/questions/40555543/how-do-i-implement-a-checkbox-list-in-asp-net-core
            var courses = _courseRepository.GetAll().Result;

            var courseListItems = CourseListItem.ToViewModel(courses);

            var selectList = new SelectList(courses.OrderBy(x => x.Name),
                nameof(CourseListItem.Id),
                nameof(CourseListItem.Name));

            return selectList;
        }
    }
}
