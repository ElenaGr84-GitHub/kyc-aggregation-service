using System.Net;
using System.Net.Http.Json;
using KycAggregationService.Api.Clients.CustomerDataApi.Models;

namespace KycAggregationService.Api.Clients.CustomerDataApi;

public class CustomerDataApiClient(HttpClient httpClient, ILogger<CustomerDataApiClient> logger) : ICustomerDataApiClient
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
            logger.LogInformation("Customer Data API returned not found.");
            return default;
        }

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Customer Data API returned unsuccessful status code {StatusCode}.", (int)response.StatusCode);
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
    }
}