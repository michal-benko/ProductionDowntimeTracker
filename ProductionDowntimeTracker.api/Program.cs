using ProductionDowntimeTracker.api.Data;
using ProductionDowntimeTracker.api.Services;
using ProductionDowntimeTracker.api.Options;
using Microsoft.EntityFrameworkCore;
using Opc.Ua;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.Configure<OpcUaOptions>(
    builder.Configuration.GetSection(OpcUaOptions.SectionName));

builder.Services.Configure<AiAssistantOptions>(
    builder.Configuration.GetSection(AiAssistantOptions.SectionName));

builder.Services.AddSingleton<IAiAssistantService, GeminiAiAssistantService>();

builder.Services.AddSingleton<ITelemetryContext>(
    _ => DefaultTelemetry.Create(logging => logging.AddConsole()));

builder.Services.AddSingleton<IOpcUaService, OpcUaService>();

builder.Services.AddDbContext<MachineDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<CsvExportService>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.NumberHandling =
        System.Text.Json.Serialization.JsonNumberHandling.Strict;
});

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("https://localhost:7253")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "Production Downtime Tracker API v1");
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowBlazorClient");

app.UseAuthorization();

app.MapControllers();

app.Run();

