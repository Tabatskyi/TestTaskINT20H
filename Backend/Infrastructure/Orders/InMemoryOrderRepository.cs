using NetTopologySuite.Geometries;
using TestTaskINT20H.Domain.Orders.Entities;
using TestTaskINT20H.Domain.Orders.Repositories;
using TestTaskINT20H.Domain.Orders.Specifications;

namespace TestTaskINT20H.Infrastructure.Orders;

/// <summary>
/// In-memory implementation of the order repository.
/// </summary>
public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly List<Order> _orders = [];
    private readonly Lock _lock = new();

    public void Add(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        lock (_lock)
        {
            _orders.Add(order);
        }
    }

    public void AddRange(IEnumerable<Order> orders)
    {
        ArgumentNullException.ThrowIfNull(orders);

        var orderList = orders as IList<Order> ?? orders.ToList();

        lock (_lock)
        {
            _orders.AddRange(orderList);
        }
    }

    public Order? GetById(Guid id)
    {
        lock (_lock)
        {
            return _orders.FirstOrDefault(order => order.Id == id);
        }
    }

    public IReadOnlyList<Order> Find(OrderSpecification spec)
    {
        lock (_lock)
        {
            return ApplySpecification(spec)
                .OrderByDescending(order => order.Timestamp)
                .Skip(spec.Skip)
                .Take(spec.Take)
                .ToList()
                .AsReadOnly();
        }
    }

    public int Count(OrderSpecification spec)
    {
        lock (_lock)
        {
            return ApplySpecification(spec).Count();
        }
    }

    private IEnumerable<Order> ApplySpecification(OrderSpecification spec)
    {
        IEnumerable<Order> query = _orders;

        if (spec.FromDate.HasValue)
        {
            var fromDate = spec.FromDate.Value;
            query = query.Where(order => order.Timestamp >= fromDate);
        }

        if (spec.ToDate.HasValue)
        {
            var toDate = spec.ToDate.Value;
            query = query.Where(order => order.Timestamp <= toDate);
        }

        if (spec.MinTotal.HasValue)
        {
            var minTotal = spec.MinTotal.Value;
            query = query.Where(order => order.GetTotalAmount().Amount >= minTotal);
        }

        if (spec.MaxTotal.HasValue)
        {
            var maxTotal = spec.MaxTotal.Value;
            query = query.Where(order => order.GetTotalAmount().Amount <= maxTotal);
        }

        if (!string.IsNullOrEmpty(spec.Jurisdiction))
        {
            var jurisdiction = spec.Jurisdiction;
            query = query.Where(order => order.GetJurisdictions()
                .Any(jur => jur.Contains(jurisdiction, StringComparison.OrdinalIgnoreCase)));
        }

        return query;
    }
}