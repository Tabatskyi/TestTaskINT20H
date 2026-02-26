using System.Text.Json.Serialization;

namespace TestTaskINT20H.Application.Orders.DTOs;

public sealed class OrderDto
{
    public Guid Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public decimal Subtotal { get; set; }
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("composite_tax_rate")]
    public decimal CompositeTaxRate { get; set; }

    [JsonPropertyName("tax_amount")]
    public decimal TaxAmount { get; set; }

    [JsonPropertyName("total_amount")]
    public decimal TotalAmount { get; set; }

    public TaxBreakdownDto Breakdown { get; set; } = new();
    public List<string> Jurisdictions { get; set; } = [];
}

public sealed class TaxBreakdownDto
{
    [JsonPropertyName("state_rate")]
    public decimal StateRate { get; set; }

    [JsonPropertyName("county_rate")]
    public decimal CountyRate { get; set; }

    [JsonPropertyName("city_rate")]
    public decimal CityRate { get; set; }

    [JsonPropertyName("special_rates")]
    public decimal SpecialRates { get; set; }
}

public sealed class CreateOrderDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public decimal Subtotal { get; set; }
    public DateTime? Timestamp { get; set; }
}

public sealed class ImportOrdersResponse
{
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("imported_count")]
    public int ImportedCount { get; set; }

    [JsonPropertyName("skipped_count")]
    public int SkippedCount { get; set; }

    [JsonPropertyName("skipped_rows")]
    public List<int> SkippedRows { get; set; } = [];

    [JsonPropertyName("processing_time_ms")]
    public long ProcessingTimeMs { get; set; }
}

public sealed class ErrorResponse
{
    public string Error { get; set; } = string.Empty;

    [JsonPropertyName("skipped_rows")]
    public List<int>? SkippedRows { get; set; }
}
