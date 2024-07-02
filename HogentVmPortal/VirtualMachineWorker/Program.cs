using Microsoft.EntityFrameworkCore;
using HogentVmPortal.Shared.Data;
using VirtualMachineWorker;
using VirtualMachineWorker.Data.Repositories;
using HogentVmPortal.Shared;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        IConfiguration configuration = hostContext.Configuration;
        var connectionString = configuration.GetConnectionString("DbContextConnection") ?? throw new InvalidOperationException("Connection string 'DbContextConnection' not found.");
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<IAppUserRepository, AppUserRepository>();

        services.AddScoped<IVirtualMachineRepository, VirtualMachineRepository>();
        services.AddScoped<IVirtualMachineTemplateRepository, VirtualMachineTemplateRepository>();
        services.AddScoped<IContainerRepository, ContainerRepository>();
        services.AddScoped<IContainerTemplateRepository, ContainerTemplateRepository>();

        services.AddScoped<IVirtualMachineRequestRepository,  VirtualMachineRequestRepository>();
        services.AddScoped<IContainerRequestRepository, ContainerRequestRepository>();

        services.Configure<ProxmoxSshConfig>(configuration.GetSection("ProxmoxSshConfig"));

        //read the proxmoxconfig in appsettings.json and enable them to be injected using ProxmoxConfig3
        //https://www.c-sharpcorner.com/article/asp-net-core-how-to-read-values-from-appsettings-json/

        services.Configure<ProxmoxConfig>(configuration.GetSection("ProxmoxConfig"));

        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
