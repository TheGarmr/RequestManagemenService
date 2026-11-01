using RequestManagementService.DataAccess.Extensions;

namespace RequestManagementService.API;

public class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddDataAccess(builder.Configuration);

        var app = builder.Build();

        app.MapControllers();

        await app.Services.ApplyMigrationsAsync();
        await app.RunAsync();
    }
}
