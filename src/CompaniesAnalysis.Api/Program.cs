using CompaniesAnalysis.Api.Endpoints;
using CompaniesAnalysis.Api.Middleware;
using CompaniesAnalysis.Application;
using CompaniesAnalysis.Infrastructure;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.Configure<ApiKeyConfigOptions>(builder.Configuration.GetSection(ApiKeyConfigOptions.SectionName));
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Components ??= new();
        document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["ApiKey"] = new OpenApiSecurityScheme()
            {
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Name = "X-Api-Key",
                Description = "API key authentication"
            }
        };
        document.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("ApiKey")] = []
            }
        ];
        return Task.CompletedTask;
    });
});
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>    
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

await app.Services.ApplyMigrationsAsync();

app.UseCors();
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
