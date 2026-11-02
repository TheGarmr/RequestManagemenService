using System.Text.Json.Serialization;
using RequestManagementService.API.ExceptionHandling;
using RequestManagementService.API.Extensions;
using RequestManagementService.DataAccess;

namespace RequestManagementService.API.Starters;

internal static class ApiStarter
{
    public static async Task StartAsync(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services
            .AddControllers(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(
                new JsonStringEnumConverter(allowIntegerValues: false)));

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services
            .AddHealthChecks()
            .AddDbContextCheck<RequestManagementDbContext>()
            .AddRabbitMqCheck();

        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<ApiExceptionHandler>();

        builder.Services.AddApplication();
        builder.Services.AddDataAccess(builder.Configuration);
        builder.Services.AddOutboxPublishing(builder.Configuration);

        var app = builder.Build();

        app.UseExceptionHandler();
        app.UseStatusCodePages();
        app.UseSwagger();
        app.UseSwaggerUI();
        app.MapControllers();
        app.MapHealthChecks("/health");

        await app.Services.ApplyMigrationsAsync();
        await app.RunAsync();
    }
}
