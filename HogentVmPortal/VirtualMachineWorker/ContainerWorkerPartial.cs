using HogentVmPortal.Shared.Model;
using Pulumi.Automation; //automation API
using Renci.SshNet;
using System.Text.RegularExpressions;
using VirtualMachineWorker.PulumiStrategy;

namespace VirtualMachineWorker
{
    public partial class Worker
    {
        private readonly Regex _ipRegex = new Regex(@"\b(?:\d{1,3}\.){3}\d{1,3}\b", RegexOptions.Compiled);
        private const string IPLOGFILENAME = "iplog";

        private async Task HandleContainerCreateRequests()
        {
            var createRequests = await _containerRequestRepository.GetAllCreateRequests();
            createRequests = createRequests.OrderBy(x => x.TimeStamp).ToList();

            if (!createRequests.Any()) return;

            ProviderStrategy? pulumiProvider;
            foreach (var createRequest in createRequests)
            {
                try
                {
                    var owner = await _appUserRepository.GetById(createRequest.OwnerId);
                    var template = await _containerTemplateRepository.GetByCloneId(createRequest.CloneId);

                    var container = new Container
                    {
                        Id = Guid.NewGuid(),
                        Name = createRequest.Name,
                        Owner = owner,
                        Template = template,
                    };

                    var vmArgs = new ProxmoxContainerCreateParams()
                    {
                        ContainerName = createRequest.Name,
                        CloneId = createRequest.CloneId,
                    };

                    pulumiProvider = new ProxmoxStrategy(_proxmoxConfig.Value);

                    var projectName = "pulumi_inline";
                    var stackName = vmArgs.ContainerName;

                    var containerDefinition = pulumiProvider.CreateContainer(vmArgs);
                    var stackArgs = new InlineProgramArgs(projectName, stackName, containerDefinition);
                    var stack = await LocalWorkspace.CreateOrSelectStackAsync(stackArgs);

                    //var result = await Task.Run(() => stack.UpAsync(new UpOptions { OnStandardOutput = Console.WriteLine, ShowSecrets = true }));
                    //Provision the container
                    var result = await stack.UpAsync(new UpOptions { OnStandardOutput = Console.WriteLine });

                    //Add extra data to the container object once it is provisioned.
                    if (result.Outputs.TryGetValue("id", out var proxmoxId))
                    {
                        container.ProxmoxId = int.Parse(proxmoxId.Value.ToString()!);
                        container.IpAddress = await GetIpForContainerId(container.ProxmoxId!.Value);
                    }

                    await _containerRepository.Add(container);
                    await _containerRepository.SaveChangesAsync();

                    _containerRequestRepository.Delete(createRequest);
                    await _containerRequestRepository.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);

                    //Delete the request
                    _containerRequestRepository.Delete(createRequest);
                    await _containerRequestRepository.SaveChangesAsync();
                }
            }
        }

        //private async Task HandleContainerEditRequests()
        //{
        //    var editRequests = await _containerRequestRepository.GetAllEditRequests();
        //    editRequests = editRequests.OrderBy(x => x.TimeStamp).ToList();

        //    ProviderStrategy? pulumiProvider;

        //    if (!editRequests.Any()) return;

        //    foreach (var editRequest in editRequests)
        //    {
        //        try
        //        {
        //            var container = await _containerRepository.GetById(editRequest.VmId);

        //            var vmArgs = new ProxmoxContainerEditParams()
        //            {
        //                ContainerName = container.Name,
        //                CloneId = container.Template!.ProxmoxId,
        //            };

        //            pulumiProvider = new ProxmoxStrategy(_proxmoxConfig.Value);

        //            var projectName = "pulumi_inline";
        //            var stackName = vmArgs.ContainerName;
        //            var containerDefinition = pulumiProvider.EditContainer(vmArgs);

        //            var stackArgs = new InlineProgramArgs(projectName, stackName, containerDefinition);
        //            var stack = await LocalWorkspace.SelectStackAsync(stackArgs);

        //            var result = await stack.UpAsync(new UpOptions { OnStandardOutput = Console.WriteLine, ShowSecrets = true });

        //            //Changing the stack might change the original ip, save it again.
        //            //Add extra data to the container object once it is provisioned.
        //            if (result.Outputs.TryGetValue("id", out var proxmoxId))
        //            {
        //                container.ProxmoxId = int.Parse(proxmoxId.Value.ToString()!);
        //                container.IpAddress = await GetIpForContainerId(container.ProxmoxId!.Value);
        //            }

        //            _containerRepository.Update(container);
        //            await _containerRepository.SaveChangesAsync();

        //            _containerRequestRepository.Delete(editRequest);
        //            await _containerRequestRepository.SaveChangesAsync();
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine(e.Message);

        //            //Delete the request
        //            _containerRequestRepository.Delete(editRequest);
        //            await _containerRequestRepository.SaveChangesAsync();
        //        }
        //    }
        //}

        private async Task HandleContainerRemoveRequests()
        {
            var removeRequests = await _containerRequestRepository.GetAllRemoveRequests();
            removeRequests = removeRequests.OrderBy(x => x.TimeStamp).ToList();

            if (!removeRequests.Any()) return;

            ProviderStrategy? pulumiProvider;

            foreach (var removeRequest in removeRequests)
            {
                try
                {
                    var vmArgs = new ProxmoxContainerDeleteParams()
                    {
                        ContainerName = removeRequest.Name,
                    };

                    pulumiProvider = new ProxmoxStrategy(_proxmoxConfig.Value);

                    var projectName = "pulumi_inline";
                    var stackName = vmArgs.ContainerName;

                    var containerDefinition = pulumiProvider.RemoveContainer(vmArgs);
                    var stackArgs = new InlineProgramArgs(projectName, stackName, containerDefinition);
                    var stack = await LocalWorkspace.SelectStackAsync(stackArgs);

                    await stack.DestroyAsync(new DestroyOptions { OnStandardOutput = Console.WriteLine }); //destroying the stack only removes the resources in the stack
                    await stack.Workspace.RemoveStackAsync(removeRequest.Name); //use workspace property to remove the now empty stack

                    await _containerRepository.Delete(removeRequest.VmId);
                    await _containerRepository.SaveChangesAsync();

                    _containerRequestRepository.Delete(removeRequest);
                    await _containerRequestRepository.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);

                    //Delete the request
                    _containerRequestRepository.Delete(removeRequest);
                    await _containerRequestRepository.SaveChangesAsync();
                }
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
                        //client.RunCommand("lxc-info -i -n " + id + " > " + IPLOGFILENAME);
                        await RunCommandAsync(client, "lxc-info -i -n " + id + " > " + IPLOGFILENAME);
                        //var result = client.RunCommand("cat " + IPLOGFILENAME);
                        var result = await RunCommandAsync(client, "cat " + IPLOGFILENAME);
                        //ip = result.Result;

                        //var match = _ipRegex.Match(ip);
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
                catch(Exception e)
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
