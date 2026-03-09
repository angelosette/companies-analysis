using System.Net;
using System.Text;
using CompaniesAnalysis.Infrastructure.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CompaniesAnalysis.IntegrationTests;

public class SecEdgarClientTests
{
    private static SecEdgarClient CreateClient(HttpResponseMessage response)
    {
        var handler = new FakeHttpMessageHandler(response);
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://data.sec.gov/api/xbrl/companyfacts/") };
        return new SecEdgarClient(http, NullLogger<SecEdgarClient>.Instance);
    }

    private static SecEdgarClient CreateThrowingClient(Exception ex)
    {
        var http = new HttpClient(new ThrowingHttpMessageHandler(ex)) { BaseAddress = new Uri("https://data.sec.gov/") };
        return new SecEdgarClient(http, NullLogger<SecEdgarClient>.Instance);
    }

    [Fact]
    public async Task WhenHttpRequestThrows_ReturnsNull()
    {
        var client = CreateThrowingClient(new HttpRequestException("network error"));

        var result = await client.FetchCompanyAsync(12345);

        Assert.Null(result);
    }

    [Fact]
    public async Task WhenResponseIsNotSuccess_ReturnsNull()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.NotFound));

        var result = await client.FetchCompanyAsync(12345);

        Assert.Null(result);
    }

    [Fact]
    public async Task WhenResponseBodyIsInvalidJson_ReturnsNull()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{ not valid json !", Encoding.UTF8, "application/json")
        };
        var client = CreateClient(response);

        var result = await client.FetchCompanyAsync(12345);

        Assert.Null(result);
    }

    [Fact]
    public async Task WhenValidResponse_FiltersToAnnual10KRecordsOnly()
    {
        var json = """
        {
            "cik": 12345,
            "entityName": "Test Corp",
            "facts": {
                "us-gaap": {
                    "NetIncomeLoss": {
                        "units": {
                            "usd": [
                                { "form": "10-K", "frame": "CY2021", "filed": "2022-03-01", "val": 1000000 },
                                { "form": "10-Q", "frame": "CY2021Q1", "filed": "2021-05-01", "val": 200000 },
                                { "form": "10-K", "frame": "CY2022", "filed": "2023-03-01", "val": 2000000 }
                            ]
                        }
                    }
                }
            }
        }
        """;
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        var client = CreateClient(response);

        var result = await client.FetchCompanyAsync(12345);

        Assert.NotNull(result);
        Assert.Equal(12345, result.Cik);
        Assert.Equal("Test Corp", result.EntityName);
        Assert.Equal(2, result.YearlyIncomeData.Count);
        Assert.Contains(result.YearlyIncomeData, r => r.Year == 2021 && r.Value == 1_000_000m);
        Assert.Contains(result.YearlyIncomeData, r => r.Year == 2022 && r.Value == 2_000_000m);
    }

    [Fact]
    public async Task WhenDuplicateFrames_TakesLatestFiledRecord()
    {
        var json = """
        {
            "cik": 12345,
            "entityName": "Test Corp",
            "facts": {
                "us-gaap": {
                    "NetIncomeLoss": {
                        "units": {
                            "usd": [
                                { "form": "10-K", "frame": "CY2021", "filed": "2021-03-01", "val": 500000 },
                                { "form": "10-K", "frame": "CY2021", "filed": "2022-02-15", "val": 999000 }
                            ]
                        }
                    }
                }
            }
        }
        """;
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        var client = CreateClient(response);

        var result = await client.FetchCompanyAsync(12345);

        Assert.NotNull(result);
        Assert.Single(result.YearlyIncomeData);
        Assert.Equal(999_000m, result.YearlyIncomeData[0].Value);
    }

    [Fact]
    public async Task WhenFactsIsNull_ReturnsEmptyYearlyData()
    {
        var json = """{ "cik": 99, "entityName": "Empty Corp", "facts": null }""";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        var client = CreateClient(response);

        var result = await client.FetchCompanyAsync(99);

        Assert.NotNull(result);
        Assert.Empty(result.YearlyIncomeData);
    }

    [Theory]
    [InlineData("CY2021Q1")]    // quarterly frame — excluded
    [InlineData("CY21")]        // length != 6
    [InlineData("AB2021")]      // doesn't start with CY
    [InlineData("CY202X")]      // non-numeric year
    [InlineData(null)]          // null frame
    public async Task WhenFrameIsNotAnnual_RecordIsExcluded(string? frame)
    {
        var frameJson = frame is null ? "null" : $"\"{frame}\"";
        var json = $$"""
        {
            "cik": 1,
            "entityName": "Corp",
            "facts": {
                "us-gaap": {
                    "NetIncomeLoss": {
                        "units": {
                            "usd": [
                                { "form": "10-K", "frame": {{frameJson}}, "filed": "2022-01-01", "val": 100 }
                            ]
                        }
                    }
                }
            }
        }
        """;
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        var client = CreateClient(response);

        var result = await client.FetchCompanyAsync(1);

        Assert.NotNull(result);
        Assert.Empty(result.YearlyIncomeData);
    }
}

internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage _response;

    public FakeHttpMessageHandler(HttpResponseMessage response) => _response = response;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(_response);
}

internal sealed class ThrowingHttpMessageHandler : HttpMessageHandler
{
    private readonly Exception _exception;

    public ThrowingHttpMessageHandler(Exception exception) => _exception = exception;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
        => throw _exception;
}
