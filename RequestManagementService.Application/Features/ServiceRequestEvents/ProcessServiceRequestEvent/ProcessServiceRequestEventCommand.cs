using MediatR;
using Microsoft.Extensions.Logging;
using RequestManagementService.Application.Mappers;
using RequestManagementService.DataAccess;
using RequestManagementService.DataAccess.Enums;
using RequestManagementService.DataAccess.Repositories;

namespace RequestManagementService.Application.Features.ServiceRequestEvents.ProcessServiceRequestEvent;

public sealed record ProcessServiceRequestEventCommand(
    long MessageId,
    Guid ServiceRequestId,
    ServiceRequestStatus? OldStatus,
    ServiceRequestStatus NewStatus,
    ServiceRequestPriority Priority,
    DateTime OccurredAt) : IRequest;

internal sealed class ProcessServiceRequestEventCommandHandler : IRequestHandler<ProcessServiceRequestEventCommand>
{
    private readonly IServiceRequestEventRepository _serviceRequestEventRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessServiceRequestEventCommandHandler> _logger;

    public ProcessServiceRequestEventCommandHandler(
        IServiceRequestEventRepository serviceRequestEventRepository,
        IUnitOfWork unitOfWork,
        ILogger<ProcessServiceRequestEventCommandHandler> logger)
    {
        _serviceRequestEventRepository = serviceRequestEventRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(ProcessServiceRequestEventCommand request, CancellationToken cancellationToken)
    {
        if (await _serviceRequestEventRepository.ExistsAsync(request.MessageId, cancellationToken))
        {
            return;
        }

        await _serviceRequestEventRepository.AddAsync(ServiceRequestEventMapper.ToEntity(request), cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        if (request.Priority == ServiceRequestPriority.High || request.NewStatus == ServiceRequestStatus.Done)
        {
            _logger.LogInformation(
                "Service request {ServiceRequestId} changed status from {OldStatus} to {NewStatus} with priority {Priority} at {OccurredAt}",
                request.ServiceRequestId,
                request.OldStatus,
                request.NewStatus,
                request.Priority,
                request.OccurredAt);
        }
    }
}
