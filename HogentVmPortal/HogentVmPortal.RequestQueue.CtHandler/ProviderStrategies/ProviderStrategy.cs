using Pulumi.Automation;

namespace HogentVmPortal.RequestQueue.VmHandler.ProviderStrategies
{
    public abstract class ProviderStrategy
    {
        public abstract PulumiFn CreateContainer(ContainerParams cArgs);
        public abstract PulumiFn RemoveContainer(ContainerParams cArgs);
    }

    public abstract class ContainerParams
    {
        public required string ContainerName { get; set; }
    }
}
