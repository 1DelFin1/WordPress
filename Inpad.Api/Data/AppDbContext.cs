using Inpad.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Inpad.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ArchObject> ArchObjects => Set<ArchObject>();
    public DbSet<ObjectMedia> ObjectMedias => Set<ObjectMedia>();
    public DbSet<ObjectCharacteristic> ObjectCharacteristics => Set<ObjectCharacteristic>();
    public DbSet<ObjectTeamMember> ObjectTeamMembers => Set<ObjectTeamMember>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ObjectCategory> ObjectCategories => Set<ObjectCategory>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<Reference> References => Set<Reference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ArchObject>(e =>
        {
            e.Property(x => x.Status).HasConversion<string>();
            e.Property(x => x.WordPressStatus).HasConversion<string>();
            e.Property(x => x.ProjectStatus).HasConversion<string>();
            e.Property(x => x.DesignStage).HasConversion<string>();
            e.HasIndex(x => x.Slug)
                .IsUnique()
                .HasFilter("\"Slug\" IS NOT NULL");
            e.HasMany(x => x.Media)
                .WithOne(x => x.ArchObject)
                .HasForeignKey(x => x.ArchObjectId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Characteristics)
                .WithOne(x => x.ArchObject)
                .HasForeignKey(x => x.ArchObjectId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.TeamMembers)
                .WithOne(x => x.ArchObject)
                .HasForeignKey(x => x.ArchObjectId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Categories)
                .WithMany(x => x.Objects)
                .UsingEntity(j => j.ToTable("ArchObjectCategories"));
        });

        modelBuilder.Entity<ObjectMedia>(e =>
        {
            e.Property(x => x.MediaType).HasConversion<string>();
        });

        modelBuilder.Entity<User>(e =>
        {
            e.Property(x => x.Role).HasConversion<string>();
            e.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<AppSetting>(e =>
        {
            e.HasIndex(x => x.Key).IsUnique();
        });
    }
}
