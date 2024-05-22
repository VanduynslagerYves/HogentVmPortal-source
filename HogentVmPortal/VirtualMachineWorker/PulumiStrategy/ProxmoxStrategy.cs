using VirtualMachineWorker.Models;

namespace VirtualMachineWorker.PulumiStrategy
{
    public partial class ProxmoxStrategy : ProviderStrategy
    {
        private readonly ProxmoxConfig _config;

        public ProxmoxStrategy(ProxmoxConfig config)
        {
            _config = config;
        }
    }
}
