using NetTopologySuite.Geometries;

namespace TestTaskINT20H.Domain.Orders.ValueObjects;

/// <summary>
/// Represents a geographic location with latitude and longitude.
/// This is an immutable value object.
/// </summary>
public sealed record Location
{
    private static readonly GeometryFactory GeometryFactory = new(new PrecisionModel(), 4326);

    public double Latitude { get; init; }
    public double Longitude { get; init; }

    /// <summary>
    /// WGS84 point for use with PostGIS / NetTopologySuite spatial operations.
    /// X = Longitude, Y = Latitude (NTS convention).
    /// </summary>
    public Point Point => GeometryFactory.CreatePoint(new Coordinate(Longitude, Latitude));

    public Location(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentException("Latitude must be between -90 and 90 degrees.", nameof(latitude));

        if (longitude < -180 || longitude > 180)
            throw new ArgumentException("Longitude must be between -180 and 180 degrees.", nameof(longitude));

        Latitude = latitude;
        Longitude = longitude;
    }

    public bool IsInNewYorkState()
    {
        const double minLat = 40.4961;
        const double maxLat = 45.0159;
        const double minLon = -79.7624;
        const double maxLon = -71.8562;

        return Latitude >= minLat && Latitude <= maxLat &&
               Longitude >= minLon && Longitude <= maxLon;
    }
}
