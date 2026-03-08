namespace CompaniesAnalysis.Application.DTOs;

public record CompanyFundingDto(int Id, string Name, decimal StandardFundableAmount, decimal SpecialFundableAmount);