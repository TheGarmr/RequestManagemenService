namespace RequestManagementService.Application.Messaging.Options;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    public int BatchSize { get; set; }

    public TimeSpan PollingInterval { get; set; }

    public TimeSpan MaxRetryDelay { get; set; }
}
