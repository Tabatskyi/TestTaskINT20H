namespace TestTaskINT20H.Domain.Orders.ValueObjects;

/// <summary>
/// Represents a monetary amount with a specific currency.
/// This is an immutable value object.
/// </summary>
public sealed record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; }

    public Money(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty.", nameof(currency));

        Amount = Math.Round(amount, 2);
        Currency = currency;
    }

    public static Money Zero => new(0);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add money with different currencies: {Currency} and {other.Currency}");

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Multiply(decimal factor)
    {
        return new Money(Amount * factor, Currency);
    }

    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator *(Money money, decimal factor) => money.Multiply(factor);
}
