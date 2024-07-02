using HogentVmPortal.Shared;
using HogentVmPortal.Shared.Data;
using HogentVmPortalWebAPI.Data.Repositories;
using HogentVmPortalWebAPI.Handlers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DbContextConnection")));

builder.Services.AddControllers();

//builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
//builder.Services.AddHostedService<QueuedHostedService>();

// SSH Config
builder.Services.Configure<ProxmoxSshConfig>(builder.Configuration.GetSection("ProxmoxSshConfig"));
builder.Services.Configure<ProxmoxConfig>(builder.Configuration.GetSection("ProxmoxConfig"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS policy for debug
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        builder =>
        {
            builder.WithOrigins("https://localhost:7009") // replace with your MVC project's URL
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

builder.Services.AddScoped<IVirtualMachineRepository, VirtualMachineRepository>();
builder.Services.AddScoped<IContainerRepository, ContainerRepository>();
builder.Services.AddScoped<IVirtualMachineTemplateRepository, VirtualMachineTemplateRepository>();
builder.Services.AddScoped<IContainerTemplateRepository, ContainerTemplateRepository>();

builder.Services.AddScoped<IAppUserRepository, AppUserRepository>();

builder.Services.AddScoped<VirtualMachineHandler>();
builder.Services.AddScoped<ContainerHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
