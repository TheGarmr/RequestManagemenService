using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using RequestManagementService.Application.Exceptions;
using RequestManagementService.Application.Features.ServiceRequests.ChangeServiceRequestStatus;
using RequestManagementService.DataAccess;
using RequestManagementService.DataAccess.Entities;
using RequestManagementService.DataAccess.Repositories;
using RequestManagementService.Domain.ServiceRequests;

namespace RequestManagementService.UnitTests.ServiceRequests;

public sealed class ChangeServiceRequestStatusCommandHandlerTests
{
    private readonly IServiceRequestRepository _repository = Substitute.For<IServiceRequestRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ChangeServiceRequestStatusCommandHandler _handler;

    public ChangeServiceRequestStatusCommandHandlerTests()
    {
        _handler = new ChangeServiceRequestStatusCommandHandler(_repository, _unitOfWork, new ServiceRequestStatusTransitions());
    }

    [Fact]
    public async Task Handle_WhenServiceRequestDoesNotExist_ThrowsNotFoundException()
    {
        var id = Guid.CreateVersion7();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((ServiceRequestEntity?)null);

        await Should.ThrowAsync<NotFoundException>(() =>
            _handler.Handle(new ChangeServiceRequestStatusCommand(id, ServiceRequestStatus.InProgress), CancellationToken.None));

        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTransitionIsForbidden_ThrowsConflictExceptionWithoutCommit()
    {
        var entity = CreateEntity(ServiceRequestStatus.Done);
        _repository.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>()).Returns(entity);

        await Should.ThrowAsync<ConflictException>(() =>
            _handler.Handle(new ChangeServiceRequestStatusCommand(entity.Id, ServiceRequestStatus.New), CancellationToken.None));

        entity.Status.ShouldBe(ServiceRequestStatus.Done);
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTransitionIsAllowed_UpdatesStatusAndCommits()
    {
        var entity = CreateEntity(ServiceRequestStatus.New);
        _repository.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>()).Returns(entity);

        var result = await _handler.Handle(
            new ChangeServiceRequestStatusCommand(entity.Id, ServiceRequestStatus.InProgress),
            CancellationToken.None);

        result.Status.ShouldBe(ServiceRequestStatus.InProgress);
        entity.Status.ShouldBe(ServiceRequestStatus.InProgress);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenConcurrentUpdateDetected_ThrowsConflictException()
    {
        var entity = CreateEntity(ServiceRequestStatus.New);
        _repository.GetByIdAsync(entity.Id, Arg.Any<CancellationToken>()).Returns(entity);
        _unitOfWork.CommitAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new DbUpdateConcurrencyException());

        await Should.ThrowAsync<ConflictException>(() =>
            _handler.Handle(new ChangeServiceRequestStatusCommand(entity.Id, ServiceRequestStatus.InProgress), CancellationToken.None));
    }

    private static ServiceRequestEntity CreateEntity(ServiceRequestStatus status)
    {
        return new ServiceRequestEntity
        {
            Id = Guid.CreateVersion7(),
            Title = "Replace pump bearing",
            Priority = ServiceRequestPriority.Normal,
            Status = status
        };
    }
}
