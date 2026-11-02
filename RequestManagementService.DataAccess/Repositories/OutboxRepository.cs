using Microsoft.EntityFrameworkCore;
using RequestManagementService.DataAccess.Entities;

namespace RequestManagementService.DataAccess.Repositories;

public sealed class OutboxRepository : IOutboxRepository
{
    private readonly RequestManagementDbContext _dbContext;

    public OutboxRepository(RequestManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(OutboxMessageEntity message, CancellationToken cancellationToken)
    {
        return _dbContext.OutboxMessages.AddAsync(message, cancellationToken).AsTask();
    }

    public Task<List<OutboxMessageEntity>> GetUnprocessedAsync(int batchSize, CancellationToken cancellationToken)
    {
        return _dbContext.OutboxMessages
            .Where(x => x.ProcessedAt == null)
            .OrderBy(x => x.Id)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }
}
