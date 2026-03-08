FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/CompaniesAnalysis.Domain/CompaniesAnalysis.Domain.csproj                     src/CompaniesAnalysis.Domain/
COPY src/CompaniesAnalysis.Application/CompaniesAnalysis.Application.csproj           src/CompaniesAnalysis.Application/
COPY src/CompaniesAnalysis.Infrastructure/CompaniesAnalysis.Infrastructure.csproj     src/CompaniesAnalysis.Infrastructure/
COPY src/CompaniesAnalysis.Api/CompaniesAnalysis.Api.csproj                           src/CompaniesAnalysis.Api/

RUN dotnet restore src/CompaniesAnalysis.Api/CompaniesAnalysis.Api.csproj

COPY . .
RUN dotnet publish src/CompaniesAnalysis.Api/CompaniesAnalysis.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN addgroup --system --gid 1001 appgroup \
 && adduser --system --uid 1001 --ingroup appgroup appuser

COPY --from=build --chown=appuser:appgroup /app/publish .
USER appuser

# Create directory for SQLite database
RUN mkdir -p /app/data

EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "CompaniesAnalysis.Api.dll"]
