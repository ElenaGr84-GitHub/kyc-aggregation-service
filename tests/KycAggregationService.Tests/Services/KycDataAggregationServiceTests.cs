using KycAggregationService.Api.Clients.CustomerDataApi;
using KycAggregationService.Api.Clients.CustomerDataApi.Models;
using KycAggregationService.Api.Models;
using KycAggregationService.Api.Persistence;
using KycAggregationService.Api.Persistence.Entities;
using KycAggregationService.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace KycAggregationService.Tests.Services;

public class KycDataAggregationServiceTests
{
    private const string ValidSsn = "19800115-1234";
    private const string FirstName = "Erik";
    private const string LastName = "Johansson";
    private const string Address = "Storgatan 1, 111 22 Stockholm";
    private const string PhoneNumber = "070-123 45 67";
    private const string Email = "erik.johansson@example.se";
    private const string TaxCountry = "SE";
    private const int Income = 550000;

    [Fact]
    public async Task ReturnsCachedData_WhenFoundInDatabase()
    {
        await using var database = await CreateDatabaseAsync();
        database.Context.AggregatedKycData.Add(CreateEntity());
        await database.Context.SaveChangesAsync();
        var fakeClient = new FakeCustomerDataApiClient();
        var service = CreateService(database.Context, fakeClient);

        var result = await service.GetAggregatedKycDataAsync(ValidSsn);

        Assert.NotNull(result);
        AssertFullResponse(result);
        AssertApiCallCount(fakeClient, expectedCallCount: 0);
    }

    [Fact]
    public async Task ReturnsAndSavesData_WhenNotCached()
    {
        await using var database = await CreateDatabaseAsync();
        var fakeClient = CreateFakeClient();
        var service = CreateService(database.Context, fakeClient);

        var result = await service.GetAggregatedKycDataAsync(ValidSsn);

        Assert.NotNull(result);
        AssertFullResponse(result);
        var persistedEntity = await database.Context.AggregatedKycData.SingleOrDefaultAsync(x => x.Ssn == ValidSsn);
        Assert.NotNull(persistedEntity);
        AssertFullEntity(persistedEntity);
        AssertApiCallCount(fakeClient, expectedCallCount: 1);
    }

    [Theory]
    [InlineData("personal-details")]
    [InlineData("contact-details")]
    [InlineData("kyc-form")]
    public async Task ReturnsNull_WhenApiResponseIsMissing(string missingApiResponse)
    {
        await using var database = await CreateDatabaseAsync();
        var fakeClient = CreateFakeClient();
        switch (missingApiResponse)
        {
            case "personal-details":
                fakeClient.PersonalDetails = null;
                break;

            case "contact-details":
                fakeClient.ContactDetails = null;
                break;

            case "kyc-form":
                fakeClient.KycForm = null;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(missingApiResponse), missingApiResponse, null);
        }
        var service = CreateService(database.Context, fakeClient);

        var result = await service.GetAggregatedKycDataAsync(ValidSsn);

        Assert.Null(result);
        await AssertNoDataWasPersistedAsync(database.Context);
        AssertApiCallCount(fakeClient, expectedCallCount: 1);
    }

    [Theory]
    [InlineData("first_name")]
    [InlineData("last_name")]
    [InlineData("address")]
    [InlineData("tax_country")]
    public async Task ReturnsNull_WhenRequiredFieldIsMissing(string missingProperty)
    {
        await using var database = await CreateDatabaseAsync();
        var fakeClient = CreateFakeClient();
        switch (missingProperty)
        {
            case "first_name":
                fakeClient.PersonalDetails!.FirstName = null;
                break;

            case "last_name":
                fakeClient.PersonalDetails!.SurName = null;
                break;

            case "address":
                fakeClient.ContactDetails!.Addresses = [];
                break;

            case "tax_country":
                var address = Assert.Single(fakeClient.ContactDetails!.EffectiveAddresses);
                address.Country = null;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(missingProperty), missingProperty, null);
        }
        var service = CreateService(database.Context, fakeClient);

        var result = await service.GetAggregatedKycDataAsync(ValidSsn);

        Assert.Null(result);
        await AssertNoDataWasPersistedAsync(database.Context);
        AssertApiCallCount(fakeClient, expectedCallCount: 1);
    }

    [Fact]
    public async Task ReturnsData_WhenOptionalFieldsAreMissing()
    {
        await using var database = await CreateDatabaseAsync();
        var fakeClient = CreateFakeClient();
        fakeClient.ContactDetails!.Emails = [];
        fakeClient.ContactDetails.PhoneNumbers = [];
        fakeClient.KycForm!.Items = [];
        var service = CreateService(database.Context, fakeClient);

        var result = await service.GetAggregatedKycDataAsync(ValidSsn);

        Assert.NotNull(result);
        AssertResponseWithoutOptionalFields(result);
        var persistedEntity = await database.Context.AggregatedKycData.SingleOrDefaultAsync(x => x.Ssn == ValidSsn);
        Assert.NotNull(persistedEntity);
        AssertEntityWithoutOptionalFields(persistedEntity);
        AssertApiCallCount(fakeClient, expectedCallCount: 1);
    }

    [Fact]
    public async Task UsesTaxCountryFromKycForm_WhenAvailable()
    {
        await using var database = await CreateDatabaseAsync();
        var fakeClient = CreateFakeClient();
        fakeClient.KycForm!.Items.Add(new KycFormItemResponse
        {
            Key = "tax_country",
            Value = "DK"
        });
        var service = CreateService(database.Context, fakeClient);

        var result = await service.GetAggregatedKycDataAsync(ValidSsn);

        Assert.NotNull(result);
        Assert.Equal("DK", result.TaxCountry);
        var persistedEntity = await database.Context.AggregatedKycData.SingleOrDefaultAsync(x => x.Ssn == ValidSsn);
        Assert.NotNull(persistedEntity);
        Assert.Equal("DK", persistedEntity.TaxCountry);
        AssertApiCallCount(fakeClient, expectedCallCount: 1);
    }

    [Fact]
    public async Task ReturnsDataWithNullIncome_WhenIncomeIsInvalid()
    {
        await using var database = await CreateDatabaseAsync();
        var fakeClient = CreateFakeClient();
        fakeClient.KycForm!.Items =
        [
            new KycFormItemResponse
            {
                Key = "annual_income",
                Value = "not-a-number"
            }
        ];
        var service = CreateService(database.Context, fakeClient);

        var result = await service.GetAggregatedKycDataAsync(ValidSsn);

        Assert.NotNull(result);
        Assert.Null(result.Income);
        AssertApiCallCount(fakeClient, expectedCallCount: 1);
    }

    [Fact]
    public async Task UsesPreferredContactInfo_WhenMultipleValuesExist()
    {
        await using var database = await CreateDatabaseAsync();
        var fakeClient = CreateFakeClient();

        const string preferredEmail = "preferred.email@example.se";
        const string preferredPhoneNumber = "070-999 99 99";

        fakeClient.ContactDetails!.Emails =
        [
            new EmailResponse
            {
                Preferred = false,
                EmailAddress = "other.email@example.se"
            },
            new EmailResponse
            {
                Preferred = true,
                EmailAddress = preferredEmail
            }
        ];

        fakeClient.ContactDetails.PhoneNumbers =
        [
            new PhoneNumberResponse
            {
                Preferred = false,
                Number = "070-111 11 11"
            },
            new PhoneNumberResponse
            {
                Preferred = true,
                Number = preferredPhoneNumber
            }
        ];

        var service = CreateService(database.Context, fakeClient);

        var result = await service.GetAggregatedKycDataAsync(ValidSsn);

        Assert.NotNull(result);
        Assert.Equal(preferredEmail, result.Email);
        Assert.Equal(preferredPhoneNumber, result.PhoneNumber);

        var persistedEntity = await database.Context.AggregatedKycData.SingleOrDefaultAsync(x => x.Ssn == ValidSsn);
        Assert.NotNull(persistedEntity);
        Assert.Equal(preferredEmail, persistedEntity.Email);
        Assert.Equal(preferredPhoneNumber, persistedEntity.PhoneNumber);

        AssertApiCallCount(fakeClient, expectedCallCount: 1);
    }

    [Fact]
    public async Task ReturnsCachedData_OnSecondRequest()
    {
        await using var database = await CreateDatabaseAsync();
        var fakeClient = CreateFakeClient();
        var service = CreateService(database.Context, fakeClient);

        var firstResult = await service.GetAggregatedKycDataAsync(ValidSsn);
        var secondResult = await service.GetAggregatedKycDataAsync(ValidSsn);

        Assert.NotNull(firstResult);
        Assert.NotNull(secondResult);
        AssertFullResponse(secondResult);
        AssertApiCallCount(fakeClient, expectedCallCount: 1);
    }

    private static KycDataAggregationService CreateService(KycAggregationDbContext dbContext, FakeCustomerDataApiClient fakeClient)
    {
        return new KycDataAggregationService(dbContext, fakeClient, NullLogger<KycDataAggregationService>.Instance);
    }

    private static AggregatedKycDataEntity CreateEntity()
    {
        return new AggregatedKycDataEntity
        {
            Ssn = ValidSsn,
            FirstName = FirstName,
            LastName = LastName,
            Address = Address,
            PhoneNumber = PhoneNumber,
            Email = Email,
            TaxCountry = TaxCountry,
            Income = Income
        };
    }

    private static FakeCustomerDataApiClient CreateFakeClient()
    {
        return new FakeCustomerDataApiClient
        {
            PersonalDetails = new PersonalDetailsResponse
            {
                FirstName = FirstName,
                SurName = LastName
            },
            ContactDetails = new ContactDetailsResponse
            {
                Addresses =
                [
                    new AddressResponse
                    {
                        Street = "Storgatan 1",
                        PostalCode = "111 22",
                        City = "Stockholm",
                        Country = "Sweden"
                    }
                ],
                Emails =
                [
                    new EmailResponse
                    {
                        Preferred = true,
                        EmailAddress = Email
                    }
                ],
                PhoneNumbers =
                [
                    new PhoneNumberResponse
                    {
                        Preferred = true,
                        Number = PhoneNumber
                    }
                ]
            },
            KycForm = new KycFormResponse
            {
                Items =
                [
                    new KycFormItemResponse
                    {
                        Key = "annual_income",
                        Value = Income.ToString()
                    }
                ]
            }
        };
    }

    private static void AssertFullResponse(AggregatedKycDataResponse result)
    {
        Assert.Equal(ValidSsn, result.Ssn);
        Assert.Equal(FirstName, result.FirstName);
        Assert.Equal(LastName, result.LastName);
        Assert.Equal(Address, result.Address);
        Assert.Equal(PhoneNumber, result.PhoneNumber);
        Assert.Equal(Email, result.Email);
        Assert.Equal(TaxCountry, result.TaxCountry);
        Assert.Equal(Income, result.Income);
    }

    private static void AssertResponseWithoutOptionalFields(AggregatedKycDataResponse result)
    {
        Assert.Equal(ValidSsn, result.Ssn);
        Assert.Equal(FirstName, result.FirstName);
        Assert.Equal(LastName, result.LastName);
        Assert.Equal(Address, result.Address);
        Assert.Equal(TaxCountry, result.TaxCountry);
        Assert.Null(result.PhoneNumber);
        Assert.Null(result.Email);
        Assert.Null(result.Income);
    }

    private static void AssertFullEntity(AggregatedKycDataEntity entity)
    {
        Assert.Equal(ValidSsn, entity.Ssn);
        Assert.Equal(FirstName, entity.FirstName);
        Assert.Equal(LastName, entity.LastName);
        Assert.Equal(Address, entity.Address);
        Assert.Equal(PhoneNumber, entity.PhoneNumber);
        Assert.Equal(Email, entity.Email);
        Assert.Equal(TaxCountry, entity.TaxCountry);
        Assert.Equal(Income, entity.Income);
    }

    private static void AssertEntityWithoutOptionalFields(AggregatedKycDataEntity entity)
    {
        Assert.Equal(ValidSsn, entity.Ssn);
        Assert.Equal(FirstName, entity.FirstName);
        Assert.Equal(LastName, entity.LastName);
        Assert.Equal(Address, entity.Address);
        Assert.Equal(TaxCountry, entity.TaxCountry);
        Assert.Null(entity.PhoneNumber);
        Assert.Null(entity.Email);
        Assert.Null(entity.Income);
    }

    private static void AssertApiCallCount(FakeCustomerDataApiClient fakeClient, int expectedCallCount)
    {
        Assert.Equal(expectedCallCount, fakeClient.PersonalDetailsCallCount);
        Assert.Equal(expectedCallCount, fakeClient.ContactDetailsCallCount);
        Assert.Equal(expectedCallCount, fakeClient.KycFormCallCount);
    }

    private static async Task AssertNoDataWasPersistedAsync(KycAggregationDbContext dbContext)
    {
        Assert.False(await dbContext.AggregatedKycData.AnyAsync());
    }

    private static async Task<TestDatabase> CreateDatabaseAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<KycAggregationDbContext>().UseSqlite(connection).Options;

        var context = new KycAggregationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        return new TestDatabase(connection, context);
    }

    private sealed class TestDatabase(SqliteConnection connection, KycAggregationDbContext context) : IAsyncDisposable
    {
        public KycAggregationDbContext Context { get; } = context;

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    private sealed class FakeCustomerDataApiClient : ICustomerDataApiClient
    {
        public int PersonalDetailsCallCount { get; private set; }
        public int ContactDetailsCallCount { get; private set; }
        public int KycFormCallCount { get; private set; }

        public PersonalDetailsResponse? PersonalDetails { get; set; }
        public ContactDetailsResponse? ContactDetails { get; set; }
        public KycFormResponse? KycForm { get; set; }

        public Task<PersonalDetailsResponse?> GetPersonalDetailsAsync(string ssn, CancellationToken cancellationToken = default)
        {
            PersonalDetailsCallCount++;
            return Task.FromResult(PersonalDetails);
        }

        public Task<ContactDetailsResponse?> GetContactDetailsAsync(string ssn, CancellationToken cancellationToken = default)
        {
            ContactDetailsCallCount++;
            return Task.FromResult(ContactDetails);
        }

        public Task<KycFormResponse?> GetKycFormAsync(string ssn, DateOnly asOfDate, CancellationToken cancellationToken = default)
        {
            KycFormCallCount++;
            return Task.FromResult(KycForm);
        }
    }
}