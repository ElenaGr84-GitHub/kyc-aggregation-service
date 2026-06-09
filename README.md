# KYC Aggregation Service

This is a small .NET API for aggregating KYC data for a customer.

The API has one endpoint:

```http
GET /kyc-data/{ssn}
```

The first time an SSN (personal identity number) is requested, the service gets the data from the Customer Data APIs, saves the aggregated result in a local SQLite database and returns it.

If the same SSN is requested again, the saved data is returned from the database instead of calling the external API again.

## Projects

The solution contains two projects:

* `KycAggregationService.Api` - the API project
* `KycAggregationService.Tests` - unit tests

## Tech used

* .NET 10
* ASP.NET Core
* Entity Framework Core
* SQLite
* xUnit

## How to run

Restore packages:

```bash
dotnet restore KycAggregationService.slnx
```

Create/update the local database:

```bash
dotnet ef database update --project src/KycAggregationService.Api --startup-project src/KycAggregationService.Api
```

Run the API:

```bash
dotnet run --project src/KycAggregationService.Api
```

The API can then be called with for example:

```bash
curl http://localhost:5233/kyc-data/19800115-1234
```

## Test data

The external API has test data for these SSNs:

```text
19800115-1234
19900220-5678
19751230-9101
19850505-4321
19951212-3456
```

## Configuration

The Customer Data API base URL and the SQLite connection string are configured in:

```text
src/KycAggregationService.Api/appsettings.json
```

The SQLite database is created locally when the migrations are applied.

## How it works

When a request comes in, the service first checks if the SSN already exists in the local database.

If it exists, that data is returned.

If it does not exist, the service calls the Customer Data API, collects the personal details, contact details and KYC form data and then saves the aggregated result.

The required fields are:

* `ssn`
* `first_name`
* `last_name`
* `address`
* `tax_country`

If any required data is missing, the API returns `404 Not Found`.

## Error handling

The API returns:

* `400 Bad Request` for an invalid SSN format
* `404 Not Found` when the customer data cannot be found or required data is missing
* `500 Internal Server Error` for unexpected errors

Unexpected errors are also logged to the console.

## Running tests

Run all tests with:

```bash
dotnet test KycAggregationService.slnx
```

The tests focus on the aggregation service. They cover the main flow, cached data, missing data and some edge cases around optional fields.
