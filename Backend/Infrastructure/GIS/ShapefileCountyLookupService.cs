using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.IO.Esri;

namespace TestTaskINT20H.Infrastructure.GIS;

/// <summary>
/// Service for looking up county information from shapefiles using NetTopologySuite.
/// Uses STRtree spatial index and PreparedGeometry for fast point-in-polygon lookups.
/// </summary>
public sealed class ShapefileCountyLookupService : IDisposable
{
    private readonly List<CountyFeature> _nyCounties = [];
    private readonly STRtree<CountyFeature> _spatialIndex = new();
    private readonly GeometryFactory _geometryFactory = new(new PrecisionModel(), 4326); // WGS84
    private bool _isLoaded;

    public void LoadShapefile(string shapefilePath)
    {
        if (_isLoaded)
            return;

        var features = Shapefile.ReadAllFeatures(shapefilePath);

        foreach (var feature in features)
        {
            var countyName = feature.Attributes["NAME"]?.ToString() ?? "Unknown";
            var countyFips = feature.Attributes["FIPS_CODE"]?.ToString() ?? "";
            var geometry = feature.Geometry;

            var countyFeature = new CountyFeature
            {
                Name = countyName,
                FullName = $"{countyName} County",
                CountyFips = countyFips,
                Geometry = geometry,
                PreparedGeometry = PreparedGeometryFactory.Prepare(geometry)
            };

            _nyCounties.Add(countyFeature);
            _spatialIndex.Insert(geometry.EnvelopeInternal, countyFeature);
        }

        _spatialIndex.Build();
        _isLoaded = true;
    }

    /// <summary>
    /// Finds the county containing the given WGS84 point.
    /// Uses spatial index for O(log n) bounding box lookup, then precise containment test.
    /// </summary>
    /// <param name="point">WGS84 point (X = Longitude, Y = Latitude)</param>
    /// <returns>County information if found, null otherwise</returns>
    public CountyInfo? FindCounty(Point point)
    {
        if (!_isLoaded)
            throw new InvalidOperationException("Shapefile has not been loaded. Call LoadShapefile first.");

        var candidates = _spatialIndex.Query(point.EnvelopeInternal);

        // Check precise containment using PreparedGeometry (optimized for repeated tests)
        foreach (var county in candidates)
        {
            if (county.PreparedGeometry.Contains(point))
            {
                return new CountyInfo
                {
                    Name = county.Name,
                    FullName = county.FullName,
                    CountyFips = county.CountyFips
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Finds the county containing the given coordinates.
    /// </summary>
    /// <param name="latitude">Latitude in WGS84</param>
    /// <param name="longitude">Longitude in WGS84</param>
    /// <returns>County information if found, null otherwise</returns>
    public CountyInfo? FindCounty(double latitude, double longitude)
        => FindCounty(_geometryFactory.CreatePoint(new Coordinate(longitude, latitude)));

    public int LoadedCountyCount => _nyCounties.Count;

    public IReadOnlyList<string> GetAllCountyNames()
        => _nyCounties.Select(c => c.FullName).ToList().AsReadOnly();

    public void Dispose()
    {
        _nyCounties.Clear();
    }

    private sealed class CountyFeature
    {
        public required string Name { get; init; }
        public required string FullName { get; init; }
        public required string CountyFips { get; init; }
        public required Geometry Geometry { get; init; }
        public required IPreparedGeometry PreparedGeometry { get; init; }
    }
}

/// <summary>
/// County information returned from shapefile lookup.
/// </summary>
public sealed class CountyInfo
{
    public required string Name { get; init; }
    public required string FullName { get; init; }
    public required string CountyFips { get; init; }
}
