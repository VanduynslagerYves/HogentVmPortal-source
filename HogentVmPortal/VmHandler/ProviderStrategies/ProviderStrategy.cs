using Pulumi.Automation;

namespace VmHandler.ProviderStrategies
{
    public abstract class ProviderStrategy
    {
        public abstract PulumiFn CreateVirtualMachine(VirtualMachineParams vmArgs);
        public abstract PulumiFn RemoveVirtualMachine(VirtualMachineParams vmArgs);
    }

    public abstract class VirtualMachineParams
    {
        public required string VmName { get; set; }
    }
}
