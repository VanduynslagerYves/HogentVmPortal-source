using Pulumi.Automation;

namespace HogentVmPortalWebAPI.ProviderStrategies
{
    public abstract class ProviderStrategy
    {
        public abstract PulumiFn CreateVirtualMachine(VirtualMachineParams vmArgs);
        public abstract PulumiFn RemoveVirtualMachine(VirtualMachineParams vmArgs);

        public abstract PulumiFn CreateContainer(ContainerParams cArgs);
        public abstract PulumiFn RemoveContainer(ContainerParams cArgs);
    }

    public abstract class VirtualMachineParams
    {
        public required string VmName { get; set; }
    }

    public abstract class ContainerParams
    {
        public required string ContainerName { get; set; }
    }
}
