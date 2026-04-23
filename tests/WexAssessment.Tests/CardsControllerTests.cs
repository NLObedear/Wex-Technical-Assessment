using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using WexAssessment.Api.DTOs;
using Xunit;

namespace WexAssessment.Tests;

public class CardsControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    const decimal defaultCreditLimit = 1000;

    //POST /api/cards
    [Fact]
    public async Task CreateCard_Returns201_WithValidRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/cards", new CreateCard(defaultCreditLimit));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        body.GetProperty("id").GetString().Should().NotBeNullOrEmpty();
        body.GetProperty("creditLimit").GetDecimal().Should().Be(defaultCreditLimit);
    }

    [Fact]
    public async Task CreateCard_Returns400_WhenCreditLimitIsNull()
    {
        var response = await _client.PostAsJsonAsync("/api/cards", new { creditLimit = (decimal?)null });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCard_Returns400_WhenCreditLimitIsZero()
    {
        var response = await _client.PostAsJsonAsync("/api/cards", new { creditLimit = 0 });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCard_Returns400_WhenCreditLimitIsNegative()
    {
        var response = await _client.PostAsJsonAsync("/api/cards", new { creditLimit = -100 });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCard_Returns400_WhenCreditLimitIsNotANumber()
    {
        var response = await _client.PostAsJsonAsync("/api/cards", new { creditLimit = "abc" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCard_ReturnsUniqueIds_WhenMultipleCardsCreated()
    {
        var response1 = await _client.PostAsJsonAsync("/api/cards", new CreateCard(defaultCreditLimit));
        var response2 = await _client.PostAsJsonAsync("/api/cards", new CreateCard(defaultCreditLimit));

        var body1 = await response1.Content.ReadFromJsonAsync<JsonElement>();
        var body2 = await response2.Content.ReadFromJsonAsync<JsonElement>();

        body1.GetProperty("id").GetString().Should()
            .NotBe(body2.GetProperty("id").GetString());
    }

    //GET /api/cards/{cardId}/balance
    [Fact]
    public async Task GetCardBalance_Returns200_WithValidRequest()
    {
        var cardId = await TestUtility.CreateCardAsync(_client, defaultCreditLimit);
        var response = await _client.GetAsync($"/api/cards/{cardId}/balance");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        TestUtility.AssertBodyMatchesExpected(body, new
        {
            cardId = cardId.ToString(),
            creditLimit = defaultCreditLimit,
            totalTransactions = 0,
            availableBalanceUsd = defaultCreditLimit,
            currency = "USD"
        });
    }

    [Fact]
    public async Task GetCardBalance_Returns404_WhenCardDoesNotExist()
    {
        var response = await _client.GetAsync($"/api/cards/{Guid.NewGuid()}/balance");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}