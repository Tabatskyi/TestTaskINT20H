namespace TestTaskINT20H.Domain.Orders.ValueObjects;

/// <summary>
/// Represents the result of a tax calculation.
/// This is an immutable value object.
/// </summary>
public sealed record TaxCalculation
{
    public TaxBreakdown Breakdown { get; init; }
    public Money TaxAmount { get; init; }
    public IReadOnlyList<string> Jurisdictions { get; init; }

    public TaxCalculation(TaxBreakdown breakdown, Money taxAmount, IReadOnlyList<string> jurisdictions)
    {
        ArgumentNullException.ThrowIfNull(breakdown);
        ArgumentNullException.ThrowIfNull(taxAmount);

        if (jurisdictions == null || jurisdictions.Count == 0)
            throw new ArgumentException("At least one jurisdiction must be specified.", nameof(jurisdictions));

        Breakdown = breakdown;
        TaxAmount = taxAmount;
        Jurisdictions = jurisdictions;
    }
}
