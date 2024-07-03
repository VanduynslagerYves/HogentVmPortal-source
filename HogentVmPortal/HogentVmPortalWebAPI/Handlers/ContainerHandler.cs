using HogentVmPortal.Shared.Model;
using Pulumi.Automation;
using Renci.SshNet;
using System.Text.RegularExpressions;
using HogentVmPortalWebAPI.ProviderStrategies;
using HogentVmPortalWebAPI.Data.Repositories;
using HogentVmPortal.Shared.DTO;
using HogentVmPortal.Shared;
using Microsoft.Extensions.Options;

namespace HogentVmPortalWebAPI.Handlers
{
    public class ContainerHandler
    {
        private readonly Regex _ipRegex = new Regex(@"\b(?:\d{1,3}\.){3}\d{1,3}\b", RegexOptions.Compiled);
        private const string IPLOGFILENAME = "iplog";

        private readonly IContainerRepository _containerRepository;
        private readonly IContainerTemplateRepository _containerTemplateRepository;
        private readonly IAppUserRepository _appUserRepository;
        private readonly IOptions<ProxmoxConfig> _proxmoxConfig;
        private readonly IOptions<ProxmoxSshConfig> _proxmoxSshConfig;

        public ContainerHandler(IContainerRepository containerRepository, IAppUserRepository appUserRepository, IContainerTemplateRepository containerTemplateRepository,
            IOptions<ProxmoxConfig> proxmoxConfig, IOptions<ProxmoxSshConfig> proxmoxSshConfig)
        {
            _containerRepository = containerRepository;
            _appUserRepository = appUserRepository;
            _containerTemplateRepository = containerTemplateRepository;
            _proxmoxConfig = proxmoxConfig;
            _proxmoxSshConfig = proxmoxSshConfig;
        }

        public async Task HandleContainerCreateRequest(ContainerCreateRequest createRequest)
        {
            if (createRequest == null) return;

            ProviderStrategy? pulumiProvider;
            try
            {
                var owner = await _appUserRepository.GetById(createRequest.OwnerId);
                var template = await _containerTemplateRepository.GetByCloneId(createRequest.CloneId);

                var vmArgs = ProxmoxContainerCreateParams.FromViewModel(createRequest);

                pulumiProvider = new ProxmoxStrategy(_proxmoxConfig.Value);

                var projectName = "pulumi_inline";
                var stackName = vmArgs.ContainerName;

                var pulumiFn = pulumiProvider.CreateContainer(vmArgs);
                var stackArgs = new InlineProgramArgs(projectName, stackName, pulumiFn);
                var stack = await LocalWorkspace.CreateOrSelectStackAsync(stackArgs);

                //Provision the container
                var result = await stack.UpAsync(new UpOptions { OnStandardOutput = Console.WriteLine });

                var proxmoxId = int.Parse(GetValue(result, "id", value => value.ToString())!);
                var ip = await GetIpForContainerId(proxmoxId);

                var container = new Container
                {
                    Id = Guid.NewGuid(),
                    Name = createRequest.Name,
                    Owner = owner,
                    Template = template,
                    ProxmoxId = proxmoxId,
                    IpAddress = ip
                };

                await _containerRepository.Add(container);
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

        public async Task HandleContainerRemoveRequest(ContainerRemoveRequest removeRequest)
        {
            if (removeRequest == null) return;

            ProviderStrategy? pulumiProvider;

            try
            {
                var vmArgs = ProxmoxContainerRemoveParams.FromViewModel(removeRequest);

                pulumiProvider = new ProxmoxStrategy(_proxmoxConfig.Value);

                var projectName = "pulumi_inline";
                var stackName = vmArgs.ContainerName;

                var containerDefinition = pulumiProvider.RemoveContainer(vmArgs);
                var stackArgs = new InlineProgramArgs(projectName, stackName, containerDefinition);
                var stack = await LocalWorkspace.SelectStackAsync(stackArgs);

                await stack.DestroyAsync(new DestroyOptions { OnStandardOutput = Console.WriteLine }); //destroying the stack only removes the resources in the stack
                await stack.Workspace.RemoveStackAsync(removeRequest.Name); //use workspace property to remove the now empty stack

                await _containerRepository.Delete(removeRequest.VmId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Creates an SSH connection to the proxmox server and lets the proxmox server query the container ip address
        /// </summary>
        /// <param name="id">The proxmox id of the container</param>
        /// <returns>The ip adddress of the container</returns>
        private async Task<string> GetIpForContainerId(int id)
        {
            var ip = string.Empty;
            var retryCount = 0;

            using (var client = new SshClient(_proxmoxSshConfig.Value.Endpoint, _proxmoxSshConfig.Value.UserName, _proxmoxSshConfig.Value.Password))
            {
                try
                {
                    client.Connect();

                    while (string.IsNullOrEmpty(ip) && retryCount < 5)
                    {
                        //outputs the ip address of the container and writes to filename specified in IPLOGFILENAME
                        await RunCommandAsync(client, "lxc-info -i -n " + id + " > " + IPLOGFILENAME);

                        var result = await RunCommandAsync(client, "cat " + IPLOGFILENAME);

                        var match = _ipRegex.Match(result);
                        if (match.Success)
                        {
                            ip = match.Value;
                            break;
                        }

                        retryCount++;
                        await Task.Delay(2000);
                    }

                    client.Disconnect();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            if (string.IsNullOrEmpty(ip)) { ip = "no ip found"; }
            return ip;
        }

        private async Task<string> RunCommandAsync(SshClient client, string command)
        {
            var commandResult = await Task.Run(() =>
            {
                var result = client.RunCommand(command);
                return result.Result;
            });

            return commandResult;
        }
    }
}
