using Microsoft.EntityFrameworkCore;
using HogentVmPortal.Shared;
using HogentVmPortal.Shared.Data;
using HogentVmPortal.Shared.Repositories;
using HogentVmPortal.RequestQueue.CtHandler;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        IConfiguration configuration = hostContext.Configuration;
        var connectionString = configuration.GetConnectionString("DbContextConnection") ?? throw new InvalidOperationException("Connection string 'DbContextConnection' not found.");
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<IAppUserRepository, AppUserRepository>();
        services.AddScoped<IContainerRepository, ContainerRepository>();
        services.AddScoped<IContainerTemplateRepository, ContainerTemplateRepository>();

        services.Configure<ProxmoxSshConfig>(configuration.GetSection("ProxmoxSshConfig"));
        services.Configure<ProxmoxConfig>(configuration.GetSection("ProxmoxConfig"));
        services.Configure<RabbitMQConfig>(configuration.GetSection("RabbitMQConfig"));

        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
