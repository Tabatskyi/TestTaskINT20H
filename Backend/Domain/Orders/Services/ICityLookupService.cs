using NetTopologySuite.Geometries;

namespace TestTaskINT20H.Domain.Orders.Services;

/// <summary>
/// Abstraction for geographic city/place lookup.
/// </summary>
public interface ICityLookupService
{
    CityResult? FindCity(Point point);
}

/// <summary>
/// City information returned from a geographic lookup.
/// </summary>
public sealed record CityResult(string Name);
