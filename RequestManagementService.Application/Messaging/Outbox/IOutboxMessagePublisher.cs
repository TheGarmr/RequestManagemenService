using RequestManagementService.DataAccess.Entities;

namespace RequestManagementService.Application.Messaging.Outbox;

public interface IOutboxMessagePublisher
{
    Task PublishAsync(OutboxMessageEntity message, CancellationToken cancellationToken);
}
