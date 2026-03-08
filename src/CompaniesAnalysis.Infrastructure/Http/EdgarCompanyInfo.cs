using System.Text.Json.Serialization;

namespace CompaniesAnalysis.Infrastructure.Http;

public class EdgarCompanyInfo
{
    public int Cik { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public InfoFact? Facts { get; set; }

    public class InfoFact
    {
        [JsonPropertyName("us-gaap")]
        public InfoFactUsGaap? UsGaap { get; set; }
    }

    public class InfoFactUsGaap
    {
        public InfoFactUsGaapNetIncomeLoss? NetIncomeLoss { get; set; }
    }

    public class InfoFactUsGaapNetIncomeLoss
    {
        public InfoFactUsGaapIncomeLossUnits? Units { get; set; }
    }

    public class InfoFactUsGaapIncomeLossUnits
    {
        public InfoFactUsGaapIncomeLossUnitsUsd[]? Usd { get; set; }
    }

    public class InfoFactUsGaapIncomeLossUnitsUsd
    {
        /// <summary>Only interested in 10-K form data.</summary>
        public string Form { get; set; } = string.Empty;

        /// <summary>Annual frames follow CY + 4-digit year (e.g. CY2021). Quarterly are CY2021Q1 etc.</summary>
        public string? Frame { get; set; }

        /// <summary>Filing date - used to pick most recent when duplicates exist per year.</summary>
        public string Filed { get; set; } = string.Empty;

        /// <summary>The income/loss amount.</summary>
        public decimal Val { get; set; }
    }
}
