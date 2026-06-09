using System.Net;
using System.Net.Http.Json;
using KycAggregationService.Api.Clients.CustomerDataApi.Models;

namespace KycAggregationService.Api.Clients.CustomerDataApi;

public class CustomerDataApiClient(HttpClient httpClient) : ICustomerDataApiClient
{
    public Task<PersonalDetailsResponse?> GetPersonalDetailsAsync(string ssn, CancellationToken cancellationToken = default)
    {
        return GetAsync<PersonalDetailsResponse>($"personal-details/{Uri.EscapeDataString(ssn)}", cancellationToken);
    }

    public Task<ContactDetailsResponse?> GetContactDetailsAsync(string ssn, CancellationToken cancellationToken = default)
    {
        return GetAsync<ContactDetailsResponse>($"contact-details/{Uri.EscapeDataString(ssn)}", cancellationToken);
    }

    public Task<KycFormResponse?> GetKycFormAsync(string ssn, DateOnly asOfDate, CancellationToken cancellationToken = default)
    {
        var date = asOfDate.ToString("yyyy-MM-dd");

        return GetAsync<KycFormResponse>($"kyc-form/{Uri.EscapeDataString(ssn)}/{date}", cancellationToken);
    }

    private async Task<T?> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(path, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
    }
}