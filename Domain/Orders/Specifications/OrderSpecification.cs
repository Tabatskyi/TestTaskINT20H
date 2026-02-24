namespace TestTaskINT20H.Domain.Orders.Specifications;

/// <summary>
/// Specification pattern for querying orders with various filters.
/// </summary>
public sealed class OrderSpecification(
    DateTime? fromDate = null,
    DateTime? toDate = null,
    decimal? minTotal = null,
    decimal? maxTotal = null,
    string? jurisdiction = null,
    int skip = 0,
    int take = 10)
{
    public DateTime? FromDate { get; init; } = fromDate;
    public DateTime? ToDate { get; init; } = toDate;
    public decimal? MinTotal { get; init; } = minTotal;
    public decimal? MaxTotal { get; init; } = maxTotal;
    public string? Jurisdiction { get; init; } = jurisdiction;
    public int Skip { get; init; } = skip >= 0 ? skip : 0;
    public int Take { get; init; } = take > 0 ? take : 10;
}