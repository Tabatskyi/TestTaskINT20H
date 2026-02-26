using TestTaskINT20H.Domain.Orders.Entities;
using TestTaskINT20H.Domain.Orders.Specifications;

namespace TestTaskINT20H.Domain.Orders.Repositories;

/// <summary>
/// Repository interface for Order aggregate persistence.
/// </summary>
public interface IOrderRepository
{
    void Add(Order order);
    void AddRange(IEnumerable<Order> orders);
    Order? GetById(Guid id);
    IReadOnlyList<Order> Find(OrderSpecification specification);
    int Count(OrderSpecification specification);
}