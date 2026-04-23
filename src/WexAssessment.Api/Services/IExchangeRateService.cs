namespace WexAssessment.Api.Services;

public interface IExchangeRateService
{
    Task<(decimal rate, string effectiveDate)?> GetRateForDate(string currency, DateTime transactionDate);
    Task<(decimal rate, string effectiveDate)?> GetLatestRate(string currency);
}