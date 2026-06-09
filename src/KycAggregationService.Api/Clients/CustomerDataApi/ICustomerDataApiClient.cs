using KycAggregationService.Api.Clients.CustomerDataApi.Models;

namespace KycAggregationService.Api.Clients.CustomerDataApi;

public interface ICustomerDataApiClient
{
    Task<PersonalDetailsResponse?> GetPersonalDetailsAsync(string ssn, CancellationToken cancellationToken = default);

    Task<ContactDetailsResponse?> GetContactDetailsAsync(string ssn, CancellationToken cancellationToken = default);

    Task<KycFormResponse?> GetKycFormAsync(string ssn, DateOnly asOfDate, CancellationToken cancellationToken = default);
}