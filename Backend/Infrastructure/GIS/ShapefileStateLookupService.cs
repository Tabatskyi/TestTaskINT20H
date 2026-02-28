using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.IO.Esri;

namespace TestTaskINT20H.Infrastructure.GIS;

/// <summary>
/// Service for checking whether a point falls within New York State
/// using the State_Shoreline shapefile boundary.
/// </summary>
public sealed class ShapefileStateLookupService : IDisposable
{
    private IPreparedGeometry? _stateBoundary;
    private bool _isLoaded;

    public void LoadShapefile(string shapefilePath)
    {
        if (_isLoaded)
            return;

        var features = Shapefile.ReadAllFeatures(shapefilePath);

        // The state shoreline file should contain a single feature (or few) for NY State.
        // Combine all geometries into one for containment checks.
        Geometry? combined = null;
        foreach (var feature in features)
        {
            var geometry = feature.Geometry;
            combined = combined is null ? geometry : combined.Union(geometry);
        }

        if (combined is not null)
            _stateBoundary = PreparedGeometryFactory.Prepare(combined);

        _isLoaded = true;
    }

    /// <summary>
    /// Checks if the given WGS84 point is within New York State.
    /// </summary>
    public bool IsInNewYorkState(Point point)
    {
        if (!_isLoaded)
            throw new InvalidOperationException("Shapefile has not been loaded. Call LoadShapefile first.");

        return _stateBoundary?.Contains(point) ?? false;
    }

    /// <summary>
    /// Checks if the given coordinates are within New York State.
    /// </summary>
    public bool IsInNewYorkState(double latitude, double longitude)
    {
        var factory = new GeometryFactory(new PrecisionModel(), 4326);
        return IsInNewYorkState(factory.CreatePoint(new Coordinate(longitude, latitude)));
    }

    public void Dispose()
    {
        _stateBoundary = null;
    }
}
