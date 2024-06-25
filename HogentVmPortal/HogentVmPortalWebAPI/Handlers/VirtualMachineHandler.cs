using HogentVmPortal.Shared.Model;
using Pulumi.Automation;
using HogentVmPortalWebAPI.ProviderStrategies;
using HogentVmPortal.Shared.DTO;
using HogentVmPortalWebAPI.Data.Repositories;
using Microsoft.Extensions.Options;
using HogentVmPortal.Shared;

namespace HogentVmPortalWebAPI.Handlers
{
    public class VirtualMachineHandler
    {
        private readonly IVirtualMachineRepository _virtualMachineRepository;
        private readonly IVirtualMachineTemplateRepository _virtualMachineTemplateRepository;
        private readonly IAppUserRepository _appUserRepository;
        private readonly IOptions<ProxmoxConfig> _proxmoxConfig;

        public VirtualMachineHandler(IVirtualMachineRepository virtualMachineRepository, IAppUserRepository appUserRepository, IVirtualMachineTemplateRepository virtualMachineTemplateRepository,
            IOptions<ProxmoxConfig> proxmoxConfig)
        {
            _virtualMachineRepository = virtualMachineRepository;
            _virtualMachineTemplateRepository = virtualMachineTemplateRepository;
            _appUserRepository = appUserRepository;
            _proxmoxConfig = proxmoxConfig;
        }

        /* Rework with webapi and event-based dequeue:
         * no polling of db every 5sec
         * Handler code will only be executed if there's a request in the queue (event-based)
         */
        public async Task HandleVirtualMachineCreateRequest(VirtualMachineCreateRequest createRequest)
        {
            if (createRequest == null) return;

            //TODO: init in caller (IOC), pulumiProvider should not have access to .CreateContainer here (ISP - factory pattern)
            //pulumiProvider should have a Factory (CreatorFactory)
            //use the Factory to return the correct Creator class based on the provider, with their CreateVirtualMachine or CreateContainer method
            ProviderStrategy? pulumiProvider;

            try
            {
                var owner = await _appUserRepository.GetById(createRequest.OwnerId);
                var template = await _virtualMachineTemplateRepository.GetByCloneId(createRequest.CloneId);

                var virtualMachine = new VirtualMachine
                {
                    Id = Guid.NewGuid(),
                    Name = createRequest.Name,
                    Owner = owner,
                    Template = template,
                };

                var vmArgs = new ProxmoxVirtualMachineCreateParams()
                {
                    Login = createRequest.Login,
                    Password = createRequest.Password,
                    VmName = createRequest.Name,
                    CloneId = createRequest.CloneId,
                    SshKey = createRequest.SshKey,
                };

                pulumiProvider = new ProxmoxStrategy(_proxmoxConfig.Value); //TODO: init in base

                var projectName = "pulumi_inline";
                var stackName = vmArgs.VmName;

                var virtualMachineDefinition = pulumiProvider.CreateVirtualMachine(vmArgs);
                var stackArgs = new InlineProgramArgs(projectName, stackName, virtualMachineDefinition);
                var stack = await LocalWorkspace.CreateOrSelectStackAsync(stackArgs);

                //use Task.Run if the method called is CPU bound (work is done in a separate background thread)
                //var result = await Task.Run(() => stack.UpAsync(new UpOptions { OnStandardOutput = Console.WriteLine, ShowSecrets = true }));
                //Provision the virtual machine
                var result = await stack.UpAsync(new UpOptions { OnStandardOutput = Console.WriteLine, ShowSecrets = true });

                //Add extra data to the virtual machine object once it is provisioned.
                //TODO: look for a way to get the ip from Pulumi state (cloud)
                if (result.Outputs.TryGetValue("ip", out var ip)) virtualMachine.IpAddress = ip.Value.ToString();
                if (result.Outputs.TryGetValue("id", out var proxmoxId)) virtualMachine.ProxmoxId = int.Parse(proxmoxId.Value.ToString()!);
                if (result.Outputs.TryGetValue("login", out var login)) virtualMachine.Login = login.Value.ToString();

                var t = await stack.GetInfoAsync();
                await _virtualMachineRepository.Add(virtualMachine);
                await _virtualMachineRepository.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public async Task HandleVirtualMachineRemoveRequest(VirtualMachineRemoveRequest removeRequest)
        {
            if (removeRequest == null) return;

            ProviderStrategy? pulumiProvider;

            try
            {
                var vmArgs = new ProxmoxVirtualMachineDeleteParams()
                {
                    VmName = removeRequest.Name,
                };

                pulumiProvider = new ProxmoxStrategy(_proxmoxConfig.Value);

                var projectName = "pulumi_inline";
                var stackName = vmArgs.VmName;

                var pulumiVm = pulumiProvider.RemoveVirtualMachine(vmArgs);
                var stackArgs = new InlineProgramArgs(projectName, stackName, pulumiVm);
                var stack = await LocalWorkspace.SelectStackAsync(stackArgs);

                await stack.DestroyAsync(new DestroyOptions { OnStandardOutput = Console.WriteLine }); //destroying the stack only removes the resources in the stack
                await stack.Workspace.RemoveStackAsync(removeRequest.Name); //use workspace property to remove the now empty stack

                await _virtualMachineRepository.Delete(removeRequest.VmId);
                await _virtualMachineRepository.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
