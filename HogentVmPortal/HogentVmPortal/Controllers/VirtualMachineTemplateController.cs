using Microsoft.AspNetCore.Mvc;
using HogentVmPortal.Data.Repositories;
using HogentVmPortal.Shared.ViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using HogentVmPortal.Shared.Model;

namespace HogentVmPortal.Controllers
{
    public class VirtualMachineTemplateController : Controller
    {
        private readonly IVirtualMachineTemplateRepository _templateRepository;
        private readonly ICourseRepository _courseRepository;

        public VirtualMachineTemplateController(IVirtualMachineTemplateRepository templateRepository, ICourseRepository courseRepository)
        {
            _templateRepository = templateRepository;
            _courseRepository = courseRepository;
        }

        // GET: VirtualMachineTemplate
        public async Task<IActionResult> Index()
        {
            var templates = await _templateRepository.GetAll();
            return View(VirtualMachineTemplateListItem.ToViewModel(templates));
        }

        // GET: VirtualMachineTemplate/Create
        public IActionResult Create()
        {
            ViewData["Courses"] = GetCoursesAsSelectList();
            return View(new VirtualMachineTemplateCreate());
        }

        // POST: VirtualMachineTemplate/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProxmoxId,Name,Description,OperatingSystem,CourseId")] VirtualMachineTemplateCreate templateViewModel)
        {
            if(!string.IsNullOrEmpty(templateViewModel.Name) && _templateRepository.TemplateNameExists(templateViewModel.Name))
            {
                ModelState.AddModelError("Name", $"{templateViewModel.Name} is taken");
            }
            //TODO: check here for proxmoxId exists for containers and for vm's!
            if(_templateRepository.ProxmoxIdExists(templateViewModel.ProxmoxId))
            {
                ModelState.AddModelError("ProxmoxId", $"{templateViewModel.ProxmoxId} is taken");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var template = new VirtualMachineTemplate
                    {
                        Id = Guid.NewGuid(),
                        OperatingSystem = templateViewModel.OperatingSystem!,
                        Name = templateViewModel.Name!,
                        ProxmoxId = templateViewModel.ProxmoxId,
                        Description = templateViewModel.Description!
                    };

                    if (templateViewModel.CourseId != Guid.Empty && templateViewModel.CourseId != null)
                    {
                        var course = await _courseRepository.GetById(templateViewModel.CourseId.Value);
                        template.Courses.Add(course);
                    }

                    await _templateRepository.Add(template);
                    await _templateRepository.SaveChangesAsync();

                    TempData["Message"] = string.Format("Template {0} has been added", template.Name);
                    return RedirectToAction(nameof(Index));
                }
                catch(Exception ex)
                {
                    TempData["Error"] = ex.Message;
                    return View(templateViewModel);
                }
            }
            return View(templateViewModel);
        }

        // GET: VirtualMachineTemplate/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            VirtualMachineTemplateDelete templateDelete;

            try
            {
                var template = await _templateRepository.GetById(id);
                templateDelete = VirtualMachineTemplateDelete.ToViewModel(template);
            }
            catch(VirtualMachineTemplateNotFoundException e)
            {
                TempData["Error"] = e.Message;
                return RedirectToAction(nameof(Index));
            }

            return View(templateDelete);
        }

        // POST: VirtualMachineTemplate/Delete/5
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
            catch (VirtualMachineTemplateNotFoundException e)
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
