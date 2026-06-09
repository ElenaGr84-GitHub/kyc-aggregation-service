namespace KycAggregationService.Api.Persistence.Entities;

public class AggregatedKycDataEntity
{
    public int Id { get; set; }

    public required string Ssn { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public required string Address { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public required string TaxCountry { get; set; }

    public int? Income { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}