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

public class TransactionsControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly DateTime _defaultTransactionDate = DateTime.UtcNow;
    private CreateTransaction DefaultCreateTransaction => new("Test Transaction", _defaultTransactionDate, 100.00m);
    private Guid _cardId;

    async Task IAsyncLifetime.InitializeAsync()
    {
        _cardId = await TestUtility.CreateCardAsync(_client, 1000);
    }

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    private async Task<Guid> CreateTransactionAsync(Guid cardId)
    {
        var response = await _client.PostAsJsonAsync($"/api/cards/{cardId}/transactions", DefaultCreateTransaction);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return Guid.Parse(body.GetProperty("id").GetString()!);
    }

    //POST /api/cards/{cardId}/transactions
    [Fact]
    public async Task CreateTransaction_Returns201_WithValidRequest()
    {
        var response = await _client.PostAsJsonAsync($"/api/cards/{_cardId}/transactions", DefaultCreateTransaction);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        TestUtility.AssertBodyMatchesExpected(body, new
        {
            description = DefaultCreateTransaction.Description,
            transactionDate = DefaultCreateTransaction.TransactionDate,
            amount = DefaultCreateTransaction.Amount,
        });
    }

    [Fact]
    public async Task CreateTransaction_Returns404_WhenCardDoesNotExist()
    {
        var response = await _client.PostAsJsonAsync($"/api/cards/{Guid.NewGuid()}/transactions", DefaultCreateTransaction);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateTransaction_Returns400_WhenAmountIsNegative()
    {
        var response = await _client.PostAsJsonAsync($"/api/cards/{_cardId}/transactions",
            new CreateTransaction("Test Transaction", _defaultTransactionDate, -100.00m));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    //GET /api/cards/{cardId}/transactions/{id}?currency=AUD
    [Fact]
    public async Task GetTransaction_Returns200_WithValidRequest()
    {
        var transactionId = await CreateTransactionAsync(_cardId);
        var response = await _client.GetAsync($"/api/cards/{_cardId}/transactions/{transactionId}");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        TestUtility.AssertBodyMatchesExpected(body, new
        {
            id = transactionId.ToString(),
            description = DefaultCreateTransaction.Description,
            transactionDate = DefaultCreateTransaction.TransactionDate,
            originalAmount = DefaultCreateTransaction.Amount,
            exchangeRate = 1.0m,
            convertedAmount = DefaultCreateTransaction.Amount,
            currency = "USD"
        });
    }

    [Fact]
    public async Task GetTransaction_Returns404_WhenTransactionDoesNotExist()
    {
        var response = await _client.GetAsync($"/api/cards/{_cardId}/transactions/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}