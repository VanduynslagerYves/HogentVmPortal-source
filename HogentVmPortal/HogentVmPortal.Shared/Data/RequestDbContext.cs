using HogentVmPortal.Shared.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HogentVmPortal.Shared.Data
{
    public class RequestDbContext : DbContext
    {
        public RequestDbContext(DbContextOptions<RequestDbContext> options)
            : base(options)
        {
        }

        public DbSet<Request> Requests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Request>(MapRequest);
        }

        public static void MapRequest(EntityTypeBuilder<Request> requestBuilder)
        {
            requestBuilder.ToTable("Request");

            requestBuilder.HasKey(x => x.Id);

            requestBuilder.Property(x => x.Name).IsRequired();
            requestBuilder.Property(x => x.Type).IsRequired();
            requestBuilder.Property(x => x.RequestType).IsRequired();
            requestBuilder.Property(x => x.Status).IsRequired();
            requestBuilder.Property(x => x.RequesterId).IsRequired();
            requestBuilder.Property(x => x.TimeStamp).IsRequired();

            //requestBuilder.HasOne(x => x.Requester).WithMany().IsRequired();
        }
    }
}
