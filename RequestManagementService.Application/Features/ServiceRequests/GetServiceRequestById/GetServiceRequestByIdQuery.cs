using MediatR;
using RequestManagementService.Application.Exceptions;
using RequestManagementService.Application.Features.ServiceRequests.Common;
using RequestManagementService.Application.Mappers;
using RequestManagementService.DataAccess.Repositories;

namespace RequestManagementService.Application.Features.ServiceRequests.GetServiceRequestById;

public sealed record GetServiceRequestByIdQuery(Guid Id) : IRequest<ServiceRequestItem>;

internal sealed class GetServiceRequestByIdQueryHandler : IRequestHandler<GetServiceRequestByIdQuery, ServiceRequestItem>
{
    private readonly IServiceRequestRepository _serviceRequestRepository;

    public GetServiceRequestByIdQueryHandler(IServiceRequestRepository serviceRequestRepository)
    {
        _serviceRequestRepository = serviceRequestRepository;
    }

    public async Task<ServiceRequestItem> Handle(GetServiceRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _serviceRequestRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Service request '{request.Id}' was not found.");

        return ServiceRequestMapper.ToItem(entity);
    }
}
