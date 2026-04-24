# WEX Take-Home Assessment

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

---

## Run the Application

```bash
docker compose up --build
```

Swagger UI: **http://localhost:5000/swagger**

---

## Run Tests

```bash
dotnet test
```

## Currency Examples

Currency names must match the Treasury API format of `Country-Currency`:
A full list of supported currencies can be found at the [Treasury Reporting Rates of Exchange API](https://api.fiscaldata.treasury.gov/services/api/fiscal_service/v1/accounting/od/rates_of_exchange?fields=country,country_currency_desc&page[size]=100).
