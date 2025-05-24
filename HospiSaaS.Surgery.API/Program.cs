using System.Text.Json;
using System.Text.Json.Serialization;
using HospiSaaS.Application.Interfaces;
using HospiSaaS.Application.Services;
using HospiSaaS.Domain.Repositories;
using HospiSaaS.Infrastructure.Data;
using HospiSaaS.Infrastructure.Notifiers;
using HospiSaaS.Infrastructure.Repositories;
using HospiSaaS.Infrastructure.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<INotifier, ConsoleNotifier>();
builder.Services.AddSingleton<IHospitalRepository, InMemoryHospitalRepository>();
builder.Services.AddSingleton<SchedulingService>();
builder.Services.AddHostedService<WaitingListProcessorHostedService>();

builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

SeedSampleData(app.Services);

if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "HospiSaaS Surgery API");
    });
    app.MapScalarApiReference(options => { options.WithTitle("HospiSaaS Surgery API").WithDarkMode(true); });
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();

static void SeedSampleData(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var hospitalRepo = scope.ServiceProvider.GetRequiredService<IHospitalRepository>();
    SampleDataSeeder.Seed(hospitalRepo);
}

public partial class Program { }