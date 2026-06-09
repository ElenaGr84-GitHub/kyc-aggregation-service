# Reflection

After reading the assignment, I decided to keep the solution fairly small and focused. The task only needs one endpoint and some logic for fetching, aggregating and storing KYC data, so I did not think it needed multiple services.

The solution contains one API project and one test project. The API project is split into folders for the controller, services, external API client, persistence and models. For a larger project I would probably use a more layered structure, but for this assignment I wanted to keep it simple and easy to follow.

## Main flow

The endpoint is:

```http
GET /kyc-data/{ssn}
```

When a request comes in, the service first checks if aggregated data already exists in the local database.

If it exists, the data is returned from the database.

If it does not exist, the service calls the Customer Data APIs, gets the personal details, contact details and KYC form data, aggregates the values, saves the result locally and returns it.

This means the external API is only called the first time for an SSN. Later requests use the locally saved data.

## Persistence

I used SQLite together with Entity Framework Core.

Redis could also have been used for persistence, but I felt that it would add an extra dependency without giving much value for a small coding assignment. SQLite is simple to run locally and still fulfils the requirement that data should remain after an application restart.

I added a unique index on SSN so the same customer should not be stored more than once.

The SQLite connection string is stored in `appsettings.json`. For this assignment I think that is acceptable because it only points to a local SQLite file and does not contain a password. In a real production application I would not put sensitive connection strings or secrets directly in `appsettings.json`. I would use environment variables, user secrets, Azure Key Vault or another secret store instead.

## Aggregation choices

The required fields from the API contract are:

* `ssn`
* `first_name`
* `last_name`
* `address`
* `tax_country`

I decided not to save partially aggregated records when required fields are missing. If incomplete data was saved, later requests could return invalid cached data from the local database. Optional values such as email, phone number and income may be missing, but the required fields must exist before the record is stored.

For contact details, I use the preferred email and phone number when there are multiple values. If no value is marked as preferred, the first usable value is used.

For `tax_country`, I first use the value from the KYC form if it exists. If it is missing, I fall back to the country from the address and map common country names to country codes.

Income is optional. If the income value is missing or cannot be parsed as an integer, I return `null` instead of failing the whole request.

The three Customer Data API calls are made in parallel because they do not depend on each other. This makes the request a bit faster while still keeping the code understandable.

Note: For several of the fields the naming didn't match the contract. in this assignment I decided to implement support both variants. 

## Structure and testability

The controller is kept quite thin. It validates the SSN format, calls the aggregation service and returns the correct HTTP response.

Most of the business logic is placed in `KycDataAggregationService`.

I created an interface for the Customer Data API client. This makes the service easier to test, since the unit tests can use a fake client instead of calling the real external API.

I tried to avoid unnecessary abstractions. My goal was to separate the parts that have different responsibilities, but not create extra layers just for the sake of it.

I also kept comments limited. I prefer the code to be readable through naming and structure and only add comments where a business rule or design choice is not obvious.

## Error handling and logging

The API returns:

* `400 Bad Request` for invalid SSN format
* `404 Not Found` when customer data cannot be found or required data is missing
* `500 Internal Server Error` for unexpected errors

Unexpected errors are handled by global middleware and are logged.

For this assignment, console logging is enough. In a production system I would use centralized logging and monitoring, for example Azure Application Insights, ELK or a similar service.

## Testing

I focused the unit tests on `KycDataAggregationService`, because that is where most of the important logic is.

The tests use a fake Customer Data API client instead of calling the real API. For the database, the tests use SQLite in-memory. This keeps the tests fast while still testing EF Core behavior.

The tests cover:

* returning cached data from the database
* fetching and saving data when nothing is cached
* not calling the external API again on a second request
* missing API responses
* missing required fields
* optional fields being missing
* invalid income
* preferred email and phone number
* using `tax_country` from the KYC form when available

With more time I would also add integration tests for the actual HTTP endpoint.

## Version control

I used Git during the project and kept the work split into smaller commits. This makes it easier to follow how the solution was built up.

## Possible improvements

If I had more time, I would consider:

* adding integration tests for the API endpoint
* using a more explicit result type instead of returning `null` from the service
* adding retry and timeout policies for the external API, for example with Polly
* improving SSN validation (in this assignment none of the SSNs were actually valid)
* adding API versioning
* adding Docker support
* adding a CI/CD pipeline
* moving configuration and secrets to a safer place for production
* adding centralized logging and monitoring
* making the country code mapping more complete

For this assignment, I focused on keeping the solution simple, testable and close to the requirements.
