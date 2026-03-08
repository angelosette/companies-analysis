# Companies Analysis Coding Challenge

A production-ready .NET 10 API that imports company income data from the SEC EDGAR API and exposes an endpoint to retrieve companies with their calculated fundable amounts.

## Architecture

```
src/    
  CompaniesAnalysis.Api/               Minimal API endpoints + API key middleware + Scalar docs
  CompaniesAnalysis.Application/       Service + Strategy pattern for funding calculations
  CompaniesAnalysis.Domain/            Entities, interfaces, FundingConstants
  CompaniesAnalysis.Infrastructure/    EF Core + SQLite + Polly HTTP client + background import
tests/
  CompaniesAnalysis.UnitTests/         xUnit tests for funding strategies, import logic, domain entities
  CompaniesAnalysis.IntegrationTests/  Full API tests with SQLite in-memory
.github/workflows/        CI/CD: build -> test + coverage -> Docker -> ACR -> Container Apps
```

## Running Locally

**Prerequisites:** .NET 10 SDK

```bash
dotnet run --project src/CompaniesAnalysis.Api
```

**API docs:** navigate to `{base_url}/scalar`

**Auth:** all endpoints require `X-Api-Key` header. Dev key: `dev-api-key-change-me`

## Endpoints

### GET /api/companies
Returns companies with fundable amounts. Optional `?startsWith=A` filter.

```json
[{ "id": 1, "name": "Uber Technologies", "standardFundableAmount": 123.45, "specialFundableAmount": 234.56 }]
```

### POST /api/import
Manually triggers SEC EDGAR import (also runs on startup via background service).

### GET /health  |  GET /alive
Health checks (no auth required).

## Funding Rules

**Standard Fundable Amount**
- Requires income data for all years 2018-2022, else $0
- Requires positive income in 2021 AND 2022, else $0
- Highest income >= $10B -> 12.33% | < $10B -> 21.51%

**Special Fundable Amount**
- Starts equal to Standard
- Name starts with vowel -> +15% of standard
- 2022 income < 2021 income -> -25% of standard

## Tests

```bash
dotnet test tests/CompaniesAnalysis.UnitTests
dotnet test tests/CompaniesAnalysis.IntegrationTests
```

Coverage report is generated automatically in CI and uploaded as a GitHub Actions artifact.

## Azure Container Apps Setup

All infrastructure is provisioned via GitHub Actions — no local `az` CLI required.

1. Add the [required secrets](#required-github-secrets) to your repository (`Settings → Secrets and variables → Actions`).
2. Create the Azure resources manually in the portal (resource group, Container Apps environment, Container App) — see [setup steps](#azure-container-apps-setup).
3. Push to `main` — the **CI/CD** workflow will build, test, push the Docker image to `ghcr.io`, and deploy to the Container App automatically.

## Required GitHub Secrets

| Secret | Description |
|---|---|
| `AZURE_CREDENTIALS` | Service principal JSON |
| `RESOURCE_GROUP` | Azure resource group name |
| `CONTAINERAPP_NAME` | Azure Container App name |
| `GHCR_TOKEN` | GitHub Personal Access Token with `read:packages` scope (used by Container Apps to pull images) |