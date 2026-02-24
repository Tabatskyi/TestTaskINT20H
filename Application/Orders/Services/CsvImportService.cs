using System.Collections.Concurrent;
using System.Globalization;
using TestTaskINT20H.Application.Orders.DTOs;

namespace TestTaskINT20H.Application.Orders.Services;

/// <summary>
/// Application service for CSV import functionality.
/// </summary>
public sealed class CsvImportService
{
    private const int ParallelThreshold = 1000;

    public CsvParseResult ParseCsv(Stream csvStream)
    {
        using var reader = new StreamReader(csvStream);

        var headerLine = reader.ReadLine();
        if (string.IsNullOrEmpty(headerLine))
            return new CsvParseResult();

        var headers = headerLine.Split(',')
            .Select(header => header.Trim().ToLowerInvariant())
            .ToArray();

        var columnIndices = new ColumnIndices
        {
            Latitude = Array.FindIndex(headers, header => header == "latitude" || header == "lat"),
            Longitude = Array.FindIndex(headers, header => header == "longitude" || header == "lon" || header == "lng"),
            Subtotal = Array.FindIndex(headers, header => header == "subtotal" || header == "amount" || header == "price"),
            Timestamp = Array.FindIndex(headers, header => header == "timestamp" || header == "date" || header == "datetime")
        };

        if (columnIndices.Latitude < 0 || columnIndices.Longitude < 0 || columnIndices.Subtotal < 0)
            throw new ArgumentException("CSV must contain latitude, longitude, and subtotal columns");

        var lines = new List<(int RowNumber, string Line)>();
        var rowNumber = 1;
        while (!reader.EndOfStream)
        {
            rowNumber++;
            var line = reader.ReadLine();
            if (!string.IsNullOrWhiteSpace(line))
            {
                lines.Add((rowNumber, line));
            }
        }

        return lines.Count >= ParallelThreshold
            ? ParseLinesParallel(lines, columnIndices)
            : ParseLinesSequential(lines, columnIndices);
    }

    private static CsvParseResult ParseLinesSequential(
        List<(int RowNumber, string Line)> lines,
        ColumnIndices indices)
    {
        var result = new CsvParseResult();

        foreach (var (rowNumber, line) in lines)
        {
            var (Order, Skipped) = ParseLine(line, indices);
            if (Order is not null)
            {
                result.Orders.Add(Order);
            }
            else
            {
                result.SkippedCount++;
                result.SkippedRows.Add(rowNumber);
            }
        }

        return result;
    }

    private static CsvParseResult ParseLinesParallel(
        List<(int RowNumber, string Line)> lines,
        ColumnIndices indices)
    {
        var orders = new ConcurrentBag<(int RowNumber, CreateOrderDto Order)>();
        var skippedRows = new ConcurrentBag<int>();

        Parallel.ForEach(lines, line =>
        {
            var (Order, Skipped) = ParseLine(line.Line, indices);
            if (Order is not null)
            {
                orders.Add((line.RowNumber, Order));
            }
            else
            {
                skippedRows.Add(line.RowNumber);
            }
        });

        var sortedOrders = orders.OrderBy(x => x.RowNumber).Select(x => x.Order).ToList();
        var sortedSkipped = skippedRows.Order().ToList();

        return new CsvParseResult
        {
            Orders = sortedOrders,
            SkippedCount = sortedSkipped.Count,
            SkippedRows = sortedSkipped
        };
    }

    private static (CreateOrderDto? Order, bool Skipped) ParseLine(string line, ColumnIndices indices)
    {
        var values = ParseCsvLine(line);
        var maxIndex = Math.Max(Math.Max(indices.Latitude, indices.Longitude), indices.Subtotal);

        if (values.Length <= maxIndex)
            return (null, true);

        if (!double.TryParse(values[indices.Latitude], CultureInfo.InvariantCulture, out var latitude))
            return (null, true);

        if (!double.TryParse(values[indices.Longitude], CultureInfo.InvariantCulture, out var longitude))
            return (null, true);

        if (!decimal.TryParse(values[indices.Subtotal], CultureInfo.InvariantCulture, out var subtotal))
            return (null, true);

        if (subtotal <= 0)
            return (null, true);

        if (!IsValidNYCoordinates(latitude, longitude))
            return (null, true);

        var order = new CreateOrderDto
        {
            Latitude = latitude,
            Longitude = longitude,
            Subtotal = subtotal
        };

        if (indices.Timestamp >= 0 && indices.Timestamp < values.Length &&
            !string.IsNullOrEmpty(values[indices.Timestamp]))
        {
            if (DateTime.TryParse(values[indices.Timestamp], CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var timestamp))
            {
                order.Timestamp = timestamp;
            }
        }

        return (order, false);
    }

    private readonly record struct ColumnIndices
    {
        public int Latitude { get; init; }
        public int Longitude { get; init; }
        public int Subtotal { get; init; }
        public int Timestamp { get; init; }
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