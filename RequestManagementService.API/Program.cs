namespace RequestManagementService.API;

public class Program
{
    private static Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        var app = builder.Build();

        app.MapControllers();

        return app.RunAsync();
    }
}
