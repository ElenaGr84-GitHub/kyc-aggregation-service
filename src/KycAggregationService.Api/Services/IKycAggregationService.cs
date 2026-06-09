using KycAggregationService.Api.Models;

namespace KycAggregationService.Api.Services;

public interface IKycAggregationService
{
    Task<AggregatedKycDataResponse?> GetAggregatedKycDataAsync(string ssn, CancellationToken cancellationToken = default);
}