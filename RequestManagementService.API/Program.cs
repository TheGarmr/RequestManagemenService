using RequestManagementService.API.Starters;

namespace RequestManagementService.API;

public class Program
{
    private const string WorkModeKey = "WorkMode";

    private static Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        return GetWorkMode(configuration[WorkModeKey]) switch
        {
            WorkMode.Api => ApiStarter.StartAsync(args),
            WorkMode.Worker => WorkerStarter.StartAsync(args),
            var workMode => throw new InvalidOperationException($"Unsupported work mode '{workMode}'.")
        };
    }

    private static WorkMode GetWorkMode(string? value)
    {
        if (value is null)
        {
            return WorkMode.Api;
        }

        if (Enum.TryParse<WorkMode>(value, ignoreCase: true, out var workMode) && Enum.IsDefined(workMode))
        {
            return workMode;
        }

        throw new InvalidOperationException(
            $"Unknown work mode '{value}'. Expected one of: {string.Join(", ", Enum.GetNames<WorkMode>())}.");
    }
}
