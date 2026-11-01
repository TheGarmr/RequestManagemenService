using RequestManagementService.DataAccess.Entities;

namespace RequestManagementService.DataAccess.Repositories;

public interface IOutboxRepository
{
    Task AddAsync(OutboxMessageEntity message, CancellationToken cancellationToken);

    Task<List<OutboxMessageEntity>> GetUnprocessedAsync(int batchSize, CancellationToken cancellationToken);
}
