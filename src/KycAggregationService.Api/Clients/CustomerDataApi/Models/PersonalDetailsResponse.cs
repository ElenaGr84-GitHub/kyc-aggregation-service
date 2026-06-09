using System.Text.Json.Serialization;

namespace KycAggregationService.Api.Clients.CustomerDataApi.Models;

public class PersonalDetailsResponse
{
    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("sur_name")]
    public string? SurName { get; set; }
}