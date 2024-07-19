using Microsoft.EntityFrameworkCore;
using HogentVmPortal.Shared.Data;
using HogentVmPortal.Shared.Repositories;
using VmHandler;
using HogentVmPortal.Shared;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        IConfiguration configuration = hostContext.Configuration;
        var connectionString = configuration.GetConnectionString("DbContextConnection") ?? throw new InvalidOperationException("Connection string 'DbContextConnection' not found.");
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

        //services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<IAppUserRepository, AppUserRepository>();
        services.AddScoped<IVirtualMachineRepository, VirtualMachineRepository>();
        services.AddScoped<IVirtualMachineTemplateRepository, VirtualMachineTemplateRepository>();

        //services.Configure<ProxmoxSshConfig>(configuration.GetSection("ProxmoxSshConfig"));
        services.Configure<ProxmoxConfig>(configuration.GetSection("ProxmoxConfig"));

        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
//var builder = Host.CreateApplicationBuilder(args);
//builder.Services.AddHostedService<Worker>();

//var host = builder.Build();
//host.Run();
