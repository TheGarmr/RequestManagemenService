using Microsoft.EntityFrameworkCore;
using RequestManagementService.DataAccess.Entities;

namespace RequestManagementService.DataAccess;

public sealed class RequestManagementDbContext : DbContext
{
    private readonly TimeProvider _timeProvider;

    public RequestManagementDbContext(DbContextOptions<RequestManagementDbContext> options, TimeProvider timeProvider)
        : base(options)
    {
        _timeProvider = timeProvider;
    }

    public DbSet<ServiceRequestEntity> ServiceRequests => Set<ServiceRequestEntity>();

    public DbSet<OutboxMessageEntity> OutboxMessages => Set<OutboxMessageEntity>();

    public DbSet<ServiceRequestEventEntity> ServiceRequestEvents => Set<ServiceRequestEventEntity>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTimestamps();

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ServiceRequestEntity>(entity =>
        {
            entity.ToTable("service_requests");

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();

            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000);

            entity.Property(x => x.Priority).HasConversion<string>().HasMaxLength(16);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);

            entity.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");

            entity.Property(x => x.Version).IsRowVersion();

            entity.HasIndex(x => new { x.Status, x.CreatedAt }).IsDescending(false, true);
        });

        modelBuilder.Entity<OutboxMessageEntity>(entity =>
        {
            entity.ToTable("outbox_messages");

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).UseIdentityAlwaysColumn();

            entity.Property(x => x.OldStatus).HasConversion<string>().HasMaxLength(16);
            entity.Property(x => x.NewStatus).HasConversion<string>().HasMaxLength(16);
            entity.Property(x => x.Priority).HasConversion<string>().HasMaxLength(16);

            entity.Property(x => x.OccurredAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ProcessedAt).HasColumnType("timestamp with time zone");

            entity.HasIndex(x => x.Id)
                .HasDatabaseName("ix_outbox_messages_unprocessed")
                .HasFilter("processed_at IS NULL");
        });

        modelBuilder.Entity<ServiceRequestEventEntity>(entity =>
        {
            entity.ToTable("service_request_events");

            entity.HasKey(x => x.MessageId);
            entity.Property(x => x.MessageId).ValueGeneratedNever();

            entity.Property(x => x.OldStatus).HasConversion<string>().HasMaxLength(16);
            entity.Property(x => x.NewStatus).HasConversion<string>().HasMaxLength(16);
            entity.Property(x => x.Priority).HasConversion<string>().HasMaxLength(16);

            entity.Property(x => x.OccurredAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ReceivedAt).HasColumnType("timestamp with time zone");
        });
    }

    private void SetTimestamps()
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;

        foreach (var entry in ChangeTracker.Entries<ServiceRequestEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        foreach (var entry in ChangeTracker.Entries<OutboxMessageEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.OccurredAt = now;
            }
        }

        foreach (var entry in ChangeTracker.Entries<ServiceRequestEventEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.ReceivedAt = now;
            }
        }
    }
}
