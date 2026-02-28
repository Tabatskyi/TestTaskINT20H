using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestTaskINT20H.Application.Orders.DTOs;
using TestTaskINT20H.Domain.Orders.Services;
using TestTaskINT20H.Domain.Orders.ValueObjects;

namespace TestTaskINT20H.Presentation.Controllers;

[ApiController]
[Route("jurisdictions")]
[Produces("application/json")]
[Authorize]
public sealed class JurisdictionsController(ITaxCalculationService taxService) : ControllerBase
{
    private readonly ITaxCalculationService _taxService = taxService ?? throw new ArgumentNullException(nameof(taxService));

    /// <summary>
    /// Returns all possible tax jurisdictions in New York State,
    /// or the jurisdictions that apply to a specific location when coordinates are provided.
    /// Each entry includes the jurisdiction name, level type, and its contributing tax rate.
    /// </summary>
    /// <param name="latitude">Latitude of the location (optional, requires longitude)</param>
    /// <param name="longitude">Longitude of the location (optional, requires latitude)</param>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<JurisdictionInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult GetJurisdictions([FromQuery] double? latitude = null, [FromQuery] double? longitude = null)
    {
        if (latitude is null && longitude is null)
            return Ok(_taxService.GetAllJurisdictions());

        if (latitude is null || longitude is null)
            return BadRequest(new ErrorResponse { Error = "Both latitude and longitude must be provided." });

        try
        {
            var location = new Location(latitude.Value, longitude.Value);
            var jurisdictions = _taxService.GetJurisdictions(location);

            if (jurisdictions.Count == 0)
                return NotFound(new ErrorResponse { Error = "Location is outside New York State." });

            return Ok(jurisdictions);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
    }
}
