using System.Text.Json.Serialization;

namespace KycAggregationService.Api.Clients.CustomerDataApi.Models;

public class EmailResponse
{
    [JsonPropertyName("preferred")]
    public bool Preferred { get; set; }

    [JsonPropertyName("email_address")]
    public string? EmailAddress { get; set; }
}