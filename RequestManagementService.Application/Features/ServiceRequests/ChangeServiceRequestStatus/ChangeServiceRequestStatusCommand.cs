using MediatR;
using Microsoft.EntityFrameworkCore;
using RequestManagementService.Application.Exceptions;
using RequestManagementService.Application.Features.ServiceRequests.Common;
using RequestManagementService.Application.Mappers;
using RequestManagementService.DataAccess;
using RequestManagementService.DataAccess.Repositories;
using RequestManagementService.Domain.ServiceRequests;

namespace RequestManagementService.Application.Features.ServiceRequests.ChangeServiceRequestStatus;

public sealed record ChangeServiceRequestStatusCommand(
    Guid Id,
    ServiceRequestStatus Status) : IRequest<ServiceRequestItem>;

internal sealed class ChangeServiceRequestStatusCommandHandler(
    IServiceRequestRepository serviceRequestRepository,
    IUnitOfWork unitOfWork,
    IServiceRequestStatusTransitions statusTransitions)
    : IRequestHandler<ChangeServiceRequestStatusCommand, ServiceRequestItem>
{
    public async Task<ServiceRequestItem> Handle(
        ChangeServiceRequestStatusCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await serviceRequestRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Service request '{request.Id}' was not found.");

        if (!statusTransitions.IsAllowed(entity.Status, request.Status))
        {
            throw new ConflictException(
                $"Status of service request '{request.Id}' cannot be changed from {entity.Status} to {request.Status}.");
        }

        entity.Status = request.Status;

        try
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException($"Service request '{request.Id}' was modified by another request.");
        }

        return ServiceRequestMapper.ToItem(entity);
    }
}
