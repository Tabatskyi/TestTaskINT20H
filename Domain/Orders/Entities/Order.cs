using TestTaskINT20H.Domain.Orders.ValueObjects;

namespace TestTaskINT20H.Domain.Orders.Entities;

/// <summary>
/// Order aggregate root. Represents a wellness kit order with location and pricing information.
/// </summary>
public sealed class Order
{
    public Guid Id { get; private set; }
    public Location Location { get; private set; }
    public Money Subtotal { get; private set; }
    public DateTime Timestamp { get; private set; }
    public TaxCalculation? TaxCalculation { get; private set; }

    // Private constructor for EF Core or serialization
    private Order() { }

    private Order(Guid id, Location location, Money subtotal, DateTime timestamp)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Order ID cannot be empty.", nameof(id));

        Id = id;
        Location = location ?? throw new ArgumentNullException(nameof(location));
        Subtotal = subtotal ?? throw new ArgumentNullException(nameof(subtotal));
        Timestamp = timestamp;

        if (Subtotal.Amount <= 0)
            throw new ArgumentException("Subtotal must be greater than zero.");
    }

    public static Order Create(Location location, Money subtotal, DateTime? timestamp = null)
    {
        if (!location.IsInNewYorkState())
            throw new InvalidOperationException("Orders can only be created for locations within New York State.");

        return new Order(
            Guid.NewGuid(),
            location,
            subtotal,
            timestamp ?? DateTime.UtcNow
        );
    }

    public void ApplyTaxCalculation(TaxCalculation taxCalculation)
    {
        TaxCalculation = taxCalculation ?? throw new ArgumentNullException(nameof(taxCalculation));
    }

    public Money GetTotalAmount()
    {
        if (TaxCalculation == null)
            throw new InvalidOperationException("Cannot calculate total amount before tax calculation is applied.");

        return Subtotal + TaxCalculation.TaxAmount;
    }

    public decimal GetCompositeTaxRate()
    {
        return TaxCalculation?.Breakdown.CompositeRate ?? 0;
    }

    public IReadOnlyList<string> GetJurisdictions()
    {
        return TaxCalculation?.Jurisdictions ?? Array.Empty<string>();
    }
}