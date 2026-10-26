using Microsoft.EntityFrameworkCore;

using OutboxPattern.Api.Endpoints;
using OutboxPattern.Application;
using OutboxPattern.Infrastructure;
using OutboxPattern.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:AutoMigrate"))
{
    await using var scope = app.Services.CreateAsyncScope();

    var dbContext = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    await dbContext.Database.MigrateAsync();
}

app.UseExceptionHandler();

app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "swagger";
    options.SwaggerEndpoint("/openapi/v1.json", "OutboxPattern API");
    options.DocumentTitle = "OutboxPattern API";
});

app.MapOpenApi();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithName("Health")
    .WithTags("Health");

app.MapOrdersEndpoints();
app.MapOutboxEndpoints();

app.Run();

/// <summary>
/// Точка входа приложения, доступная интеграционным тестам.
/// </summary>
public partial class Program
{
}
