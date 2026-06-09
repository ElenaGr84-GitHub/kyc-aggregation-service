using KycAggregationService.Api.Models;
using KycAggregationService.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace KycAggregationService.Api.Controllers;

[ApiController]
[Route("kyc-data")]
public class KycDataController(IKycAggregationService kycAggregationService) : ControllerBase
{
    [HttpGet("{ssn}")]
    public async Task<ActionResult<AggregatedKycDataResponse>> GetAggregatedKycData(string ssn, CancellationToken cancellationToken)
    {
        var response = await kycAggregationService.GetAggregatedKycDataAsync(ssn, cancellationToken);

        if (response is null)
        {
            return NotFound(new
            {
                error = "Customer data not found for the provided SSN."
            });
        }

        return Ok(response);
    }
}