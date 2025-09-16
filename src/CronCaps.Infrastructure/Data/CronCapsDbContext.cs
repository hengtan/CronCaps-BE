using CronCaps.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CronCaps.Infrastructure.Data;

public class CronCapsDbContext : DbContext
{
    public CronCapsDbContext(DbContextOptions<CronCapsDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<JobExecution> JobExecutions => Set<JobExecution>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).IsRequired().HasConversion<string>();

            entity.HasOne(e => e.Team)
                  .WithMany(t => t.Members)
                  .HasForeignKey(e => e.TeamId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Team configuration
        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // Category configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Color).HasMaxLength(7);
        });

        // Job configuration
        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Status).IsRequired().HasConversion<string>();
            entity.Property(e => e.Tags).HasMaxLength(1000);

            // Configure CronExpression as owned type
            entity.OwnsOne(e => e.CronExpression, cron =>
            {
                cron.Property(c => c.Value).IsRequired().HasMaxLength(100);
            });

            // Configure JobConfiguration as owned type
            entity.OwnsOne(e => e.Configuration, config =>
            {
                config.Property(c => c.TimeoutSeconds).IsRequired();
                config.Property(c => c.MaxRetries).IsRequired();
                config.Property(c => c.NotifyOnFailure).IsRequired();
                config.Property(c => c.NotifyOnSuccess).IsRequired();
                config.Property(c => c.NotificationEmail).HasMaxLength(255);
                config.Property(c => c.Environment).HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, string>()
                );
            });

            entity.HasOne(e => e.CreatedBy)
                  .WithMany(u => u.Jobs)
                  .HasForeignKey(e => e.CreatedById)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Team)
                  .WithMany(t => t.Jobs)
                  .HasForeignKey(e => e.TeamId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Jobs)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // JobExecution configuration
        modelBuilder.Entity<JobExecution>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).IsRequired().HasConversion<string>();
            entity.Property(e => e.Output).HasColumnType("text");
            entity.Property(e => e.ErrorMessage).HasColumnType("text");
            entity.Property(e => e.TriggeredBy).HasMaxLength(100);

            entity.HasOne(e => e.Job)
                  .WithMany(j => j.Executions)
                  .HasForeignKey(e => e.JobId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure indexes for performance
        modelBuilder.Entity<Job>()
                   .HasIndex(e => e.Status)
                   .HasDatabaseName("IX_Jobs_Status");

        modelBuilder.Entity<Job>()
                   .HasIndex(e => e.NextRunAt)
                   .HasDatabaseName("IX_Jobs_NextRunAt");

        modelBuilder.Entity<JobExecution>()
                   .HasIndex(e => e.StartedAt)
                   .HasDatabaseName("IX_JobExecutions_StartedAt");

        modelBuilder.Entity<JobExecution>()
                   .HasIndex(e => new { e.JobId, e.StartedAt })
                   .HasDatabaseName("IX_JobExecutions_JobId_StartedAt");
    }
}