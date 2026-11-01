using RequestManagementService.Application.Extensions;
using RequestManagementService.DataAccess.Extensions;

namespace RequestManagementService.API.Starters;

internal static class WorkerStarter
{
    public static Task StartAsync(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddApplication();
        builder.Services.AddDataAccess(builder.Configuration);
        builder.Services.AddEventConsuming(builder.Configuration);

        return builder.Build().RunAsync();
    }
}
