using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using WexAssessment.Api.Services;
using Xunit;

namespace WexAssessment.Tests;

public class ExchangeRateServiceTests
{
    private readonly DateTime testDate = new(2024, 4, 1);

    [Fact]
    public async Task GetRateForDateAsync_ReturnsNull_WhenNoRateAvailable()
    {
        var handler = new MockHttpMessageHandler("{\"data\": []}");
        var httpClient = new HttpClient(handler);
        var service = new ExchangeRateService(httpClient);

        var result = await service.GetRateForDate("Australian-Dollar", testDate);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRateForDateAsync_ReturnsRate_WhenRateExists()
    {
        decimal expectedRate = 1.5m;
        var handler = new MockHttpMessageHandler("""
        {
            "data": [
                {
                    "exchange_rate": "1.5",
                    "record_date": "2024-03-31",
                    "currency": "Australian-Dollar"
                }
            ]
        }
        """);

        var httpClient = new HttpClient(handler);
        var service = new ExchangeRateService(httpClient);

        var result = await service.GetRateForDate("Australian-Dollar", testDate);

        result.Should().NotBeNull();
        result!.Value.rate.Should().Be(expectedRate);
    }
}