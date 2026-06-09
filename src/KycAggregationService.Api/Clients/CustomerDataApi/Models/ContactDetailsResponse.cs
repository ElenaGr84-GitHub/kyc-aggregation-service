using System.Text.Json.Serialization;

namespace KycAggregationService.Api.Clients.CustomerDataApi.Models;

public class ContactDetailsResponse
{
    [JsonPropertyName("addresses")]
    public List<AddressResponse>? Addresses { get; set; }

    [JsonPropertyName("address")]
    public List<AddressResponse>? Address { get; set; }

    [JsonPropertyName("emails")]
    public List<EmailResponse>? Emails { get; set; }

    [JsonPropertyName("phoneNumbers")]
    public List<PhoneNumberResponse>? PhoneNumbers { get; set; }

    [JsonPropertyName("phone_numbers")]
    public List<PhoneNumberResponse>? PhoneNumbersSnakeCase { get; set; }

    [JsonIgnore]
    public IReadOnlyList<AddressResponse> EffectiveAddresses => Addresses ?? Address ?? [];

    [JsonIgnore]
    public IReadOnlyList<EmailResponse> EffectiveEmails => Emails ?? [];

    [JsonIgnore]
    public IReadOnlyList<PhoneNumberResponse> EffectivePhoneNumbers => PhoneNumbers ?? PhoneNumbersSnakeCase ?? [];
}