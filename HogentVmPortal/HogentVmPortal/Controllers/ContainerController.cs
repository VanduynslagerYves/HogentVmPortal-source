using HogentVmPortal.Data.Repositories;
using HogentVmPortal.Extensions;
using HogentVmPortal.Services;
using HogentVmPortal.Shared;
using HogentVmPortal.Shared.DTO;
using HogentVmPortal.Shared.Model;
using HogentVmPortal.Shared.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Renci.SshNet;

namespace HogentVmPortal.Controllers
{
    public class ContainerController : Controller
    {
        private readonly ILogger<ContainerController> _logger;

        private readonly IContainerTemplateRepository _containerTemplateRepository;
        private readonly ICourseRepository _courseRepository;

        private readonly IAppUserRepository _appUserRepository;

        private readonly ContainerApiService _ctApiService;
        private readonly ValidateApiService _validateApiService;

        private readonly ProxmoxSshConfig _proxmoxSshConfig;

        public ContainerController(ILogger<ContainerController> logger,
            IContainerTemplateRepository templateRepository,
            IAppUserRepository appUserRepository,
            ICourseRepository courseRepository,
            IOptions<ProxmoxSshConfig> sshConfig,
            ContainerApiService ctApiService,
            ValidateApiService validateApiService)
        {
            _logger = logger;

            _containerTemplateRepository = templateRepository;
            _appUserRepository = appUserRepository;
            _courseRepository = courseRepository;

            _proxmoxSshConfig = sshConfig.Value;

            _ctApiService = ctApiService;
            _validateApiService = validateApiService;
        }

        // GET: Container
        public async Task<IActionResult> Index()
        {
            var currentUserId = User.GetId();

            try
            {
                var containerDTOs = await _ctApiService.GetAll(includeUsers: true);
                containerDTOs = (containerDTOs != null && !string.IsNullOrEmpty(currentUserId)) ? containerDTOs.Where(v => v.OwnerId == currentUserId).ToList() : new List<ContainerDTO>();

                var ctListItems = ContainerListItem.ToViewModel(containerDTOs);

                return View(ctListItems);
            }
            catch (HttpRequestException ex)
            {
                TempData["Error"] = ex.Message;
                _logger.LogError(ex.Message);

                return View(new List<ContainerListItem>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return View(new List<ContainerListItem>());
            }
        }

        // GET: Container/Create
        public IActionResult Create()
        {
            ViewData["Templates"] = GetTemplatesAsSelectList();
            return View();
        }

        // POST: Container/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name, CloneId")] ContainerCreate containerViewModel)
        {
            var createRequest = new ContainerCreateRequest
            {
                Id = Guid.NewGuid(),
                TimeStamp = DateTime.Now,
                Name = containerViewModel.Name,

                OwnerId = User.GetId(),
                CloneId = containerViewModel.CloneId,
            };

            var isValid = await _validateApiService.ValidateName(createRequest.Name);
            if (!isValid)
            {
                ModelState.AddModelError("Name", $"{containerViewModel.Name} is taken");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //call API
                    var response = await _ctApiService.CreateContainerAsync(createRequest);
                    ViewBag.Response = response;

                    TempData["Message"] = string.Format("Creatie van container {0} is in behandeling", createRequest.Name);
                    return RedirectToAction(nameof(Index));
                }
                catch (KeyNotFoundException) //TODO: aanpassen naar authentication (Identity) ipv exception op not logged in
                {
                    TempData["Error"] = "Not logged in!";

                    ViewData["Templates"] = GetTemplatesAsSelectList();
                    return View(containerViewModel);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = ex.Message;

                    ViewData["Templates"] = GetTemplatesAsSelectList();
                    return View(containerViewModel);
                }
            }

            ViewData["Templates"] = GetTemplatesAsSelectList();
            return View(containerViewModel);
        }

        // GET: Container/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            ContainerDelete containerDelete;

            try
            {
                var container = await _ctApiService.GetById(id, includeUsers: true);
                containerDelete = ContainerDelete.ToViewModel(container);
            }
            catch (ContainerNotFoundException e)
            {
                TempData["Error"] = e.Message;
                return RedirectToAction(nameof(Index));
            }

            return View(containerDelete);
        }

        // POST: Container/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var container = await _ctApiService.GetById(id, includeUsers: true);

                var removeRequest = new ContainerRemoveRequest
                {
                    Id = Guid.NewGuid(),
                    TimeStamp = DateTime.UtcNow,
                    Name = container.Name,
                    VmId = container.Id,
                    OwnerId = User.GetId(),
                };

                //call API
                var response = await _ctApiService.RemoveContainerAsync(removeRequest);
                ViewBag.Response = response;

                TempData["Message"] = string.Format("Verwijderen van container {0} is in behandeling", container.Name);
            }
            catch (ContainerNotFoundException e)
            {
                TempData["Error"] = e.Message;
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Container/Start/101
        public IActionResult Start(int id)
        {
            using (var client = new SshClient(_proxmoxSshConfig.Endpoint, _proxmoxSshConfig.UserName, _proxmoxSshConfig.Password))
            {
                client.Connect();

                client.RunCommand("pct start " + id);

                client.Disconnect();
            }

            TempData["Message"] = string.Format("Container wordt gestart...");

            return RedirectToAction(nameof(Index));
        }

        //Gets the available templates for the current user
        private SelectList GetTemplatesAsSelectList()
        {
            var templates = _containerTemplateRepository.GetByUserId(User.GetId()).Result;
            var templateListItems = ContainerTemplateListItem.ToViewModel(templates);

            var selectList = new SelectList(templateListItems.OrderBy(x => x.Name),
                nameof(ContainerTemplateListItem.ProxmoxId),
                nameof(ContainerTemplateListItem.Name));

            return selectList;
        }
    }
}
