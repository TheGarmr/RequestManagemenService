using MediatR;
using RequestManagementService.Application.Features.ServiceRequests.Common;
using RequestManagementService.Application.Mappers;
using RequestManagementService.DataAccess;
using RequestManagementService.DataAccess.Enums;
using RequestManagementService.DataAccess.Repositories;

namespace RequestManagementService.Application.Features.ServiceRequests.CreateServiceRequest;

public sealed record CreateServiceRequestCommand(
    string Title,
    string? Description,
    ServiceRequestPriority Priority) : IRequest<ServiceRequestItem>;

internal sealed class CreateServiceRequestCommandHandler : IRequestHandler<CreateServiceRequestCommand, ServiceRequestItem>
{
    private readonly IServiceRequestRepository _serviceRequestRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateServiceRequestCommandHandler(
        IServiceRequestRepository serviceRequestRepository,
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork)
    {
        _serviceRequestRepository = serviceRequestRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceRequestItem> Handle(CreateServiceRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = ServiceRequestMapper.ToEntity(request, ServiceRequestStatus.New);

        await _serviceRequestRepository.AddAsync(entity, cancellationToken);
        await _outboxRepository.AddAsync(ServiceRequestMapper.ToOutboxMessage(entity, null), cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return ServiceRequestMapper.ToItem(entity);
    }
}
