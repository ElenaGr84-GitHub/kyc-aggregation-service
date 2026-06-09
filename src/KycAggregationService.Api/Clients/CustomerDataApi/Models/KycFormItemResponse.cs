using System.Text.Json.Serialization;

namespace KycAggregationService.Api.Clients.CustomerDataApi.Models;

public class KycFormItemResponse
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}