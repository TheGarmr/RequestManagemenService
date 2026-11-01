using MediatR;
using RequestManagementService.Application.Common;
using RequestManagementService.Application.Features.ServiceRequests.Common;
using RequestManagementService.Application.Mappers;
using RequestManagementService.DataAccess.Enums;
using RequestManagementService.DataAccess.Repositories;

namespace RequestManagementService.Application.Features.ServiceRequests.ListServiceRequests;

public sealed record ListServiceRequestsQuery(
    string? Status,
    int Page,
    int PageSize) : IRequest<PagedResult<ServiceRequestItem>>;

internal sealed class ListServiceRequestsQueryHandler : IRequestHandler<ListServiceRequestsQuery, PagedResult<ServiceRequestItem>>
{
    private readonly IServiceRequestRepository _serviceRequestRepository;

    public ListServiceRequestsQueryHandler(IServiceRequestRepository serviceRequestRepository)
    {
        _serviceRequestRepository = serviceRequestRepository;
    }

    public async Task<PagedResult<ServiceRequestItem>> Handle(
        ListServiceRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var status = request.Status is null
            ? (ServiceRequestStatus?)null
            : Enum.Parse<ServiceRequestStatus>(request.Status, ignoreCase: true);

        var skip = (request.Page - 1) * request.PageSize;

        var totalCount = await _serviceRequestRepository.CountAsync(status, cancellationToken);
        var entities = await _serviceRequestRepository.GetListAsync(
            status,
            skip,
            request.PageSize,
            cancellationToken);

        var items = entities.Select(ServiceRequestMapper.ToItem).ToList();

        return new PagedResult<ServiceRequestItem>(items, request.Page, request.PageSize, totalCount);
    }
}
