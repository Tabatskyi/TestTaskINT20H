using NetTopologySuite.Geometries;

namespace TestTaskINT20H.Domain.Orders.Services;

/// <summary>
/// Abstraction for geographic county lookup.
/// </summary>
public interface ICountyLookupService
{
    CountyResult? FindCounty(Point point);
    IReadOnlyList<string> GetAllCountyNames();
}

/// <summary>
/// County information returned from a geographic lookup.
/// </summary>
public sealed record CountyResult(string Name, string FullName, string CountyFips);
