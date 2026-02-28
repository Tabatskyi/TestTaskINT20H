using TestTaskINT20H.Domain.Orders.ValueObjects;
namespace TestTaskINT20H.Domain.Orders.Services;

/// <summary>
/// Domain service interface for tax calculation logic.
/// </summary>
public interface ITaxCalculationService
{
    TaxCalculation CalculateTax(Location location, Money subtotal);

    /// <summary>Returns the tax jurisdictions (with rates) that apply to <paramref name="location"/>.</summary>
    IReadOnlyList<JurisdictionInfo> GetJurisdictions(Location location);

    /// <summary>Returns every possible jurisdiction (with rates) across all of New York State.</summary>
    IReadOnlyList<JurisdictionInfo> GetAllJurisdictions();
}