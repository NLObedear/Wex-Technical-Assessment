using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WexAssessment.Api.Data;
using WexAssessment.Api.DTOs;
using WexAssessment.Api.Models;
using WexAssessment.Api.Services;

namespace WexAssessment.Api.Controllers;

[ApiController]
[Route("api/cards/{cardId}/transactions")]
public class TransactionsController(AppDbContext db, IExchangeRateService exchangeRateService) : ControllerBase
{
    private readonly AppDbContext _db = db;
    private readonly IExchangeRateService _exchangeRateService = exchangeRateService;

    // POST /api/cards/{cardId}/transactions
    /// <summary>
    /// Creates and stores a new purchase transaction associated with a specific card.
    /// </summary>
    /// <param name="cardId">The unique identifier of the card to associate the transaction with</param>
    /// <param name="body">The request body containing the description, transaction date, and amount</param>
    /// <returns>The result of the transaction creation request</returns>
    [HttpPost]
    public async Task<IActionResult> CreateTransaction(Guid cardId, [FromBody] CreateTransaction body)
    {
        var card = await _db.Cards.FindAsync(cardId);
        if (card == null) return NotFound("Card not found.");

        if (body.Amount <= 0)
            return BadRequest("Transaction amount must be greater than zero.");

        var transaction = new Transaction
        {
            CardId = cardId,
            Description = body.Description,
            TransactionDate = body.TransactionDate,
            Amount = Math.Round(body.Amount, 2)
        };

        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTransaction),
            new { cardId, id = transaction.Id },
            new { transaction.Id, transaction.Description, transaction.TransactionDate, transaction.Amount });
    }

    // GET /api/cards/{cardId}/transactions/{id}?currency=AUD
    /// <summary>
    /// Retrieves a specific transaction converted to a specified currency using the exchange rate
    /// active on or before the transaction date within a 6 month window.
    /// </summary>
    /// <param name="cardId">The unique identifier of the card the transaction belongs to</param>
    /// <param name="id">The unique identifier of the transaction</param>
    /// <param name="currency">The country name to convert the transaction amount to (e.g. "Australia-Dollar"). Defaults to USD</param>
    /// <returns>The result of the get transaction request</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransaction(Guid cardId, Guid id, [FromQuery] string currency = "USD")
    {
        var transaction = await _db.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.CardId == cardId);

        if (transaction == null) return NotFound("Transaction not found.");

        if (currency.Equals("USD", StringComparison.CurrentCultureIgnoreCase))
        {
            return Ok(new TransactionResponse(
                transaction.Id, transaction.Description, transaction.TransactionDate,
                transaction.Amount, 1.0m, transaction.Amount, "USD"));
        }

        var rateResult = await _exchangeRateService.GetRateForDate(currency, transaction.TransactionDate);

        if (rateResult == null)
            return BadRequest($"No exchange rate available for {currency} within 6 months of the transaction date.");

        var converted = Math.Round(transaction.Amount * rateResult.Value.rate, 2);

        return Ok(new TransactionResponse(
            transaction.Id, transaction.Description, transaction.TransactionDate,
            transaction.Amount, rateResult.Value.rate, converted, currency));
    }
}