using System.Text.Json.Serialization;

namespace KycAggregationService.Api.Clients.CustomerDataApi.Models;

public class KycFormResponse
{
    [JsonPropertyName("items")]
    public List<KycFormItemResponse> Items { get; set; } = [];
}