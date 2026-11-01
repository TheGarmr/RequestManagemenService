using RequestManagementService.Application.Features.ServiceRequestEvents.ProcessServiceRequestEvent;
using RequestManagementService.Application.Messaging.Contracts;
using RequestManagementService.DataAccess.Entities;

namespace RequestManagementService.Application.Mappers;

internal static class ServiceRequestEventMapper
{
    public static ProcessServiceRequestEventCommand ToCommand(long messageId, ServiceRequestEventMessage message)
    {
        return new ProcessServiceRequestEventCommand(
            messageId,
            message.ServiceRequestId,
            message.OldStatus,
            message.NewStatus,
            message.Priority,
            message.OccurredAt);
    }

    public static ServiceRequestEventEntity ToEntity(ProcessServiceRequestEventCommand command)
    {
        return new ServiceRequestEventEntity
        {
            MessageId = command.MessageId,
            ServiceRequestId = command.ServiceRequestId,
            OldStatus = command.OldStatus,
            NewStatus = command.NewStatus,
            Priority = command.Priority,
            OccurredAt = command.OccurredAt
        };
    }
}
