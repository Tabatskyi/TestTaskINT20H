using TestTaskINT20H.Application.Orders.DTOs;
using TestTaskINT20H.Domain.Orders.Entities;

namespace TestTaskINT20H.Application.Orders.Mappers;

/// <summary>
/// Maps between domain entities and DTOs.
/// </summary>
public sealed class OrderMapper
{
    public OrderDto MapToDto(Order order) => order == null
        ? throw new ArgumentNullException(nameof(order))
        : new OrderDto
        {
            Id = order.Id,
            Latitude = order.Location.Latitude,
            Longitude = order.Location.Longitude,
            Subtotal = order.Subtotal.Amount,
            Timestamp = order.Timestamp,
            CompositeTaxRate = order.GetCompositeTaxRate(),
            TaxAmount = order.TaxCalculation?.TaxAmount.Amount ?? 0,
            TotalAmount = order.GetTotalAmount().Amount,
            Breakdown = new TaxBreakdownDto
            {
                StateRate = order.TaxCalculation?.Breakdown.StateRate ?? 0,
                CountyRate = order.TaxCalculation?.Breakdown.CountyRate ?? 0,
                CityRate = order.TaxCalculation?.Breakdown.CityRate ?? 0,
                SpecialRates = order.TaxCalculation?.Breakdown.SpecialRates ?? 0
            },
            Jurisdictions = order.GetJurisdictions().ToList()
        };
}