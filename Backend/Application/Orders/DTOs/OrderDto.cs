using System.Text.Json.Serialization;

namespace TestTaskINT20H.Application.Orders.DTOs;

public sealed record OrderDto
{
    public Guid Id { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public decimal Subtotal { get; init; }
    public DateTime Timestamp { get; init; }

    [JsonPropertyName("composite_tax_rate")]
    public decimal CompositeTaxRate { get; init; }

    [JsonPropertyName("tax_amount")]
    public decimal TaxAmount { get; init; }

    [JsonPropertyName("total_amount")]
    public decimal TotalAmount { get; init; }

    public TaxBreakdownDto Breakdown { get; init; } = new();
    public List<string> Jurisdictions { get; init; } = [];
}

public sealed record TaxBreakdownDto
{
    [JsonPropertyName("state_rate")]
    public decimal StateRate { get; init; }

    [JsonPropertyName("county_rate")]
    public decimal CountyRate { get; init; }

    [JsonPropertyName("city_rate")]
    public decimal CityRate { get; init; }

    [JsonPropertyName("special_rates")]
    public decimal SpecialRates { get; init; }
}

public sealed record CreateOrderDto
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public decimal Subtotal { get; init; }
    public DateTime? Timestamp { get; init; }
}

public sealed record ImportOrdersResponse
{
    public string Message { get; init; } = string.Empty;

    [JsonPropertyName("imported_count")]
    public int ImportedCount { get; init; }

    [JsonPropertyName("skipped_count")]
    public int SkippedCount { get; init; }

    [JsonPropertyName("skipped_rows")]
    public List<int> SkippedRows { get; init; } = [];

    [JsonPropertyName("processing_time_ms")]
    public long ProcessingTimeMs { get; init; }
}