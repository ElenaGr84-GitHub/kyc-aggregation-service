using System.Text.Json.Serialization;

namespace KycAggregationService.Api.Clients.CustomerDataApi.Models;

public class PersonalDetailsResponse
{
    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("first_name")]
    public string? FirstNameSnakeCase { get; set; }

    [JsonPropertyName("surName")]
    public string? SurName { get; set; }

    [JsonPropertyName("sur_name")]
    public string? SurNameSnakeCase { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonIgnore]
    public string? EffectiveFirstName => FirstName ?? FirstNameSnakeCase;

    [JsonIgnore]
    public string? EffectiveSurName => SurName ?? SurNameSnakeCase;
}