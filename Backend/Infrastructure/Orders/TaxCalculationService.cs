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
    private const string MCTDJurisdictionName = "MCTD";

    private readonly ShapefileCountyLookupService _countyLookup;
    private readonly ShapefileCityLookupService _cityLookup;

    // Tax rates by county name (rates as of 2026)
    private static readonly Dictionary<string, CountyTaxInfo> CountyTaxRates = new(StringComparer.OrdinalIgnoreCase)
    {
        // NYC boroughs (combined county + city + MCTD rate)
        ["New York"] = new("New York City", 0.0m, 0.045m, 0.00375m),
        ["Kings"] = new("New York City", 0.0m, 0.045m, 0.00375m),
        ["Queens"] = new("New York City", 0.0m, 0.045m, 0.00375m),
        ["Bronx"] = new("New York City", 0.0m, 0.045m, 0.00375m),
        ["Richmond"] = new("New York City", 0.0m, 0.045m, 0.00375m),

        // Long Island
        ["Nassau"] = new(null, 0.0425m, 0.0m, 0.00375m),
        ["Suffolk"] = new(null, 0.04375m, 0.0m, 0.00375m),

        // Westchester (MCTD district)
        ["Westchester"] = new(null, 0.04m, 0.0m, 0.00375m),

        // Other counties with specific rates
        ["Erie"] = new(null, 0.0475m, 0.0m, 0.0m),
        ["Monroe"] = new(null, 0.04m, 0.0m, 0.0m),
        ["Onondaga"] = new(null, 0.04m, 0.0m, 0.0m),
        ["Albany"] = new(null, 0.04m, 0.0m, 0.0m),
        ["Dutchess"] = new(null, 0.0375m, 0.0m, 0.00375m),
        ["Orange"] = new(null, 0.0375m, 0.0m, 0.00375m),
        ["Putnam"] = new(null, 0.04m, 0.0m, 0.00375m),
        ["Rockland"] = new(null, 0.04m, 0.0m, 0.00375m),
    };

    // Cities with special tax rates (rates as of 2026)
    private static readonly Dictionary<string, CityTaxInfo> SpecialCityRates = new(StringComparer.OrdinalIgnoreCase)
    {
        // Westchester County (MCTD transit district applies: 0.375%)
        ["Mount Vernon"] = new("Mount Vernon", "Westchester", 0.04m, 0.00375m),
        ["New Rochelle"] = new("New Rochelle", "Westchester", 0.04m, 0.00375m),
        ["White Plains"] = new("White Plains", "Westchester", 0.04m, 0.00375m),
        ["Yonkers"] = new("Yonkers", "Westchester", 0.045m, 0.00375m),

        // Oneida County
        ["Rome"] = new("Rome", "Oneida", 0.0475m, 0.0m),
        ["Utica"] = new("Utica", "Oneida", 0.0475m, 0.0m),

        // Fulton County
        ["Gloversville"] = new("Gloversville", "Fulton", 0.04m, 0.0m),
        ["Johnstown"] = new("Johnstown", "Fulton", 0.04m, 0.0m),

        // Cattaraugus County
        ["Olean"] = new("Olean", "Cattaraugus", 0.04m, 0.0m),
        ["Salamanca"] = new("Salamanca", "Cattaraugus", 0.04m, 0.0m),

        // Other Independent Cities (Single city per county)
        ["Auburn"] = new("Auburn", "Cayuga", 0.04m, 0.0m),
        ["Glens Falls"] = new("Glens Falls", "Warren", 0.03m, 0.0m),
        ["Ithaca"] = new("Ithaca", "Tompkins", 0.04m, 0.0m),
        ["Norwich"] = new("Norwich", "Chenango", 0.04m, 0.0m),
        ["Oneida"] = new("Oneida", "Madison", 0.04m, 0.0m),
        ["Oswego"] = new("Oswego", "Oswego", 0.04m, 0.0m),
        ["Saratoga Springs"] = new("Saratoga Springs", "Saratoga", 0.03m, 0.0m),
    };

    public TaxCalculationService(ShapefileCountyLookupService countyLookup, ShapefileCityLookupService cityLookup)
    {
        _countyLookup = countyLookup ?? throw new ArgumentNullException(nameof(countyLookup));
        _cityLookup = cityLookup ?? throw new ArgumentNullException(nameof(cityLookup));
    }

    public TaxCalculation CalculateTax(Location location, Money subtotal)
    {
        var countyInfo = _countyLookup.FindCounty(location.Point);

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
        var countyRate = specialCity is not null ? 0m : taxInfo.CountyRate;
        var cityRate = specialCity?.CityRate ?? taxInfo.CityRate;
        var specialRates = specialCity?.SpecialRates ?? taxInfo.SpecialRates;

        var breakdown = new TaxBreakdown(
            NYStateRate,
            countyRate,
            cityRate,
            specialRates
        );

        var taxAmount = new Money(subtotal.Amount * breakdown.CompositeRate, subtotal.Currency);

        var jurisdictions = BuildJurisdictionList(countyInfo, taxInfo, specialCity, specialRates);

        return new TaxCalculation(breakdown, taxAmount, jurisdictions.AsReadOnly());
    }

    private CityTaxInfo? FindSpecialCity(Location location, string countyName)
    {
        var cityInfo = _cityLookup.FindCity(location.Point);
        if (cityInfo is null)
            return null;

        if (!SpecialCityRates.TryGetValue(cityInfo.Name, out var taxInfo))
            return null;

        if (!string.Equals(taxInfo.CountyName, countyName, StringComparison.OrdinalIgnoreCase))
            return null;

        return taxInfo;
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
        CityTaxInfo? specialCity,
        decimal specialRates)
    {
        var jurisdictions = new List<string> { "New York State" };

        var countyRate = specialCity is not null ? 0m : taxInfo.CountyRate;

        if (taxInfo.CityGroupName is not null && taxInfo.CityRate > 0)
            jurisdictions.Add(taxInfo.CityGroupName);

        if (countyRate > 0)
            jurisdictions.Add(countyInfo.FullName);

        if (specialCity is not null)
            jurisdictions.Add(specialCity.CityName);

        if (specialRates > 0)
            jurisdictions.Add(MCTDJurisdictionName);

        return jurisdictions;
    }

    private sealed record CountyTaxInfo(string? CityGroupName, decimal CountyRate, decimal CityRate, decimal SpecialRates);

    private sealed record CityTaxInfo(string CityName, string CountyName, decimal CityRate, decimal SpecialRates);

    public IReadOnlyList<JurisdictionInfo> GetJurisdictions(Location location)
    {
        var countyInfo = _countyLookup.FindCounty(location.Point);

        if (countyInfo is null)
            return [];

        var specialCity = FindSpecialCity(location, countyInfo.Name);
        var taxInfo = GetCountyTaxInfo(countyInfo.Name);

        var result = new List<JurisdictionInfo>
        {
            new("New York State", "state", NYStateRate)
        };

        var specialRates = specialCity?.SpecialRates ?? taxInfo.SpecialRates;

        if (taxInfo.CityGroupName is not null)
            result.Add(new(taxInfo.CityGroupName, "city_group", taxInfo.CityRate));

        result.Add(new(countyInfo.FullName, "county", specialCity is not null ? 0m : taxInfo.CountyRate));

        if (specialCity is not null)
            result.Add(new(specialCity.CityName, "city", specialCity.CityRate));

        if (specialRates > 0)
            result.Add(new(MCTDJurisdictionName, "special", specialRates));

        return result.AsReadOnly();
    }

    public IReadOnlyList<JurisdictionInfo> GetAllJurisdictions()
    {
        var result = new List<JurisdictionInfo>
        {
            new("New York State", "state", NYStateRate)
        };

        foreach (var countyFullName in _countyLookup.GetAllCountyNames().OrderBy(n => n))
        {
            var baseName = countyFullName.EndsWith(" County", StringComparison.OrdinalIgnoreCase)
                ? countyFullName[..^7]
                : countyFullName;
            result.Add(new(countyFullName, "county", GetCountyTaxInfo(baseName).CountyRate));
        }

        var cityGroups = CountyTaxRates.Values
            .Where(t => t.CityGroupName is not null)
            .GroupBy(t => t.CityGroupName!, StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g.Key);

        foreach (var group in cityGroups)
            result.Add(new(group.Key, "city_group", group.First().CityRate));

        foreach (var city in SpecialCityRates.Values.OrderBy(c => c.CityName))
            result.Add(new(city.CityName, "city", city.CityRate));

        return result.AsReadOnly();
    }
}