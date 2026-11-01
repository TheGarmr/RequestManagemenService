using MediatR;
using RequestManagementService.Application.Exceptions;
using RequestManagementService.Application.Features.ServiceRequests.Common;
using RequestManagementService.Application.Mappers;
using RequestManagementService.DataAccess.Repositories;

namespace RequestManagementService.Application.Features.ServiceRequests.GetServiceRequestById;

public sealed record GetServiceRequestByIdQuery(Guid Id) : IRequest<ServiceRequestItem>;

internal sealed class GetServiceRequestByIdQueryHandler(IServiceRequestRepository serviceRequestRepository)
    : IRequestHandler<GetServiceRequestByIdQuery, ServiceRequestItem>
{
    public async Task<ServiceRequestItem> Handle(GetServiceRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await serviceRequestRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Service request '{request.Id}' was not found.");

        return ServiceRequestMapper.ToItem(ServiceRequestMapper.ToDomain(entity));
    }
}
