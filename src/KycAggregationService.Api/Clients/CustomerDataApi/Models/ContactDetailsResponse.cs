using System.Text.Json.Serialization;

namespace KycAggregationService.Api.Clients.CustomerDataApi.Models;

public class ContactDetailsResponse
{
    [JsonPropertyName("address")]
    public List<AddressResponse> Addresses { get; set; } = [];

    [JsonPropertyName("emails")]
    public List<EmailResponse> Emails { get; set; } = [];

    [JsonPropertyName("phone_numbers")]
    public List<PhoneNumberResponse> PhoneNumbers { get; set; } = [];
}