using TestTaskINT20H.Domain.Orders.Services;
using TestTaskINT20H.Domain.Orders.ValueObjects;
using TestTaskINT20H.Infrastructure.GIS;

namespace TestTaskINT20H.Infrastructure.Orders;

/// <summary>
/// Implementation of tax calculation domain service for New York State.
/// Uses shapefile-based county lookup for accurate jurisdiction determination.
/// </summary>
public sealed class TaxCalculationService : ITaxCalculationService
{
    private const decimal NYStateRate = 0.04m;
    private const decimal DefaultCountyRate = 0.04m;

    private readonly ShapefileCountyLookupService _countyLookup;

    // Tax rates by county name (rates as of 2024)
    private static readonly Dictionary<string, CountyTaxInfo> CountyTaxRates = new(StringComparer.OrdinalIgnoreCase)
    {
        // NYC boroughs (combined county + city + MCTD rate)
        ["New York"] = new("New York City", 0.045m, 0.0m, 0.00375m),
        ["Kings"] = new("New York City", 0.045m, 0.0m, 0.00375m),
        ["Queens"] = new("New York City", 0.045m, 0.0m, 0.00375m),
        ["Bronx"] = new("New York City", 0.045m, 0.0m, 0.00375m),
        ["Richmond"] = new("New York City", 0.045m, 0.0m, 0.00375m),

        // Long Island
        ["Nassau"] = new(null, 0.04625m, 0.0m, 0.0m),
        ["Suffolk"] = new(null, 0.04625m, 0.0m, 0.0m),

        // Westchester (MCTD district)
        ["Westchester"] = new(null, 0.0375m, 0.0m, 0.00375m),

        // Other counties with specific rates
        ["Erie"] = new(null, 0.04m, 0.0m, 0.0m),
        ["Monroe"] = new(null, 0.04m, 0.0m, 0.0m),
        ["Onondaga"] = new(null, 0.04m, 0.0m, 0.0m),
        ["Albany"] = new(null, 0.04m, 0.0m, 0.0m),
        ["Dutchess"] = new(null, 0.0375m, 0.0m, 0.00375m),
        ["Orange"] = new(null, 0.0375m, 0.0m, 0.00375m),
        ["Putnam"] = new(null, 0.04m, 0.0m, 0.00375m),
        ["Rockland"] = new(null, 0.04m, 0.0m, 0.00375m),
    };

    // Cities with special tax rates (checked separately)
    private static readonly CityTaxInfo[] SpecialCities =
    [
        new("Yonkers", "Westchester", 40.9126, 40.9787, -73.9075, -73.8265, 0.015m),
    ];

    public TaxCalculationService(ShapefileCountyLookupService countyLookup)
    {
        _countyLookup = countyLookup ?? throw new ArgumentNullException(nameof(countyLookup));
    }

    public TaxCalculation CalculateTax(Location location, Money subtotal)
    {
        var countyInfo = _countyLookup.FindCounty(location.Latitude, location.Longitude);

        // If location is outside NY State, return zero tax
        if (countyInfo is null)
        {
            var noTaxBreakdown = new TaxBreakdown(0.0m, 0.0m, 0.0m, 0.0m);
            var noTax = new Money(0.0m, subtotal.Currency);
            return new TaxCalculation(noTaxBreakdown, noTax, new List<string> { "Out of State" }.AsReadOnly());
        }

        // Check for special city rates first
        var specialCity = FindSpecialCity(location, countyInfo.Name);

        var taxInfo = GetCountyTaxInfo(countyInfo.Name);
        var cityRate = specialCity?.CityRate ?? taxInfo.CityRate;
        var cityName = specialCity?.CityName;

        var breakdown = new TaxBreakdown(
            NYStateRate,
            taxInfo.CountyRate,
            cityRate,
            taxInfo.SpecialRates
        );

        var taxAmount = new Money(subtotal.Amount * breakdown.CompositeRate, subtotal.Currency);

        var jurisdictions = BuildJurisdictionList(countyInfo, taxInfo, cityName);

        return new TaxCalculation(breakdown, taxAmount, jurisdictions.AsReadOnly());
    }

    private static CityTaxInfo? FindSpecialCity(Location location, string countyName)
    {
        foreach (var city in SpecialCities)
        {
            if (!string.Equals(city.CountyName, countyName, StringComparison.OrdinalIgnoreCase))
                continue;

            if (location.IsWithinBounds(city.MinLat, city.MaxLat, city.MinLon, city.MaxLon))
                return city;
        }

        return null;
    }

    private static CountyTaxInfo GetCountyTaxInfo(string countyName)
    {
        return CountyTaxRates.TryGetValue(countyName, out var info)
            ? info
            : new CountyTaxInfo(null, DefaultCountyRate, 0.0m, 0.0m);
    }

    private static List<string> BuildJurisdictionList(
        CountyInfo countyInfo,
        CountyTaxInfo taxInfo,
        string? cityName)
    {
        var jurisdictions = new List<string> { "New York State" };

        // Add the overarching city if applicable (e.g., NYC)
        if (taxInfo.CityGroupName is not null)
        {
            jurisdictions.Add(taxInfo.CityGroupName);
        }

        jurisdictions.Add(countyInfo.FullName);

        if (cityName is not null)
        {
            jurisdictions.Add(cityName);
        }

        return jurisdictions;
    }

    private sealed record CountyTaxInfo(string? CityGroupName, decimal CountyRate, decimal CityRate, decimal SpecialRates);

    private sealed record CityTaxInfo(
        string CityName,
        string CountyName,
        double MinLat,
        double MaxLat,
        double MinLon,
        double MaxLon,
        decimal CityRate);
}