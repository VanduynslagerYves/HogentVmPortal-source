using Microsoft.EntityFrameworkCore;
using HogentVmPortal.Shared.Model;
using HogentVmPortal.Data.Repositories;
using HogentVmPortal.Shared.Data;
using HogentVmPortal.Shared;
using HogentVmPortal.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbContextConnection"), optionsBuilder => optionsBuilder.MigrationsAssembly("HogentVmPortal"));
});

builder.Services.AddDbContext<RequestDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("RequestDbContextConnection"), optionsBuilder => optionsBuilder.MigrationsAssembly("HogentVmPortal"));
});

builder.Services.AddDefaultIdentity<HogentUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<IVirtualMachineTemplateRepository, VirtualMachineTemplateRepository>();
builder.Services.AddScoped<IContainerTemplateRepository, ContainerTemplateRepository>();

builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IAppUserRepository, AppUserRepository>();

builder.Services.Configure<ProxmoxSshConfig>(builder.Configuration.GetSection("ProxmoxSshConfig"));
builder.Services.Configure<WebApiConfig>(builder.Configuration.GetSection("WebApiConfig"));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

builder.Services.AddTransient<VirtualMachineApiService>();
builder.Services.AddTransient<ContainerApiService>();
builder.Services.AddTransient<ValidateApiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=VirtualMachine}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
