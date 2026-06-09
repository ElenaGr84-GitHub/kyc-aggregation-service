using System.Text.Json.Serialization;

namespace KycAggregationService.Api.Clients.CustomerDataApi.Models;

public class EmailResponse
{
    [JsonPropertyName("preferred")]
    public bool Preferred { get; set; }

    [JsonPropertyName("emailAddress")]
    public string? EmailAddress { get; set; }

    [JsonPropertyName("email_address")]
    public string? EmailAddressSnakeCase { get; set; }

    [JsonIgnore]
    public string? EffectiveEmailAddress => EmailAddress ?? EmailAddressSnakeCase;
}