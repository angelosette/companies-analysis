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

RUN groupadd --system --gid 1001 appgroup \
 && useradd --system --uid 1001 --gid appgroup appuser \
 && mkdir -p /app/data \
 && chown -R appuser:appgroup /app/data

COPY --from=build --chown=appuser:appgroup /app/publish .
USER appuser

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "CompaniesAnalysis.Api.dll"]
