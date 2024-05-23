using HogentVmPortal.Shared;
using Microsoft.Extensions.Options;
using VirtualMachineWorker.Data.Repositories;
using VirtualMachineWorker.Models;

namespace VirtualMachineWorker
{
    public partial class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private readonly IOptions<ProxmoxConfig> _proxmoxConfig;
        private readonly IOptions<ProxmoxSshConfig> _proxmoxSshConfig;

        private readonly IVirtualMachineRequestRepository _virtualMachineRequestRepository;
        private readonly IContainerRequestRepository _containerRequestRepository;

        private readonly IVirtualMachineRepository _virtualMachineRepository;
        private readonly IVirtualMachineTemplateRepository _virtualMachineTemplateRepository;

        private readonly IContainerRepository _containerRepository;
        private readonly IContainerTemplateRepository _containerTemplateRepository;

        private readonly IAppUserRepository _appUserRepository;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IOptions<ProxmoxConfig> proxmoxConfig, IOptions<ProxmoxSshConfig> proxmoxSshConfig)
        {
            _logger = logger;
            var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            //https://stackoverflow.com/questions/48368634/how-to-consume-a-scoped-service-from-a-singleton
            _virtualMachineRequestRepository = services.GetService<IVirtualMachineRequestRepository>()!;
            _containerRequestRepository = services.GetService<IContainerRequestRepository>()!;

            _virtualMachineRepository = services.GetService<IVirtualMachineRepository>()!;
            _virtualMachineTemplateRepository = services.GetService<IVirtualMachineTemplateRepository>()!;

            _containerRepository = services.GetService<IContainerRepository>()!;
            _containerTemplateRepository = services.GetRequiredService<IContainerTemplateRepository>()!;

            _appUserRepository = services.GetService<IAppUserRepository>()!;

            //https://stackoverflow.com/questions/31453495/how-to-read-appsettings-values-from-a-json-file-in-asp-net-core
            _proxmoxConfig = proxmoxConfig;
            _proxmoxSshConfig = proxmoxSshConfig;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                //TODO: change implementation of handlers: every request should be handled by timestamp, not by category
                //get all request of all categories, order them by timestamp and check type for each request, then call the correct handler

                await HandleVirtualMachineRemoveRequests();
                await HandleVirtualMachineCreateRequests();

                await HandleContainerRemoveRequests();
                await HandleContainerCreateRequests();

                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
