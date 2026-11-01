using Microsoft.EntityFrameworkCore;
using RequestManagementService.DataAccess.Entities;
using RequestManagementService.Domain.ServiceRequests;

namespace RequestManagementService.DataAccess.Repositories;

internal sealed class ServiceRequestRepository(RequestManagementDbContext dbContext) : IServiceRequestRepository
{
    public Task<ServiceRequestEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.ServiceRequests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<List<ServiceRequestEntity>> GetListAsync(
        ServiceRequestStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        return Filter(status)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(ServiceRequestStatus? status, CancellationToken cancellationToken)
    {
        return Filter(status).CountAsync(cancellationToken);
    }

    public Task AddAsync(ServiceRequestEntity entity, CancellationToken cancellationToken)
    {
        return dbContext.ServiceRequests.AddAsync(entity, cancellationToken).AsTask();
    }

    private IQueryable<ServiceRequestEntity> Filter(ServiceRequestStatus? status)
    {
        var query = dbContext.ServiceRequests.AsNoTracking();

        if (status is not null)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        return query;
    }
}
