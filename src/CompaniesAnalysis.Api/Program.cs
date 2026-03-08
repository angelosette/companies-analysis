using CompaniesAnalysis.Api.Endpoints;
using CompaniesAnalysis.Api.Middleware;
using CompaniesAnalysis.Application;
using CompaniesAnalysis.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.Configure<ApiKeyConfigOptions>(builder.Configuration.GetSection(ApiKeyConfigOptions.SectionName));
builder.Services.AddOpenApi();

var app = builder.Build();

await app.Services.ApplyMigrationsAsync();

app.UseMiddleware<ApiKeyMiddleware>();
app.MapDefaultEndpoints();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "Companies Analysis API";
    options.Theme = ScalarTheme.Purple;
});

app.MapCompaniesEndpoints();
app.MapImportEndpoints();

app.Run();

public partial class Program { }
