using HogentVmPortal.Shared;
using HogentVmPortal.Shared.DTO;
using Pulumi.Automation;
using Pulumi.ProxmoxVE.VM.Inputs;
using Pulumi.ProxmoxVE.VM;
using Pulumi.ProxmoxVE;
using Pulumi;

namespace VmHandler.ProviderStrategies
{
    public class ProxmoxStrategy : ProviderStrategy
    {
        private readonly ProxmoxConfig _config;

        public ProxmoxStrategy(ProxmoxConfig config)
        {
            _config = config;
        }

        public override PulumiFn CreateVirtualMachine(VirtualMachineParams vmArgs)
        {
            var createArgs = vmArgs as ProxmoxVirtualMachineCreateParams;
            if (createArgs == null) throw new ArgumentNullException();

            return PulumiFn.Create(() =>
            {
                //ssh config, used to query pulumi state
                //var ssh = new ProviderSshArgs
                //{

                //}

                //proxmox specific
                var provider = new Provider("proxmox_provider", new ProviderArgs
                {
                    Endpoint = _config.Endpoint,
                    Insecure = _config.Insecure,
                    Username = _config.UserName,
                    Password = _config.Password,
                    //Ssh = ssh,
                });

                //proxmox specific
                var virtualMachine = new VirtualMachine(createArgs.VmName, new VirtualMachineArgs
                {
                    NodeName = _config.TargetNode,
                    Name = createArgs.VmName,
                    Agent = new VirtualMachineAgentArgs //QEMU agent, has to be enabled to retreive the ip-address after provisioning (on by default)
                    {
                        Enabled = true,
                        Trim = true,
                        Type = "virtio",
                    },
                    Cpu = new VirtualMachineCpuArgs
                    {
                        Cores = 4,
                        Sockets = 1,
                        Architecture = "x86_64"
                    },
                    Clone = new VirtualMachineCloneArgs
                    {
                        NodeName = _config.SourceNode,
                        VmId = createArgs.CloneId,
                    },
                    Memory = new VirtualMachineMemoryArgs
                    {
                        Dedicated = 4096,
                    },
                    Disks = new VirtualMachineDiskArgs
                    {
                        Interface = "scsi0",
                        Size = 30,
                        FileFormat = "qcow2",
                        Ssd = true,
                    },
                    OperatingSystem = new VirtualMachineOperatingSystemArgs
                    {
                        Type = "l26",
                    },
                    SerialDevices = new VirtualMachineSerialDeviceArgs[] { }, //important as setting for console in proxmox, if not set the vm connections is setup with a serial port resulting in a blank screen.
                    NetworkDevices = new VirtualMachineNetworkDeviceArgs
                    {
                        Bridge = "vmbr0",
                        Model = "virtio",
                    },
                    Initialization = new VirtualMachineInitializationArgs
                    {
                        Type = "nocloud", //'nocloud' for linux, 'configdrive2' for windows
                        DatastoreId = "local-lvm",
                        Dns = new VirtualMachineInitializationDnsArgs
                        {
                            Domain = "localdomain",
                            Servers = "192.168.152.2"
                        },
                        IpConfigs = new VirtualMachineInitializationIpConfigArgs
                        {
                            Ipv4 = new VirtualMachineInitializationIpConfigIpv4Args
                            {
                                Address = "dhcp",
                                Gateway = "192.168.152.2"
                            },
                        },
                        UserAccount = new VirtualMachineInitializationUserAccountArgs //cloud-init must be installed on a linux host
                        {
                            Username = createArgs.Login,
                            Password = createArgs.Password,
                            Keys = new InputList<string>() { createArgs.SshKey },
                        },
                    }
                },
                new CustomResourceOptions
                {
                    Provider = provider,
                });

                //QEMU agent must be enabled, and ip setup must be done via Initialization (cloud-init) in code (see above), otherwise the ip adress is not known until after the host has booted.
                //TODO: look for a way to get the ip from Pulumi state (cloud)
                //notes: these properties effectively already do this.

                //TODO: try catch these. not filled in when an invalid SSH key is given. So validate this too.
                var ip = virtualMachine.Ipv4Addresses.Apply(res => res.LastOrDefault().LastOrDefault());
                var proxmoxId = virtualMachine.Id;
                var login = virtualMachine.Initialization.Apply(res => res!.UserAccount!.Username);
                var password = virtualMachine.Initialization.Apply(res => res!.UserAccount!.Password);

                return new Dictionary<string, object?>
                {
                    ["ip"] = ip,
                    ["id"] = proxmoxId,
                    ["login"] = login,
                    ["password"] = password,
                };
            });
        }

        public override PulumiFn RemoveVirtualMachine(VirtualMachineParams vmArgs)
        {
            var deleteArgs = vmArgs as ProxmoxVirtualMachineDeleteParams;
            if (deleteArgs == null) throw new ArgumentNullException();

            return PulumiFn.Create(() =>
            {
                var provider = new Provider("proxmox_provider", new ProviderArgs
                {
                    Endpoint = _config.Endpoint,
                    Insecure = _config.Insecure,
                    Username = _config.UserName,
                    Password = _config.Password,
                });

                var virtualMachine = new VirtualMachine(deleteArgs.VmName, new VirtualMachineArgs
                {
                    NodeName = _config.TargetNode,
                    Name = deleteArgs.VmName,
                },
                new CustomResourceOptions
                {
                    Provider = provider,
                });
            });
        }

        #region Edit
        //public override PulumiFn EditVirtualMachine(VirtualMachineParams vmArgs)
        //{
        //    var editArgs = vmArgs as ProxmoxVirtualMachineEditParams;
        //    if (editArgs == null) throw new ArgumentNullException();

        //    return PulumiFn.Create(() =>
        //    {
        //        //https://www.pulumi.com/registry/packages/proxmoxve/installation-configuration/
        //        //This config can also be saved using environment variables and be retreived here. Both are correct.
        //        var provider = new Provider("proxmox_provider", new ProviderArgs
        //        {
        //            Endpoint = _config.Endpoint,
        //            Insecure = _config.Insecure,
        //            Username = _config.UserName,
        //            Password = _config.Password,
        //        });

        //        //VirtualMachine is a pulumi resource
        //        var virtualMachine = new VirtualMachine(editArgs.VmName, new VirtualMachineArgs
        //        {
        //            NodeName = _config.TargetNode,
        //            Name = editArgs.VmName,
        //            Agent = new VirtualMachineAgentArgs
        //            {
        //                Enabled = true,
        //                Trim = true,
        //                Type = "virtio",
        //            },
        //            Cpu = new VirtualMachineCpuArgs
        //            {
        //                Cores = 4,
        //                Sockets = 1,
        //                Architecture = "x86_64",
        //            },
        //            Clone = new VirtualMachineCloneArgs
        //            {
        //                NodeName = _config.SourceNode,
        //                VmId = editArgs.CloneId,
        //            },
        //            Memory = new VirtualMachineMemoryArgs
        //            {
        //                Dedicated = 4096,
        //            },
        //            Disks = new VirtualMachineDiskArgs
        //            {
        //                Interface = "scsi0",
        //                Size = 30,
        //                FileFormat = "qcow2",
        //                Ssd = true,
        //            },
        //            OperatingSystem = new VirtualMachineOperatingSystemArgs
        //            {
        //                Type = "l26"
        //            },
        //            SerialDevices = new VirtualMachineSerialDeviceArgs[] { }, //belangrijk als instelling voor console in proxmox, console start anders standaard mbv serial port
        //            NetworkDevices = new VirtualMachineNetworkDeviceArgs
        //            {
        //                Bridge = "vmbr0",
        //                Model = "virtio",
        //            },
        //            Initialization = new VirtualMachineInitializationArgs
        //            {
        //                Type = "nocloud",
        //                Dns = new VirtualMachineInitializationDnsArgs
        //                {
        //                    Domain = "localdomain",
        //                    Servers = "192.168.152.2"
        //                },
        //                IpConfigs =
        //                    new VirtualMachineInitializationIpConfigArgs
        //                    {
        //                        Ipv4 = new VirtualMachineInitializationIpConfigIpv4Args
        //                        {
        //                            //Cloud-init settings, changing this (for example the password) triggers a reset for the virtual machine. However, this does not happen when doing this in the proxmox ve web interface.
        //                            //The ip address therefor might also be changed, this is handled by updating the ip address for the virtual machine record after this provisioning is completed.
        //                            Address = "dhcp", //TODO: get the existing ip, so we don't need to update virtual machine db record ?
        //                            Gateway = "192.168.152.2"
        //                        },
        //                    },
        //                UserAccount = new VirtualMachineInitializationUserAccountArgs //apt-get install cloud-init moet geïnstalleerd worden op de host, dnf install voor fedora. of via cloud images
        //                {
        //                    Username = editArgs.Login,
        //                    Password = editArgs.Password,
        //                    //TODO: get from user input as example, for production store in Pulumi ESC on create, and retreive on update
        //                    Keys = new InputList<string>() { editArgs.SshKey },
        //                },
        //            }
        //        },
        //        new CustomResourceOptions
        //        {
        //            Provider = provider,
        //        });

        //        var ip = virtualMachine.Ipv4Addresses.Apply(res => res.LastOrDefault().LastOrDefault());
        //        var proxmoxId = virtualMachine.Id;
        //        var login = virtualMachine.Initialization.Apply(res => res!.UserAccount!.Username);
        //        var password = virtualMachine.Initialization.Apply(res => res!.UserAccount!.Password);

        //        return new Dictionary<string, object?>
        //        {
        //            ["ip"] = ip,
        //            ["id"] = proxmoxId,
        //            ["login"] = login,
        //            ["password"] = password,
        //        };
        //    });
        //}
        #endregion
    }

    public class ProxmoxVirtualMachineCreateParams : VirtualMachineParams
    {
        public int CloneId { get; set; }
        public required string Login { get; set; }
        public required string Password { get; set; }
        public required string SshKey { get; set; }

        public static ProxmoxVirtualMachineCreateParams FromViewModel(VirtualMachineCreateRequest request)
        {
            return new ProxmoxVirtualMachineCreateParams
            {
                CloneId = request.CloneId,
                Login = request.Login,
                Password = request.Password,
                SshKey = request.SshKey,
                VmName = request.Name,
            };
        }
    }

    public class ProxmoxVirtualMachineDeleteParams : VirtualMachineParams
    {
        public static ProxmoxVirtualMachineDeleteParams FromViewModel(VirtualMachineRemoveRequest request)
        {
            return new ProxmoxVirtualMachineDeleteParams
            {
                VmName = request.Name,
            };
        }
    }
}
