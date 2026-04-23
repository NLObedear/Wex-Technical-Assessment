namespace WexAssessment.Api.DTOs;

public record TransactionResponse(
    Guid Id,
    string Description,
    DateTime TransactionDate,
    decimal OriginalAmount,
    decimal ExchangeRate,
    decimal ConvertedAmount,
    string Currency
);

public record CardBalanceResponse(
    Guid CardId,
    decimal CreditLimit,
    decimal TotalTransactions,
    decimal AvailableBalanceUsd,
    decimal AvailableBalanceConverted,
    string Currency,
    decimal ExchangeRate
);