namespace WexAssessment.Api.DTOs;

public record CreateTransaction(
    string Description,
    DateTime TransactionDate,
    decimal Amount
);