using Pulumi.Automation;
using Pulumi.ProxmoxVE;
using Pulumi;
using Pulumi.ProxmoxVE.CT;
using Pulumi.ProxmoxVE.CT.Inputs;

namespace HogentVmPortalWebAPI.ProviderStrategies
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
                    Clone = new ContainerCloneArgs
                    {
                        VmId = createArgs.CloneId,
                        DatastoreId = "local-lvm",
                        NodeName = _config.SourceNode,
                    },
                    StartOnBoot = true,
                    Started = true,
                    NetworkInterfaces = new[]
                    {
                        new ContainerNetworkInterfaceArgs
                        {
                            Name = "eth0",
                            Bridge = "vmbr0",
                            Enabled = true,
                            Firewall = true,
                        },
                    },
                    Initialization = new ContainerInitializationArgs
                    {
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
                    },
                },
                new CustomResourceOptions
                {
                    Provider = provider,
                });

                var proxmoxId = container.Id;
                //TODO: figure out how to get the ip address here, so we don't have to connect to the server to retreive it afterwards
                //or add it afterwards with Container.Get(name, id);
                //following code will not work since the ip address is not known until after the container has started
                //container.Initialization.Apply(res => res.IpConfigs);

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
