using RequestManagementService.DataAccess.Entities;

namespace RequestManagementService.DataAccess.Repositories;

public interface IServiceRequestEventRepository
{
    Task<bool> ExistsAsync(long messageId, CancellationToken cancellationToken);

    Task AddAsync(ServiceRequestEventEntity serviceRequestEvent, CancellationToken cancellationToken);
}
