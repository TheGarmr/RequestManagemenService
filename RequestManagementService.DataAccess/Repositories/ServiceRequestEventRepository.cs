using Microsoft.EntityFrameworkCore;
using RequestManagementService.DataAccess.Entities;

namespace RequestManagementService.DataAccess.Repositories;

public sealed class ServiceRequestEventRepository : IServiceRequestEventRepository
{
    private readonly RequestManagementDbContext _dbContext;

    public ServiceRequestEventRepository(RequestManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsAsync(long messageId, CancellationToken cancellationToken)
    {
        return _dbContext.ServiceRequestEvents.AnyAsync(x => x.MessageId == messageId, cancellationToken);
    }

    public Task AddAsync(ServiceRequestEventEntity serviceRequestEvent, CancellationToken cancellationToken)
    {
        return _dbContext.ServiceRequestEvents.AddAsync(serviceRequestEvent, cancellationToken).AsTask();
    }
}
