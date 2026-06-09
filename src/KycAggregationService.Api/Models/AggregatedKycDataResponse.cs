using System.Text.Json.Serialization;

namespace KycAggregationService.Api.Models;

public class AggregatedKycDataResponse
{
    [JsonPropertyName("ssn")]
    public required string Ssn { get; set; }

    [JsonPropertyName("first_name")]
    public required string FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public required string LastName { get; set; }

    [JsonPropertyName("address")]
    public required string Address { get; set; }

    [JsonPropertyName("phone_number")]
    public string? PhoneNumber { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("tax_country")]
    public required string TaxCountry { get; set; }

    [JsonPropertyName("income")]
    public int? Income { get; set; }
}