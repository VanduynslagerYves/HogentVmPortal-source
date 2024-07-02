using HogentVmPortal.Shared;

namespace HogentVmPortalWebAPI.ProviderStrategies
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
