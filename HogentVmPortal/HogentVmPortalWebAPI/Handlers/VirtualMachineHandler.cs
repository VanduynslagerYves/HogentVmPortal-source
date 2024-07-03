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

                var vmArgs = ProxmoxVirtualMachineCreateParams.FromViewModel(createRequest);

                pulumiProvider = new ProxmoxStrategy(_proxmoxConfig.Value); //TODO: init in base

                var projectName = "pulumi_inline";
                var stackName = vmArgs.VmName;

                var pulumiFn = pulumiProvider.CreateVirtualMachine(vmArgs);
                var stackArgs = new InlineProgramArgs(projectName, stackName, pulumiFn);
                var stack = await LocalWorkspace.CreateOrSelectStackAsync(stackArgs);

                //Provision the virtual machine
                var result = await stack.UpAsync(new UpOptions { OnStandardOutput = Console.WriteLine, ShowSecrets = true });

                var virtualMachine = new VirtualMachine
                {
                    Id = Guid.NewGuid(),
                    Name = createRequest.Name,
                    Owner = owner,
                    Template = template,
                    IpAddress = GetValue(result, "ip", value => value.ToString()) ?? string.Empty,
                    ProxmoxId = GetValue(result, "id", value => int.Parse(value.ToString()!)),
                    Login = GetValue(result, "login", value => value.ToString()) ?? string.Empty
                };

                await _virtualMachineRepository.Add(virtualMachine);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static T? GetValue<T>(UpResult? result, string key, Func<object, T> convertFunc)
        {
            return result != null && result.Outputs.TryGetValue(key, out var outputValue) ? convertFunc(outputValue.Value) : default;
        }

        public async Task HandleVirtualMachineRemoveRequest(VirtualMachineRemoveRequest removeRequest)
        {
            if (removeRequest == null) return;

            ProviderStrategy? pulumiProvider;

            try
            {
                var vmArgs = ProxmoxVirtualMachineDeleteParams.FromViewModel(removeRequest);

                pulumiProvider = new ProxmoxStrategy(_proxmoxConfig.Value);

                var projectName = "pulumi_inline";
                var stackName = vmArgs.VmName;

                var pulumiFn = pulumiProvider.RemoveVirtualMachine(vmArgs);
                var stackArgs = new InlineProgramArgs(projectName, stackName, pulumiFn);
                var stack = await LocalWorkspace.SelectStackAsync(stackArgs);

                await stack.DestroyAsync(new DestroyOptions { OnStandardOutput = Console.WriteLine }); //destroying the stack only removes the resources in the stack
                await stack.Workspace.RemoveStackAsync(removeRequest.Name); //use workspace property to remove the now empty stack

                await _virtualMachineRepository.Delete(removeRequest.VmId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
