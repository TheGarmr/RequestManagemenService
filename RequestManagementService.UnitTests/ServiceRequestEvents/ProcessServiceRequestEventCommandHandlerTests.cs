using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using RequestManagementService.Application.Features.ServiceRequestEvents.ProcessServiceRequestEvent;
using RequestManagementService.DataAccess;
using RequestManagementService.DataAccess.Entities;
using RequestManagementService.DataAccess.Enums;
using RequestManagementService.DataAccess.Repositories;

namespace RequestManagementService.UnitTests.ServiceRequestEvents;

public sealed class ProcessServiceRequestEventCommandHandlerTests
{
    private readonly IServiceRequestEventRepository _repository = Substitute.For<IServiceRequestEventRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ProcessServiceRequestEventCommandHandler _handler;

    public ProcessServiceRequestEventCommandHandlerTests()
    {
        _handler = new ProcessServiceRequestEventCommandHandler(
            _repository,
            _unitOfWork,
            NullLogger<ProcessServiceRequestEventCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WhenEventIsNew_SavesIt()
    {
        var command = CreateCommand(42);
        _repository.ExistsAsync(42, Arg.Any<CancellationToken>()).Returns(false);

        await _handler.Handle(command, CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<ServiceRequestEventEntity>(entity =>
                entity.MessageId == 42
                && entity.ServiceRequestId == command.ServiceRequestId
                && entity.OldStatus == ServiceRequestStatus.New
                && entity.NewStatus == ServiceRequestStatus.InProgress
                && entity.Priority == ServiceRequestPriority.High
                && entity.OccurredAt == command.OccurredAt),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEventWasAlreadyProcessed_SkipsIt()
    {
        _repository.ExistsAsync(42, Arg.Any<CancellationToken>()).Returns(true);

        await _handler.Handle(CreateCommand(42), CancellationToken.None);

        await _repository.DidNotReceive().AddAsync(Arg.Any<ServiceRequestEventEntity>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    private static ProcessServiceRequestEventCommand CreateCommand(long messageId)
    {
        return new ProcessServiceRequestEventCommand(
            messageId,
            Guid.CreateVersion7(),
            ServiceRequestStatus.New,
            ServiceRequestStatus.InProgress,
            ServiceRequestPriority.High,
            new DateTime(2026, 11, 1, 10, 0, 0, DateTimeKind.Utc));
    }
}
