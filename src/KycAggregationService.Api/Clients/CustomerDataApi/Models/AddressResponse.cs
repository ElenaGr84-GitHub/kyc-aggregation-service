using System.Text.Json.Serialization;

namespace KycAggregationService.Api.Clients.CustomerDataApi.Models;

public class AddressResponse
{
    [JsonPropertyName("street")]
    public string? Street { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("postalCode")]
    public string? PostalCode { get; set; }

    [JsonPropertyName("postal_code")]
    public string? PostalCodeSnakeCase { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonIgnore]
    public string? EffectivePostalCode => PostalCode ?? PostalCodeSnakeCase;
}