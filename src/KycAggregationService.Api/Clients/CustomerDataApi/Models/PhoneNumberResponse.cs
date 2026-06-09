using System.Text.Json.Serialization;

namespace KycAggregationService.Api.Clients.CustomerDataApi.Models;

public class PhoneNumberResponse
{
    [JsonPropertyName("preferred")]
    public bool Preferred { get; set; }

    [JsonPropertyName("number")]
    public string? Number { get; set; }
}