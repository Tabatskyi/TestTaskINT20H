namespace TestTaskINT20H.Domain.Orders.ValueObjects;

/// <summary>
/// Represents the breakdown of tax rates by jurisdiction level.
/// This is an immutable value object.
/// </summary>
public sealed record TaxBreakdown
{
    public decimal StateRate { get; init; }
    public decimal CountyRate { get; init; }
    public decimal CityRate { get; init; }
    public decimal SpecialRates { get; init; }

    public TaxBreakdown(decimal stateRate, decimal countyRate, decimal cityRate, decimal specialRates)
    {
        if (stateRate < 0 || countyRate < 0 || cityRate < 0 || specialRates < 0)
            throw new ArgumentException("Tax rates cannot be negative.");

        StateRate = stateRate;
        CountyRate = countyRate;
        CityRate = cityRate;
        SpecialRates = specialRates;
    }

    public decimal CompositeRate => StateRate + CountyRate + CityRate + SpecialRates;
}
