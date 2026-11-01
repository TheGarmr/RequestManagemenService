using RequestManagementService.DataAccess.Entities;
using RequestManagementService.DataAccess.Enums;

namespace RequestManagementService.DataAccess.Repositories;

public interface IServiceRequestRepository
{
    Task<ServiceRequestEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<List<ServiceRequestEntity>> GetListAsync(
        ServiceRequestStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountAsync(ServiceRequestStatus? status, CancellationToken cancellationToken);

    Task AddAsync(ServiceRequestEntity entity, CancellationToken cancellationToken);
}
