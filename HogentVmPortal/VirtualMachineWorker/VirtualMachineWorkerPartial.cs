using HogentVmPortal.Shared.Model;
using Pulumi.Automation;
using VirtualMachineWorker.PulumiStrategy;

namespace VirtualMachineWorker
{
    public partial class Worker
    {
        //https://www.pulumi.com/docs/using-pulumi/automation-api/getting-started-automation-api/
        private async Task HandleVirtualMachineCreateRequests()
        {
            var createRequests = await _virtualMachineRequestRepository.GetAllCreateRequests();
            createRequests = createRequests.OrderBy(x => x.TimeStamp).ToList();
            if (!createRequests.Any()) return;

            ProviderStrategy? pulumiProvider; //TODO: init in Worker
            foreach (var createRequest in createRequests)
            {
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

                    pulumiProvider = new ProxmoxStrategy(_proxmoxConfig.Value); //TODO: init in Worker

                    var projectName = "pulumi_inline";
                    var stackName = vmArgs.VmName;

                    var virtualMachineDefinition = pulumiProvider.CreateVirtualMachine(vmArgs);
                    var stackArgs = new InlineProgramArgs(projectName, stackName, virtualMachineDefinition);
                    var stack = await LocalWorkspace.CreateOrSelectStackAsync(stackArgs);

                    //var result = await Task.Run(() => stack.UpAsync(new UpOptions { OnStandardOutput = Console.WriteLine, ShowSecrets = true }));
                    //Provision the virtual machine
                    var result = await stack.UpAsync(new UpOptions { OnStandardOutput = Console.WriteLine, ShowSecrets = true });

                    //Add extra data to the virtual machine object once it is provisioned.
                    if (result.Outputs.TryGetValue("ip", out var ip)) virtualMachine.IpAddress = ip.Value.ToString();
                    if (result.Outputs.TryGetValue("id", out var proxmoxId)) virtualMachine.ProxmoxId = int.Parse(proxmoxId.Value.ToString()!);
                    if (result.Outputs.TryGetValue("login", out var login)) virtualMachine.Login = login.Value.ToString();

                    await _virtualMachineRepository.Add(virtualMachine);
                    await _virtualMachineRepository.SaveChangesAsync();

                    _virtualMachineRequestRepository.Delete(createRequest);
                    await _virtualMachineRequestRepository.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);

                    //Delete the request
                    _virtualMachineRequestRepository.Delete(createRequest);
                    await _virtualMachineRequestRepository.SaveChangesAsync();
                }
            }
        }

        //private async Task HandleVirtualMachineEditRequests()
        //{
        //    var editRequests = await _virtualMachineRequestRepository.GetAllEditRequests();
        //    editRequests = editRequests.OrderBy(x => x.TimeStamp).ToList();

        //    ProviderStrategy? pulumiProvider;

        //    if (!editRequests.Any()) return;

        //    foreach (var editRequest in editRequests)
        //    {
        //        try
        //        {
        //            var virtualMachine = await _virtualMachineRepository.GetById(editRequest.VmId);

        //            //TODO: build these with builder or factory, based on selected type in the VirtualMachineCreate viewmodel
        //            var vmArgs = new ProxmoxVirtualMachineEditParams()
        //            {
        //                VmName = virtualMachine.Name,
        //                Login = virtualMachine.Login!,
        //                //Password = editRequest.Password,
        //                CloneId = virtualMachine.Template!.ProxmoxId,
        //                //SshKey = editRequest.SshKey,
        //            };

        //            //TODO: build these with builder or factory, based on selected type in the VirtualMachineCreate viewmodel
        //            pulumiProvider = new ProxmoxStrategy(_proxmoxConfig.Value);

        //            var projectName = "pulumi_inline";
        //            var stackName = vmArgs.VmName;
        //            var pulumiVm = pulumiProvider.EditVirtualMachine(vmArgs);

        //            var stackArgs = new InlineProgramArgs(projectName, stackName, pulumiVm);
        //            var stack = await LocalWorkspace.SelectStackAsync(stackArgs);

        //            var result = await stack.UpAsync(new UpOptions { OnStandardOutput = Console.WriteLine, ShowSecrets = true });

        //            //Changing the stack might change the original ip, save it again.
        //            if (result.Outputs.TryGetValue("ip", out var ip)) virtualMachine.IpAddress = ip.Value.ToString();
        //            //if (result.Outputs.TryGetValue("login", out var login)) virtualMachine.Login = login.Value.ToString();

        //            _virtualMachineRepository.Update(virtualMachine);
        //            await _virtualMachineRepository.SaveChangesAsync();

        //            _virtualMachineRequestRepository.Delete(editRequest);
        //            await _virtualMachineRequestRepository.SaveChangesAsync();
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine(e.Message);

        //            //Delete the request
        //            _virtualMachineRequestRepository.Delete(editRequest);
        //            await _virtualMachineRequestRepository.SaveChangesAsync();
        //        }
        //    }
        //}

        private async Task HandleVirtualMachineRemoveRequests()
        {
            var removeRequests = await _virtualMachineRequestRepository.GetAllRemoveRequests();
            removeRequests = removeRequests.OrderBy(x => x.TimeStamp).ToList();
            if (!removeRequests.Any()) return;

            ProviderStrategy? pulumiProvider;

            foreach (var removeRequest in removeRequests)
            {
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

                    _virtualMachineRequestRepository.Delete(removeRequest);
                    await _virtualMachineRequestRepository.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);

                    //Delete the request
                    _virtualMachineRequestRepository.Delete(removeRequest);
                    await _virtualMachineRequestRepository.SaveChangesAsync();
                }
            }
        }
    }
}
