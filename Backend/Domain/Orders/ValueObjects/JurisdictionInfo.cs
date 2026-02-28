namespace TestTaskINT20H.Domain.Orders.ValueObjects;

/// <summary>
/// Represents a single tax jurisdiction with its contributing rate.
/// </summary>
/// <param name="Name">Display name (e.g. "New York State", "Kings County", "Yonkers").</param>
/// <param name="Type">
/// Jurisdiction level: <c>state</c>, <c>county</c>, <c>city_group</c>, or <c>city</c>.
/// </param>
/// <param name="TaxRate">The tax rate this jurisdiction contributes to the composite rate.</param>
public sealed record JurisdictionInfo(string Name, string Type, decimal TaxRate);
