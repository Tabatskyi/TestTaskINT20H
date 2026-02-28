using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.IO.Esri;
using TestTaskINT20H.Domain.Orders.Services;

namespace TestTaskINT20H.Infrastructure.GIS;

/// <summary>
/// Service for looking up incorporated city/place boundaries from a NY State places shapefile.
/// Uses STRtree spatial index and PreparedGeometry for fast point-in-polygon lookups.
/// Compatible with Census Bureau TIGER/Line place files (e.g. tl_YYYY_36_place.shp).
/// </summary>
public sealed class ShapefileCityLookupService : ICityLookupService, IDisposable
{
    private readonly List<CityFeature> _cities = [];
    private readonly STRtree<CityFeature> _spatialIndex = new();
    private bool _isLoaded;

    public void LoadShapefile(string shapefilePath)
    {
        if (_isLoaded)
            return;

        foreach (var feature in Shapefile.ReadAllFeatures(shapefilePath))
        {
            var name = feature.Attributes["NAME"]?.ToString();
            if (name is null)
                continue;

            var geometry = feature.Geometry;
            var cityFeature = new CityFeature
            {
                Name = name,
                Geometry = geometry,
                PreparedGeometry = PreparedGeometryFactory.Prepare(geometry)
            };

            _cities.Add(cityFeature);
            _spatialIndex.Insert(geometry.EnvelopeInternal, cityFeature);
        }

        _spatialIndex.Build();
        _isLoaded = true;
    }

    /// <summary>
    /// Finds the city/place containing the given WGS84 point.
    /// Uses spatial index for O(log n) bounding box lookup, then precise containment test.
    /// </summary>
    /// <param name="point">WGS84 point (X = Longitude, Y = Latitude)</param>
    /// <returns>City information if found, null otherwise</returns>
    public CityResult? FindCity(Point point)
    {
        if (!_isLoaded)
            throw new InvalidOperationException("Shapefile has not been loaded. Call LoadShapefile first.");

        var candidates = _spatialIndex.Query(point.EnvelopeInternal);

        foreach (var city in candidates)
        {
            if (city.PreparedGeometry.Contains(point))
                return new CityResult(city.Name);
        }

        return null;
    }

    public int LoadedCityCount => _cities.Count;

    public void Dispose() => _cities.Clear();

    private sealed class CityFeature
    {
        public required string Name { get; init; }
        public required Geometry Geometry { get; init; }
        public required IPreparedGeometry PreparedGeometry { get; init; }
    }
}
