using HogentVmPortal.Shared.DTO;
using HogentVmPortal.Shared.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HogentVmPortal.Shared.Data;

//https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/projects?tabs=dotnet-core-cli
//https://stackoverflow.com/questions/60442788/what-is-the-proper-way-to-reuse-dbcontext-across-different-projects-in-a-single
//https://stackoverflow.com/questions/38705694/add-migration-with-different-assembly

//DbContext is placed in HogentVmPortal.Shared, both the webapp and the workers need to access this mapping
public class ApplicationDbContext : IdentityDbContext<HogentUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
    {
    }

    public DbSet<VirtualMachine> VirtualMachines { get; set; }
    public DbSet<Container> Containers { get; set; }
    public DbSet<VirtualMachineTemplate> VirtualMachineTemplates { get; set; }
    public DbSet<ContainerTemplate> ContainerTemplates { get; set; }

    public DbSet<HogentUser> HogentUsers { get; set; }
    public DbSet<Course> Courses { get; set; }

    public DbSet<VirtualMachineCreateRequest> VirtualMachineCreateRequests { get; set; }
    public DbSet<VirtualMachineRemoveRequest> VirtualMachineRemoveRequests { get; set; }

    public DbSet<ContainerCreateRequest> ContainerCreateRequests { get; set; }
    public DbSet<ContainerRemoveRequest> ContainerRemoveRequests { get; set; }

    //Most of these mappings are handled implicitely and are not needed
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<VirtualMachine>(MapVirtualMachine);
        builder.Entity<Container>(MapContainer);
        builder.Entity<VirtualMachineTemplate>(MapVirtualMachineTemplate);
        builder.Entity<ContainerTemplate>(MapContainerTemplate);

        builder.Entity<HogentUser>(MapHogentUser);
        builder.Entity<Course>(MapCourse);

        builder.Entity<VirtualMachineCreateRequest>(MapVmCreateRequest);
        builder.Entity<VirtualMachineRemoveRequest>(MapVmRemoveRequest);

        builder.Entity<ContainerCreateRequest>(MapContainerCreateRequest);
        builder.Entity<ContainerRemoveRequest>(MapContainerRemoveRequest);
    }

    public static void MapVirtualMachine(EntityTypeBuilder<VirtualMachine> vmBuilder)
    {
        vmBuilder.ToTable("VirtualMachine");

        vmBuilder.HasKey(x => x.Id);
        vmBuilder.HasIndex(x => x.Name).HasDatabaseName("IX_Name");

        vmBuilder.Property(x => x.Login).IsRequired();
        vmBuilder.Property(x => x.Name).IsRequired();
        vmBuilder.Property(x => x.ProxmoxId).IsRequired();
        vmBuilder.Property(x => x.IpAddress).IsRequired();

        vmBuilder.HasOne(x => x.Owner).WithMany(x => x.VirtualMachines).HasForeignKey("OwnerId").IsRequired();

        vmBuilder.HasOne(x => x.Template).WithMany(x => x.VirtualMachines).HasForeignKey("TemplateId").IsRequired();
    }

    public static void MapContainer(EntityTypeBuilder<Container> containerBuilder)
    {
        containerBuilder.ToTable("Container");

        containerBuilder.HasKey(x => x.Id);
        containerBuilder.HasIndex(x => x.Name).HasDatabaseName("IX_Name");

        containerBuilder.Property(x => x.Name).IsRequired();
        containerBuilder.Property(x => x.ProxmoxId).IsRequired();
        containerBuilder.Property(x => x.IpAddress).IsRequired();

        containerBuilder.HasOne(x => x.Template).WithMany(x => x.Containers).HasForeignKey("TemplateId").IsRequired();
    }

    public static void MapVirtualMachineTemplate(EntityTypeBuilder<VirtualMachineTemplate> templateBuilder)
    {
        templateBuilder.ToTable("VirtualMachineTemplate");

        templateBuilder.HasKey(x => x.Id);
        templateBuilder.HasIndex(x => x.Name).HasDatabaseName("IX_Name");

        templateBuilder.Property(x => x.Name).IsRequired();
        templateBuilder.Property(x => x.OperatingSystem).IsRequired();
        templateBuilder.Property(x => x.ProxmoxId).IsRequired();
        templateBuilder.Property(x => x.Description);

        templateBuilder.HasMany(x => x.Courses).WithMany(x => x.VirtualMachineTemplates);
    }

    public static void MapContainerTemplate(EntityTypeBuilder<ContainerTemplate> templateBuilder)
    {
        templateBuilder.ToTable("ContainerTemplate");

        templateBuilder.HasKey(x => x.Id);
        templateBuilder.HasIndex(x => x.Name).HasDatabaseName("IX_Name");

        templateBuilder.Property(x => x.Name).IsRequired();
        templateBuilder.Property(x => x.ProxmoxId).IsRequired();
        templateBuilder.Property(x => x.Description);

        templateBuilder.HasMany(x => x.Courses).WithMany(x => x.ContainerTemplates);
    }

    public static void MapHogentUser(EntityTypeBuilder<HogentUser> userBuilder)
    {
        userBuilder.ToTable("HogentUser");
        userBuilder.HasKey(x => x.Id);

        userBuilder.HasMany(x => x.VirtualMachines).WithOne(x => x.Owner);
        userBuilder.HasMany(x => x.Containers).WithOne(x => x.Owner);
        userBuilder.HasMany(x => x.Courses).WithMany(x => x.Students);
    }

    public static void MapCourse(EntityTypeBuilder<Course> courseBuilder)
    {
        courseBuilder.ToTable("Course");

        courseBuilder.HasKey(x => x.Id);
        courseBuilder.Property(x => x.Name).IsRequired();

        courseBuilder.HasMany(x => x.Students).WithMany(x => x.Courses);
        courseBuilder.HasMany(x => x.VirtualMachineTemplates).WithMany(x => x.Courses);
        courseBuilder.HasMany(x => x.ContainerTemplates).WithMany(x => x.Courses);
    }

    public static void MapVmCreateRequest(EntityTypeBuilder<VirtualMachineCreateRequest> vmCreateBuilder)
    {
        vmCreateBuilder.ToTable("VirtualMachineCreateRequest");

        vmCreateBuilder.HasKey(x => x.Id);

        vmCreateBuilder.Property(x => x.Name).IsRequired();
        vmCreateBuilder.Property(x => x.TimeStamp).IsRequired();
        vmCreateBuilder.Property(x => x.OwnerId).IsRequired();
        vmCreateBuilder.Property(x => x.Login).IsRequired();
        vmCreateBuilder.Property(x => x.Password).IsRequired();
        vmCreateBuilder.Property(x => x.SshKey).IsRequired();
        vmCreateBuilder.Property(x => x.CloneId).IsRequired();
    }

    public static void MapVmRemoveRequest(EntityTypeBuilder<VirtualMachineRemoveRequest> vmRemoveBuilder)
    {
        vmRemoveBuilder.ToTable("VirtualMachineRemoveRequest");

        vmRemoveBuilder.HasKey(x => x.Id);

        vmRemoveBuilder.Property(x => x.VmId).IsRequired();
        vmRemoveBuilder.Property(x => x.Name).IsRequired();
        vmRemoveBuilder.Property(x => x.TimeStamp).IsRequired();
    }

    public static void MapContainerCreateRequest(EntityTypeBuilder<ContainerCreateRequest> containerCreateBuilder)
    {
        containerCreateBuilder.ToTable("ContainerCreateRequest");

        containerCreateBuilder.HasKey(x => x.Id);

        containerCreateBuilder.Property(x => x.Name).IsRequired();
        containerCreateBuilder.Property(x => x.TimeStamp).IsRequired();
        containerCreateBuilder.Property(x => x.OwnerId).IsRequired();
        containerCreateBuilder.Property(x => x.CloneId).IsRequired();
    }

    public static void MapContainerRemoveRequest(EntityTypeBuilder<ContainerRemoveRequest> containerRemoveBuilder)
    {
        containerRemoveBuilder.ToTable("ContainerRemoveRequest");

        containerRemoveBuilder.HasKey(x => x.Id);

        containerRemoveBuilder.Property(x => x.VmId).IsRequired();
        containerRemoveBuilder.Property(x => x.Name).IsRequired();
        containerRemoveBuilder.Property(x => x.TimeStamp).IsRequired();
    }
}
