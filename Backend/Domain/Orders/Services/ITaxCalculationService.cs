using TestTaskINT20H.Domain.Orders.ValueObjects;

namespace TestTaskINT20H.Domain.Orders.Services;

/// <summary>
/// Domain service interface for tax calculation logic.
/// </summary>
public interface ITaxCalculationService
{
    TaxCalculation CalculateTax(Location location, Money subtotal);
}