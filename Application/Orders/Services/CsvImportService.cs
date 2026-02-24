using TestTaskINT20H.Application.Orders.DTOs;
using System.Globalization;

namespace TestTaskINT20H.Application.Orders.Services;

/// <summary>
/// Application service for CSV import functionality.
/// </summary>
public sealed class CsvImportService
{
    public CsvParseResult ParseCsv(Stream csvStream)
    {
        var result = new CsvParseResult();
        using var reader = new StreamReader(csvStream);

        var headerLine = reader.ReadLine();
        if (string.IsNullOrEmpty(headerLine))
            return result;

        var headers = headerLine.Split(',')
            .Select(header => header.Trim().ToLowerInvariant())
            .ToArray();

        var latIndex = Array.FindIndex(headers, header => header == "latitude" || header == "lat");
        var lonIndex = Array.FindIndex(headers, header => header == "longitude" || header == "lon" || header == "lng");
        var subtotalIndex = Array.FindIndex(headers, header => header == "subtotal" || header == "amount" || header == "price");
        var timestampIndex = Array.FindIndex(headers, header => header == "timestamp" || header == "date" || header == "datetime");

        if (latIndex < 0 || lonIndex < 0 || subtotalIndex < 0)
            throw new ArgumentException("CSV must contain latitude, longitude, and subtotal columns");

        var rowNumber = 1;
        while (!reader.EndOfStream)
        {
            rowNumber++;
            var line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var values = ParseCsvLine(line);

            if (values.Length <= Math.Max(Math.Max(latIndex, lonIndex), subtotalIndex))
            {
                result.SkippedCount++;
                result.SkippedRows.Add(rowNumber);
                continue;
            }

            if (!double.TryParse(values[latIndex], CultureInfo.InvariantCulture, out var latitude))
            {
                result.SkippedCount++;
                result.SkippedRows.Add(rowNumber);
                continue;
            }

            if (!double.TryParse(values[lonIndex], CultureInfo.InvariantCulture, out var longitude))
            {
                result.SkippedCount++;
                result.SkippedRows.Add(rowNumber);
                continue;
            }

            if (!decimal.TryParse(values[subtotalIndex], CultureInfo.InvariantCulture, out var subtotal))
            {
                result.SkippedCount++;
                result.SkippedRows.Add(rowNumber);
                continue;
            }

            if (subtotal <= 0)
            {
                result.SkippedCount++;
                result.SkippedRows.Add(rowNumber);
                continue;
            }

            // Validate NY coordinates
            if (!IsValidNYCoordinates(latitude, longitude))
            {
                result.SkippedCount++;
                result.SkippedRows.Add(rowNumber);
                continue;
            }

            var order = new CreateOrderDto
            {
                Latitude = latitude,
                Longitude = longitude,
                Subtotal = subtotal
            };

            if (timestampIndex >= 0 && timestampIndex < values.Length &&
                !string.IsNullOrEmpty(values[timestampIndex]))
            {
                if (DateTime.TryParse(values[timestampIndex], CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var timestamp))
                {
                    order.Timestamp = timestamp;
                }
            }

            result.Orders.Add(order);
        }

        return result;
    }

    private static bool IsValidNYCoordinates(double latitude, double longitude)
    {
        const double minLat = 40.4961;
        const double maxLat = 45.0159;
        const double minLon = -79.7624;
        const double maxLon = -71.8562;

        return latitude >= minLat && latitude <= maxLat &&
               longitude >= minLon && longitude <= maxLon;
    }

    private static string[] ParseCsvLine(string line)
    {
        var values = new List<string>();
        var inQuotes = false;
        var valueBuilder = new System.Text.StringBuilder();

        foreach (var ch in line)
        {
            if (ch == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (ch == ',' && !inQuotes)
            {
                values.Add(valueBuilder.ToString().Trim());
                valueBuilder.Clear();
            }
            else
            {
                valueBuilder.Append(ch);
            }
        }
        values.Add(valueBuilder.ToString().Trim());

        return values.ToArray();
    }
}

public sealed class CsvParseResult
{
    public List<CreateOrderDto> Orders { get; set; } = [];
    public int SkippedCount { get; set; }
    public List<int> SkippedRows { get; set; } = [];
}