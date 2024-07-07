using Microsoft.AspNetCore.Mvc;
using HogentVmPortal.Shared.Model;
using HogentVmPortal.Data.Repositories;
using HogentVmPortal.Shared.ViewModel;
using HogentVmPortal.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Renci.SshNet;
using Microsoft.Extensions.Options;
using HogentVmPortal.Shared.DTO;
using HogentVmPortal.Shared;
using HogentVmPortal.Services;

namespace HogentVmPortal.Controllers
{
    public class VirtualMachineController : Controller
    {
        private readonly ILogger<VirtualMachineController> _logger;

        private readonly IVirtualMachineRepository _vmRepository;
        private readonly IVirtualMachineTemplateRepository _vmTemplateRepository;
        private readonly ICourseRepository _courseRepository;

        private readonly IAppUserRepository _appUserRepository;

        private readonly VirtualMachineApiService _vmApiService;

        private readonly ProxmoxSshConfig _proxmoxSshConfig;

        public VirtualMachineController(ILogger<VirtualMachineController> logger,
            IVirtualMachineRepository virtualMachineRepository,
            IVirtualMachineTemplateRepository templateRepository,
            IAppUserRepository appUserRepository,
            ICourseRepository courseRepository,
            IOptions<ProxmoxSshConfig> sshConfig,
            VirtualMachineApiService vmApiService)
        {
            _logger = logger;

            _vmRepository = virtualMachineRepository;
            _vmTemplateRepository = templateRepository;
            _appUserRepository = appUserRepository;
            _courseRepository = courseRepository;

            _proxmoxSshConfig = sshConfig.Value;

            _vmApiService = vmApiService;
        }

        // GET: VirtualMachine
        public async Task<IActionResult> Index()
        {
            var currentUserId = User.GetId();

            try
            {
                var virtualMachineDTOs = await _vmApiService.GetAll(includeUsers: true);
                virtualMachineDTOs = (virtualMachineDTOs != null && !string.IsNullOrEmpty(currentUserId)) ? virtualMachineDTOs.Where(v => v.OwnerId == currentUserId).ToList() : new List<VirtualMachineDTO>();

                var vmListItems = VirtualMachineListItem.ToViewModel(virtualMachineDTOs);

                return View(vmListItems);
            }
            catch (HttpRequestException ex)
            {
                TempData["Error"] = ex.Message;
                _logger.LogError(ex.Message);

                return View(new List<VirtualMachineListItem>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return View(new List<VirtualMachineListItem>());
            }
        }

        // GET: VirtualMachine/Create
        public IActionResult Create()
        {
            ViewData["Templates"] = GetTemplatesAsSelectList();
            return View();
        }

        // POST: VirtualMachine/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name, Login, Password, SshKey, CloneId")] VirtualMachineCreate virtualMachineViewModel)
        {
            var createRequest = new VirtualMachineCreateRequest
            {
                Id = Guid.NewGuid(), //Not needed right now, will be needed when the request is saved and passed on to other microservice
                TimeStamp = DateTime.Now,
                Name = virtualMachineViewModel.Name,

                OwnerId = User.GetId(),
                CloneId = virtualMachineViewModel.CloneId,

                //a full implementation can determine what to do here. auto-generate and send via e-mail?
                Login = virtualMachineViewModel.Login,
                Password = virtualMachineViewModel.Password,

                SshKey = virtualMachineViewModel.SshKey,
            };

            var isValid = await _vmApiService.Validate(createRequest);
            if (!isValid)
            {
                ModelState.AddModelError("Name", $"{virtualMachineViewModel.Name} is taken");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //call API, TODO: publish data to rabbitMQ and save request in request db
                    var response = await _vmApiService.CreateVmAsync(createRequest);
                    ViewBag.Response = response;

                    TempData["Message"] = string.Format("Creatie van VM {0} is in behandeling", createRequest.Name);
                    return RedirectToAction(nameof(Index));
                }
                catch (KeyNotFoundException) //TODO: aanpassen naar Authorized ipv exception op not logged in
                {
                    TempData["Error"] = "Not logged in!";

                    ViewData["Templates"] = GetTemplatesAsSelectList();
                    return View(virtualMachineViewModel);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = ex.Message;

                    ViewData["Templates"] = GetTemplatesAsSelectList();
                    return View(virtualMachineViewModel);
                }
            }

            ViewData["Templates"] = GetTemplatesAsSelectList();
            return View(virtualMachineViewModel);
        }

        // GET: VirtualMachine/Delete/5
        public async Task<IActionResult> Delete(Guid id)
        {
            VirtualMachineDelete virtualMachineDelete;

            try
            {
                var virtualMachine = await _vmRepository.GetById(id);
                virtualMachineDelete = VirtualMachineDelete.ToViewModel(virtualMachine);
            }
            catch (VirtualMachineNotFoundException e)
            {
                TempData["Error"] = e.Message;
                return RedirectToAction(nameof(Index));
            }

            return View(virtualMachineDelete);
        }

        // POST: VirtualMachine/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var virtualMachine = await _vmRepository.GetById(id);

                var removeRequest = new VirtualMachineRemoveRequest
                {
                    Id = Guid.NewGuid(),
                    TimeStamp = DateTime.UtcNow,
                    Name = virtualMachine.Name,
                    VmId = virtualMachine.Id,
                };

                //call API
                var response = await _vmApiService.RemoveVmAsync(removeRequest);
                ViewBag.Response = response;

                TempData["Message"] = string.Format("Verwijderen van VM {0} is in behandeling", virtualMachine.Name);
            }
            catch (VirtualMachineNotFoundException e)
            {
                TempData["Error"] = e.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                TempData["Error"] = e.Message;
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: VirtualMachine/Start/101
        public IActionResult Start(int id)
        {
            using (var client = new SshClient(_proxmoxSshConfig.Endpoint, _proxmoxSshConfig.UserName, _proxmoxSshConfig.Password))
            {
                client.Connect();

                client.RunCommand("qm start " + id);

                client.Disconnect();
            }

            TempData["Message"] = string.Format("Virtuele machine wordt gestart...");

            return RedirectToAction(nameof(Index));
        }

        //Gets the available templates for the current user
        private SelectList GetTemplatesAsSelectList()
        {
            var templates = _vmTemplateRepository.GetByUserId(User.GetId()).Result;
            var templateListItems = VirtualMachineTemplateListItem.ToViewModel(templates);

            var selectList = new SelectList(templateListItems.OrderBy(x => x.FullDescription),
                nameof(VirtualMachineTemplateListItem.ProxmoxId),
                nameof(VirtualMachineTemplateListItem.FullDescription));

            return selectList;
        }
    }
}
