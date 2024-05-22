using Pulumi.Automation;
using Pulumi.ProxmoxVE;
using Pulumi;
using Pulumi.ProxmoxVE.CT;
using Pulumi.ProxmoxVE.CT.Inputs;

namespace VirtualMachineWorker.PulumiStrategy
{
    public partial class ProxmoxStrategy
    {
        public override PulumiFn CreateContainer(ContainerParams cArgs)
        {
            var createArgs = cArgs as ProxmoxContainerCreateParams;
            if (createArgs == null) throw new ArgumentNullException();

            return PulumiFn.Create(() =>
            {
                var provider = new Provider("proxmox_provider", new ProviderArgs
                {
                    Endpoint = _config.Endpoint,
                    Insecure = _config.Insecure,
                    Username = _config.UserName,
                    Password = _config.Password,
                });

                var container = new Container(createArgs.ContainerName, new ContainerArgs
                {
                    NodeName = _config.TargetNode,
                    //Description = createArgs.ContainerName,
                    //Disk = new ContainerDiskArgs
                    //{
                    //    DatastoreId = "local-lvm",
                    //    Size = 8096,
                    //},
                    //Memory = new ContainerMemoryArgs
                    //{
                    //    Dedicated = 1048,
                    //    //Swap = 0,
                    //},
                    //Features = new ContainerFeaturesArgs
                    //{
                    //    Nesting = true,
                    //},
                    Clone = new ContainerCloneArgs
                    {
                        VmId = createArgs.CloneId,
                        DatastoreId = "local-lvm",
                        NodeName = _config.SourceNode,
                    },
                    //Cpu = new ContainerCpuArgs
                    //{
                    //    Architecture = "amd64",
                    //    Cores = 2,
                    //    Units = 1024,
                    //},
                    StartOnBoot = true,
                    Started = true,
                    //Console = new ContainerConsoleArgs
                    //{
                    //    Enabled = true,
                    //    TtyCount = 2,
                    //    //Type = "string",
                    //},
                    NetworkInterfaces = new[]
                    {
                        new ContainerNetworkInterfaceArgs
                        {
                            Name = "eth0",
                            Bridge = "vmbr0",
                            Enabled = true,
                            Firewall = true,
                            //MacAddress = "string",
                            //Mtu = 0,
                            //RateLimit = 0,
                            //VlanId = 0,
                        },
                    },
                    Initialization = new ContainerInitializationArgs
                    {
                        //Dns = new ContainerInitializationDnsArgs
                        //{
                        //    Domain = "localdomain",
                        //    Servers = new[]
                        //    {
                        //        "192.168.152.2",
                        //    },
                        //},
                        Hostname = createArgs.ContainerName,
                        IpConfigs = new[]
                        {
                            new ContainerInitializationIpConfigArgs
                            {
                                //Moet ingesteld zijn om het ip-adres te kunnen achterhalen via SSH workaround
                                Ipv4 = new ContainerInitializationIpConfigIpv4Args
                                {
                                    Address = "dhcp",
                                    Gateway = "192.168.152.2",
                                },
                            },
                        },
                        //UserAccount = new ContainerInitializationUserAccountArgs
                        //{
                        //    Keys = new InputList<string>() { createArgs.SshKey },
                        //    Password = createArgs.Password,
                        //},
                    },
                },
                new CustomResourceOptions
                {
                    Provider = provider,
                });

                var proxmoxId = container.Id;

                return new Dictionary<string, object?>
                {
                    ["id"] = proxmoxId,
                };
            });
        }

        public override PulumiFn RemoveContainer(ContainerParams cArgs)
        {
            var deleteArgs = cArgs as ProxmoxContainerDeleteParams;
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

                var container = new Container(deleteArgs.ContainerName, new ContainerArgs
                {
                    NodeName = _config.TargetNode,
                    Description = deleteArgs.ContainerName,
                },
                new CustomResourceOptions
                {
                    Provider = provider,
                });
            });
        }

        //public override PulumiFn EditContainer(ContainerParams cArgs)
        //{
        //    throw new NotImplementedException();
        //}
    }

    public class ProxmoxContainerCreateParams : ContainerParams
    {
        public int CloneId { get; set; }
    }

    public class ProxmoxContainerEditParams : ContainerParams
    {
        public int CloneId { get; set; }
    }

    public class ProxmoxContainerDeleteParams : ContainerParams
    {
    }
}
