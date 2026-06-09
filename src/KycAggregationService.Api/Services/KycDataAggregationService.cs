using System.Globalization;
using KycAggregationService.Api.Clients.CustomerDataApi;
using KycAggregationService.Api.Clients.CustomerDataApi.Models;
using KycAggregationService.Api.Models;
using KycAggregationService.Api.Persistence;
using KycAggregationService.Api.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace KycAggregationService.Api.Services;

public class KycDataAggregationService(KycAggregationDbContext dbContext, ICustomerDataApiClient customerDataApiClient) : IKycAggregationService
{
    public async Task<AggregatedKycDataResponse?> GetAggregatedKycDataAsync(string ssn, CancellationToken cancellationToken = default)
    {
        var existingData = await dbContext.AggregatedKycData.AsNoTracking().SingleOrDefaultAsync(x => x.Ssn == ssn, cancellationToken);

        if (existingData is not null)
        {
            return MapToResponse(existingData);
        }

        var asOfDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var personalDetailsTask = customerDataApiClient.GetPersonalDetailsAsync(ssn, cancellationToken);
        var contactDetailsTask = customerDataApiClient.GetContactDetailsAsync(ssn, cancellationToken);
        var kycFormTask = customerDataApiClient.GetKycFormAsync(ssn, asOfDate, cancellationToken);

        await Task.WhenAll(personalDetailsTask, contactDetailsTask, kycFormTask);

        var personalDetails = await personalDetailsTask;
        var contactDetails = await contactDetailsTask;
        var kycForm = await kycFormTask;

        if (personalDetails is null || contactDetails is null || kycForm is null)
        {
            return null;
        }

        var firstName = personalDetails.EffectiveFirstName?.Trim();
        var lastName = personalDetails.EffectiveSurName?.Trim();
        var address = FormatAddress(contactDetails.EffectiveAddresses);
        var taxCountry = GetKycValue(kycForm, "tax_country")?.Trim() ?? GetCountryCode(contactDetails.EffectiveAddresses.FirstOrDefault()?.Country);
        var income = ParseIncome(GetKycValue(kycForm, "annual_income"));
        var email = GetPreferredEmail(contactDetails);
        var phoneNumber = GetPreferredPhoneNumber(contactDetails);

        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(address) ||
            string.IsNullOrWhiteSpace(taxCountry))
        {
            return null;
        }

        var entity = new AggregatedKycDataEntity
        {
            Ssn = ssn,
            FirstName = firstName,
            LastName = lastName,
            Address = address,
            PhoneNumber = phoneNumber,
            Email = email,
            TaxCountry = taxCountry,
            Income = income
        };

        dbContext.AggregatedKycData.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(entity);
    }

    private static AggregatedKycDataResponse MapToResponse(AggregatedKycDataEntity entity)
    {
        return new AggregatedKycDataResponse
        {
            Ssn = entity.Ssn,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Address = entity.Address,
            PhoneNumber = entity.PhoneNumber,
            Email = entity.Email,
            TaxCountry = entity.TaxCountry,
            Income = entity.Income
        };
    }

    private static string? GetKycValue(KycFormResponse kycForm, string key)
    {
        return kycForm.Items.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase))?.Value;
    }

    private static int? ParseIncome(string? value)
    {
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var income))
        {
            return income;
        }

        return null;
    }

    private static string? FormatAddress(IEnumerable<AddressResponse> addresses)
    {
        var address = addresses.FirstOrDefault(x =>
            !string.IsNullOrWhiteSpace(x.Street) ||
            !string.IsNullOrWhiteSpace(x.EffectivePostalCode) ||
            !string.IsNullOrWhiteSpace(x.City));

        if (address is null)
        {
            return null;
        }

        var cityLine = string.Join(" ", new[] { address.EffectivePostalCode, address.City }.Where(x => !string.IsNullOrWhiteSpace(x)));

        return string.Join(", ", new[] { address.Street, cityLine }.Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    private static string? GetPreferredEmail(ContactDetailsResponse contactDetails)
    {
        return contactDetails.EffectiveEmails
            .OrderByDescending(x => x.Preferred)
            .Select(x => x.EffectiveEmailAddress?.Trim())
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
    }

    private static string? GetPreferredPhoneNumber(ContactDetailsResponse contactDetails)
    {
        return contactDetails.EffectivePhoneNumbers
            .OrderByDescending(x => x.Preferred)
            .Select(x => x.Number?.Trim())
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
    }

    private static string? GetCountryCode(string? country)
    {
        if (string.IsNullOrWhiteSpace(country))
        {
            return null;
        }

        var trimmedValue = country.Trim();
        if (trimmedValue.Length == 2 && trimmedValue.All(c => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z'))
        {
            return trimmedValue.ToUpperInvariant();
        }

        var normalizedValue = trimmedValue.ToLowerInvariant();
        return normalizedValue switch
        {
            "se" or "sweden" or "sverige" or "swedish" or "svensk" => "SE",
            "dk" or "denmark" or "danmark" or "danish" or "dansk" => "DK",
            "no" or "norway" or "norge" or "norwegian" or "norsk" => "NO",
            "fi" or "finland" or "finnish" or "finsk" => "FI",
            _ => null
        };
    }
}