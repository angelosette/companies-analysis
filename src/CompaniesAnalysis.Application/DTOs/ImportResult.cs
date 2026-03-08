namespace CompaniesAnalysis.Application.DTOs;

public record ImportResult(int Imported, int Failed, IReadOnlyList<string> Errors);
