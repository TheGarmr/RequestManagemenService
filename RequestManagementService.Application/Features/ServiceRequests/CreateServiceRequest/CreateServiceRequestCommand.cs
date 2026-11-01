using MediatR;
using RequestManagementService.Application.Features.ServiceRequests.Common;
using RequestManagementService.Application.Mappers;
using RequestManagementService.DataAccess;
using RequestManagementService.DataAccess.Repositories;
using RequestManagementService.Domain.ServiceRequests;

namespace RequestManagementService.Application.Features.ServiceRequests.CreateServiceRequest;

public sealed record CreateServiceRequestCommand(
    string Title,
    string? Description,
    ServiceRequestPriority Priority) : IRequest<ServiceRequestItem>;

internal sealed class CreateServiceRequestCommandHandler(
    IServiceRequestRepository serviceRequestRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateServiceRequestCommand, ServiceRequestItem>
{
    public async Task<ServiceRequestItem> Handle(CreateServiceRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = ServiceRequestMapper.ToEntity(request);

        await serviceRequestRepository.AddAsync(entity, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return ServiceRequestMapper.ToItem(entity);
    }
}
