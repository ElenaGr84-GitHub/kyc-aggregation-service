using System.Text.RegularExpressions;
using KycAggregationService.Api.Models;
using KycAggregationService.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace KycAggregationService.Api.Controllers;

[ApiController]
[Route("kyc-data")]
public class KycDataController(IKycAggregationService kycAggregationService) : ControllerBase
{
    private static readonly Regex SsnFormatRegex = new(@"^\d{8}-\d{4}$", RegexOptions.Compiled);

    [HttpGet("{ssn}")]
    [ProducesResponseType(typeof(AggregatedKycDataResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AggregatedKycDataResponse>> GetAggregatedKycData(string ssn, CancellationToken cancellationToken)
    {
        if (!SsnFormatRegex.IsMatch(ssn))
        {
            return BadRequest(new ErrorResponse("Invalid SSN format. Expected format is yyyyMMdd-xxxx."));
        }

        var response = await kycAggregationService.GetAggregatedKycDataAsync(ssn, cancellationToken);

        if (response is null)
        {
            return NotFound(new ErrorResponse("Customer data not found for the provided SSN."));
        }

        return Ok(response);
    }
}