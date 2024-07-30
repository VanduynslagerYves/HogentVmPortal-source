using HogentVmPortal.Shared;
using HogentVmPortal.Shared.Data;
using HogentVmPortal.Shared.Repositories;
using HogentVmPortal.RequestQueue.WebAPI.Handlers;
using Microsoft.EntityFrameworkCore;
using HogentVmPortal.RequestQueue.WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DbContextConnection")));

builder.Services.AddDbContext<RequestDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("RequestDbContextConnection")));

builder.Services.AddControllers();

//builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
//builder.Services.AddHostedService<QueuedHostedService>();
builder.Services.AddHostedService<RequestUpdateService>();

// RabbitMQ Config
builder.Services.Configure<RabbitMQConfig>(builder.Configuration.GetSection("RabbitMQConfig"));

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
builder.Services.AddScoped<IRequestRepository, RequestRepository>();

builder.Services.AddScoped<VirtualMachineQueueHandler>();
builder.Services.AddScoped<ContainerQueueHandler>();

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
