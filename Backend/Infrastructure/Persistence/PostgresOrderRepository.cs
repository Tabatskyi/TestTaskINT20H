using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using TestTaskINT20H.Domain.Orders.Entities;
using TestTaskINT20H.Domain.Orders.Repositories;
using TestTaskINT20H.Domain.Orders.Specifications;

namespace TestTaskINT20H.Infrastructure.Persistence;

/// <summary>
/// PostgreSQL / PostGIS implementation of the order repository.
/// Scalar filters (date, total) are evaluated server-side.
/// The PostGIS <c>point</c> generated column is used for spatial proximity queries.
/// </summary>
public sealed class PostgresOrderRepository(OrderDbContext dbContext) : IOrderRepository
{
    public void Add(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        dbContext.Orders.Add(order);
        dbContext.SaveChanges();
    }

    public void AddRange(IEnumerable<Order> orders)
    {
        ArgumentNullException.ThrowIfNull(orders);
        dbContext.Orders.AddRange(orders);
        dbContext.SaveChanges();
    }

    public Order? GetById(Guid id)
        => dbContext.Orders.FirstOrDefault(o => o.Id == id);

    public IReadOnlyList<Order> Find(OrderSpecification spec)
    {
        var serverQuery = ApplyServerSideFilters(spec)
            .OrderByDescending(o => o.Timestamp);

        if (string.IsNullOrEmpty(spec.Jurisdiction))
        {
            return serverQuery
                .Skip(spec.Skip)
                .Take(spec.Take)
                .ToList()
                .AsReadOnly();
        }

        // Jurisdictions is a JSONB value-converted column — filter client-side after server push-down
        return ApplyJurisdictionFilter(serverQuery.AsEnumerable(), spec.Jurisdiction)
            .Skip(spec.Skip)
            .Take(spec.Take)
            .ToList()
            .AsReadOnly();
    }

    public int Count(OrderSpecification spec)
    {
        var serverQuery = ApplyServerSideFilters(spec);

        if (string.IsNullOrEmpty(spec.Jurisdiction))
            return serverQuery.Count();

        return ApplyJurisdictionFilter(serverQuery.AsEnumerable(), spec.Jurisdiction).Count();
    }

    // ── private helpers ──────────────────────────────────────────────────────

    private IQueryable<Order> ApplyServerSideFilters(OrderSpecification spec)
    {
        IQueryable<Order> query = dbContext.Orders;

        if (spec.FromDate.HasValue)
            query = query.Where(o => o.Timestamp >= spec.FromDate.Value);

        if (spec.ToDate.HasValue)
            query = query.Where(o => o.Timestamp <= spec.ToDate.Value);

        if (spec.MinTotal.HasValue)
            query = query.Where(o =>
                o.TaxCalculation != null &&
                o.Subtotal.Amount + o.TaxCalculation.TaxAmount.Amount >= spec.MinTotal.Value);

        if (spec.MaxTotal.HasValue)
            query = query.Where(o =>
                o.TaxCalculation != null &&
                o.Subtotal.Amount + o.TaxCalculation.TaxAmount.Amount <= spec.MaxTotal.Value);

        return query;
    }

    private static IEnumerable<Order> ApplyJurisdictionFilter(IEnumerable<Order> orders, string jurisdiction)
        => orders.Where(o => o.GetJurisdictions()
            .Any(j => j.Contains(jurisdiction, StringComparison.OrdinalIgnoreCase)));
}
