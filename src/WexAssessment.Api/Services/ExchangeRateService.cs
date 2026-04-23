using System.Text.Json;

namespace WexAssessment.Api.Services;

public class ExchangeRateService : IExchangeRateService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://api.fiscaldata.treasury.gov/services/api/fiscal_service/v1/accounting/od/rates_of_exchange";

    public ExchangeRateService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Retrieves the exchange rate for a specified currency on or before the transaction date,
    /// within a 6 month lookback window.
    /// </summary>
    /// <param name="country_currency">The country name to filter by (e.g. "Canada-Dollar", "Australia-Dollar")</param>
    /// <param name="transactionDate">The date of the transaction to find the applicable rate for</param>
    /// <returns>A tuple of (rate, effectiveDate) if found, or null if no rate exists within 6 months</returns>
    public async Task<(decimal rate, string effectiveDate)?> GetRateForDate(string country_currency, DateTime transactionDate)
    {
        var sixMonthsAgo = transactionDate.AddMonths(-6).ToString("yyyy-MM-dd");
        var transactionDateStr = transactionDate.ToString("yyyy-MM-dd");

        var url = $"{BaseUrl}?fields=exchange_rate,record_date,currency,country" +
                $"&filter=country_currency_desc:eq:{Uri.EscapeDataString(country_currency)}" +
                $",record_date:lte:{transactionDateStr}" +
                $",record_date:gte:{sixMonthsAgo}" +
                $"&sort=-record_date&page[size]=1";

        return await GetExchangeRateRequest(url);
    }

    /// <summary>
    /// Retrieves the most recent available exchange rate for a specified currency.
    /// </summary>
    /// <param name="country_currency">The country name to filter by (e.g. "Canada-Dollar", "Australia-Dollar")</param>
    /// <returns>A tuple of (rate, effectiveDate) if found, or null if no rate exists</returns>
    public async Task<(decimal rate, string effectiveDate)?> GetLatestRate(string country_currency)
    {
        var url = $"{BaseUrl}?fields=exchange_rate,record_date,currency,country" +
                $"&filter=country_currency_desc:eq:{Uri.EscapeDataString(country_currency)}" +
                $"&sort=-record_date&page[size]=1";

        return await GetExchangeRateRequest(url);
    }

    /// <summary>
    /// Sends a request to the Treasury API and parses the exchange rate from the response.
    /// </summary>
    /// <param name="url">The fully constructed Treasury API URL to call</param>
    /// <returns>A tuple of (rate, effectiveDate) if found, or null if the response is empty or unsuccessful</returns>
    private async Task<(decimal rate, string effectiveDate)?> GetExchangeRateRequest(string url)
    {
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var data = doc.RootElement.GetProperty("data");

        if (data.GetArrayLength() == 0) return null;

        var entry = data[0];
        var rate = decimal.Parse(entry.GetProperty("exchange_rate").GetString()!);
        var date = entry.GetProperty("record_date").GetString()!;

        return (rate, date);
    }
}