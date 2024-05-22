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

namespace HogentVmPortal.Controllers
{
    public class VirtualMachineController : Controller
    {
        private readonly IVirtualMachineRepository _vmRepository;
        private readonly IVirtualMachineTemplateRepository _vmTemplateRepository;
        private readonly IVirtualMachineRequestRepository _vmRequestRepository;
        private readonly ICourseRepository _courseRepository;

        private readonly IAppUserRepository _appUserRepository;

        private readonly ProxmoxSshConfig _proxmoxSshConfig;

        public VirtualMachineController(IVirtualMachineRepository virtualMachineRepository,
            IVirtualMachineTemplateRepository templateRepository,
            IAppUserRepository appUserRepository,
            IVirtualMachineRequestRepository virtualMachineRequestRepository,
            ICourseRepository courseRepository,
            IOptions<ProxmoxSshConfig> sshConfig)
        {
            _vmRepository = virtualMachineRepository;
            _vmTemplateRepository = templateRepository;
            _appUserRepository = appUserRepository;
            _vmRequestRepository = virtualMachineRequestRepository;
            _courseRepository = courseRepository;

            _proxmoxSshConfig = sshConfig.Value;
        }

        // GET: VirtualMachine
        public async Task<IActionResult> Index()
        {
            var currentUserId = User.GetId();

            var virtualMachines = await _vmRepository.GetAll(includeUsers: true);
            virtualMachines = virtualMachines.Where(v => v.Owner.Id == currentUserId).ToList();

            return View(VirtualMachineListItem.ToViewModel(virtualMachines));
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
            if (_vmRepository.VirtualMachineNameExists(virtualMachineViewModel.Name))
            {
                ModelState.AddModelError("Name", $"{virtualMachineViewModel.Name} is taken");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var currentUserId = User.GetId();

                    var createRequest = new VirtualMachineCreateRequest
                    {
                        Id = Guid.NewGuid(),
                        TimeStamp = DateTime.Now,
                        Name = virtualMachineViewModel.Name,

                        OwnerId = currentUserId,
                        CloneId = virtualMachineViewModel.CloneId,

                        //a full implementation can determine what to do here. auto-generate and send via e-mail?
                        Login = virtualMachineViewModel.Login,
                        Password = virtualMachineViewModel.Password,

                        SshKey = virtualMachineViewModel.SshKey,
                    };

                    await _vmRequestRepository.Add(createRequest);
                    await _vmRequestRepository.SaveChangesAsync();

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

        // GET: VirtualMachine/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            VirtualMachineEdit? virtualMachineEdit;
            try
            {
                var virtualMachine = await _vmRepository.GetById(id);
                ViewData["Name"] = virtualMachine.Name;
                virtualMachineEdit = VirtualMachineEdit.ToViewModel(virtualMachine);
            }
            catch (VirtualMachineNotFoundException e)
            {
                TempData["Error"] = e.Message;
                return RedirectToAction(nameof(Index));
            }

            return View(virtualMachineEdit);
        }

        // POST: VirtualMachine/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,NewPassword")] VirtualMachineEdit virtualMachineEdit)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var editRequest = new VirtualMachineEditRequest
        //            {
        //                Id = Guid.NewGuid(),
        //                TimeStamp = DateTime.Now,
        //                VmId = id,
        //            };

        //            await _vmRequestRepository.Add(editRequest);
        //            await _vmRequestRepository.SaveChangesAsync();

        //            TempData["Message"] = string.Format("Wachtwoordreset voor VM {0} is in behandeling", virtualMachineEdit.Name);
        //            return RedirectToAction(nameof(Index));
        //        }
        //        catch (Exception ex)
        //        {
        //            TempData["Error"] = ex.Message;
        //            return View(virtualMachineEdit);
        //        }
        //    }

        //    return View(virtualMachineEdit);
        //}

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

                var request = new VirtualMachineRemoveRequest
                {
                    Id = Guid.NewGuid(),
                    TimeStamp = DateTime.UtcNow,
                    Name = virtualMachine.Name,
                    VmId = virtualMachine.Id,
                };

                await _vmRequestRepository.Add(request);
                await _vmRequestRepository.SaveChangesAsync();

                TempData["Message"] = string.Format("Verwijderen van VM {0} is in behandeling", virtualMachine.Name);
            }
            catch (VirtualMachineNotFoundException e)
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
