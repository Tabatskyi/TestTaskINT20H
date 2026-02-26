using TestTaskINT20H.Domain.Orders.Services;
using TestTaskINT20H.Domain.Orders.ValueObjects;

namespace TestTaskINT20H.Infrastructure.Orders;

/// <summary>
/// Implementation of tax calculation domain service for New York State.
/// </summary>
public sealed class TaxCalculationService : ITaxCalculationService
{
    private const decimal NYStateRate = 0.04m;

    private static readonly TaxJurisdiction[] CityJurisdictions;
    private static readonly TaxJurisdiction[] CountyJurisdictions;
    private static readonly TaxJurisdiction DefaultStateJurisdiction;

    private static readonly List<TaxJurisdiction> Jurisdictions =
    [
        // New York City (5 boroughs)
        new TaxJurisdiction
        {
            Name = "New York City",
            Type = JurisdictionType.City,
            MinLat = 40.4774, MaxLat = 40.9176,
            MinLon = -74.2591, MaxLon = -73.7004,
            CountyRate = 0.045m,
            CityRate = 0.0m,
            SpecialRates = 0.00375m,
            Counties = ["New York County", "Kings County", "Queens County", "Bronx County", "Richmond County"]
        },
        // Yonkers (special city rate)
        new TaxJurisdiction
        {
            Name = "Yonkers",
            Type = JurisdictionType.City,
            MinLat = 40.9126, MaxLat = 40.9787,
            MinLon = -73.9075, MaxLon = -73.8265,
            CountyRate = 0.0375m,
            CityRate = 0.015m,
            SpecialRates = 0.0m,
            Counties = ["Westchester County"]
        },
        // Buffalo
        new TaxJurisdiction
        {
            Name = "Buffalo",
            Type = JurisdictionType.City,
            MinLat = 42.8260, MaxLat = 42.9663,
            MinLon = -78.9120, MaxLon = -78.7953,
            CountyRate = 0.04m,
            CityRate = 0.0m,
            SpecialRates = 0.0m,
            Counties = ["Erie County"]
        },
        // Rochester
        new TaxJurisdiction
        {
            Name = "Rochester",
            Type = JurisdictionType.City,
            MinLat = 43.0845, MaxLat = 43.2313,
            MinLon = -77.7012, MaxLon = -77.5055,
            CountyRate = 0.04m,
            CityRate = 0.0m,
            SpecialRates = 0.0m,
            Counties = ["Monroe County"]
        },
        // Syracuse
        new TaxJurisdiction
        {
            Name = "Syracuse",
            Type = JurisdictionType.City,
            MinLat = 42.9849, MaxLat = 43.0845,
            MinLon = -76.2040, MaxLon = -76.0743,
            CountyRate = 0.04m,
            CityRate = 0.0m,
            SpecialRates = 0.0m,
            Counties = ["Onondaga County"]
        },
        // Albany
        new TaxJurisdiction
        {
            Name = "Albany",
            Type = JurisdictionType.City,
            MinLat = 42.6145, MaxLat = 42.7085,
            MinLon = -73.8170, MaxLon = -73.7236,
            CountyRate = 0.04m,
            CityRate = 0.0m,
            SpecialRates = 0.0m,
            Counties = ["Albany County"]
        },
        // Long Island - Nassau County
        new TaxJurisdiction
        {
            Name = "Nassau County",
            Type = JurisdictionType.County,
            MinLat = 40.5464, MaxLat = 40.9112,
            MinLon = -73.7432, MaxLon = -73.4232,
            CountyRate = 0.04625m,
            CityRate = 0.0m,
            SpecialRates = 0.0m,
            Counties = ["Nassau County"]
        },
        // Long Island - Suffolk County
        new TaxJurisdiction
        {
            Name = "Suffolk County",
            Type = JurisdictionType.County,
            MinLat = 40.6018, MaxLat = 41.1614,
            MinLon = -73.4978, MaxLon = -71.8562,
            CountyRate = 0.04625m,
            CityRate = 0.0m,
            SpecialRates = 0.0m,
            Counties = ["Suffolk County"]
        },
        // Westchester County (excluding Yonkers)
        new TaxJurisdiction
        {
            Name = "Westchester County",
            Type = JurisdictionType.County,
            MinLat = 40.8859, MaxLat = 41.3682,
            MinLon = -73.9822, MaxLon = -73.4827,
            CountyRate = 0.0375m,
            CityRate = 0.0m,
            SpecialRates = 0.0m,
            Counties = ["Westchester County"]
        },
        // Default NY State jurisdiction
        new TaxJurisdiction
        {
            Name = "New York State (Default)",
            Type = JurisdictionType.State,
            MinLat = 40.4961, MaxLat = 45.0159,
            MinLon = -79.7624, MaxLon = -71.8562,
            CountyRate = 0.04m,
            CityRate = 0.0m,
            SpecialRates = 0.0m,
            Counties = []
        }
    ];

    static TaxCalculationService()
    {
        CityJurisdictions = Jurisdictions.Where(jr => jr.Type == JurisdictionType.City).ToArray();
        CountyJurisdictions = Jurisdictions.Where(jr => jr.Type == JurisdictionType.County).ToArray();
        DefaultStateJurisdiction = Jurisdictions.First(jr => jr.Type == JurisdictionType.State);
    }

    public TaxCalculation CalculateTax(Location location, Money subtotal)
    {
        var jurisdiction = FindJurisdiction(location);

        var breakdown = new TaxBreakdown(
            NYStateRate,
            jurisdiction.CountyRate,
            jurisdiction.CityRate,
            jurisdiction.SpecialRates
        );

        var taxAmount = new Money(subtotal.Amount * breakdown.CompositeRate, subtotal.Currency);

        var jurisdictions = new List<string> { "New York State" };
        if (jurisdiction.Counties.Length != 0)
        {
            jurisdictions.AddRange(jurisdiction.Counties);
        }
        if (jurisdiction.Type == JurisdictionType.City)
        {
            jurisdictions.Add(jurisdiction.Name);
        }

        return new TaxCalculation(breakdown, taxAmount, jurisdictions.AsReadOnly());
    }

    private static TaxJurisdiction FindJurisdiction(Location location)
    {
        // First, try to find a city-level jurisdiction (most specific)
        foreach (var cityJuris in CityJurisdictions)
        {
            if (location.IsWithinBounds(cityJuris.MinLat, cityJuris.MaxLat, cityJuris.MinLon, cityJuris.MaxLon))
                return cityJuris;
        }

        // Then, try county-level
        foreach (var countyJuris in CountyJurisdictions)
        {
            if (location.IsWithinBounds(countyJuris.MinLat, countyJuris.MaxLat, countyJuris.MinLon, countyJuris.MaxLon))
                return countyJuris;
        }

        // Finally, return state default
        return DefaultStateJurisdiction;
    }

    private sealed class TaxJurisdiction
    {
        public string Name { get; init; } = string.Empty;
        public JurisdictionType Type { get; init; }
        public double MinLat { get; init; }
        public double MaxLat { get; init; }
        public double MinLon { get; init; }
        public double MaxLon { get; init; }
        public decimal CountyRate { get; init; }
        public decimal CityRate { get; init; }
        public decimal SpecialRates { get; init; }
        public string[] Counties { get; init; } = [];
    }

    private enum JurisdictionType
    {
        State,
        County,
        City
    }
}