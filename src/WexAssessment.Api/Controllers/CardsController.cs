using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WexAssessment.Api.Data;
using WexAssessment.Api.DTOs;
using WexAssessment.Api.Models;
using WexAssessment.Api.Services;

namespace WexAssessment.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CardsController(AppDbContext db, IExchangeRateService exchangeRateService) : ControllerBase
{
    private readonly AppDbContext _db = db;
    private readonly IExchangeRateService _exchangeRateService = exchangeRateService;

    const string DefaultCurrency = "USD";
    const decimal DefaultExchangeRate = 1.0m;

    // POST /api/cards
    /// <summary>
    /// Creates a new card with a specified credit limit.
    /// </summary>
    /// <param name="body">The request body containing the credit limit for the card</param>
    /// <returns>The result of the card creation request</returns>
    [HttpPost]
    public async Task<IActionResult> CreateCard([FromBody] CreateCard body)
    {
        if (body.CreditLimit <= 0)
            return BadRequest("Credit limit must be greater than zero.");
            
        var card = new Card { CreditLimit = body.CreditLimit };
        _db.Cards.Add(card);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBalance), new { id = card.Id }, new { card.Id, card.CreditLimit });
    }

    // GET /api/cards/{id}/balance?currency=AUD
    /// <summary>
    /// Retrieves the available balance for a card, optionally converted to a specified currency.
    /// Available balance is calculated as credit limit minus total transactions.
    /// </summary>
    /// <param name="id">The unique identifier of the card</param>
    /// <param name="currency">The country name to convert the balance to (e.g. "Canada"). Defaults to USD</param>
    /// <returns>The result of the get balance request</returns>
    [HttpGet("{id}/balance")]
    public async Task<IActionResult> GetBalance(Guid id, [FromQuery] string currency = DefaultCurrency)
    {
        var card = await _db.Cards
            .Include(c => c.Transactions)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (card == null) return NotFound("Card not found.");

        var totalTransactions = card.Transactions.Sum(t => t.Amount);
        var availableBalanceUsd = card.CreditLimit - totalTransactions;

        if (currency.Equals(DefaultCurrency, StringComparison.CurrentCultureIgnoreCase))
        {
            return Ok(new CardBalanceResponse(
                card.Id, card.CreditLimit, totalTransactions,
                availableBalanceUsd, availableBalanceUsd, DefaultCurrency, DefaultExchangeRate));
        }

        var rateResult = await _exchangeRateService.GetLatestRate(currency);
        if (rateResult == null)
            return BadRequest($"No exchange rate available for currency: {currency}");

        var converted = availableBalanceUsd * rateResult.Value.rate;

        return Ok(new CardBalanceResponse(
            card.Id, card.CreditLimit, totalTransactions,
            availableBalanceUsd, converted, currency, rateResult.Value.rate));
    }
}