using Microsoft.AspNetCore.Mvc;
using KycAggregationService.Api.Models;

namespace KycAggregationService.Api.Controllers;

[ApiController]
[Route("kyc-data")]
public class KycDataController : ControllerBase
{
    [HttpGet("{ssn}")]
    public ActionResult<AggregatedKycDataResponse> GetAggregatedKycData(string ssn)
    {
        var response = new AggregatedKycDataResponse
        {
            Ssn = ssn,
            FirstName = "Lars",
            LastName = "Larsson",
            Address = "Smågatan 1, 123 22 Malmö",
            PhoneNumber = "+46 70 123 45 67",
            Email = "lars.larsson@example.com",
            TaxCountry = "SE",
            Income = 550000
        };

        return Ok(response);
    }
}