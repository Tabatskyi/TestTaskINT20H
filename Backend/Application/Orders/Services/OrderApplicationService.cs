using TestTaskINT20H.Application.Orders.DTOs;
using TestTaskINT20H.Application.Orders.Mappers;
using TestTaskINT20H.Application.Shared;
using TestTaskINT20H.Domain.Orders.Entities;
using TestTaskINT20H.Domain.Orders.Repositories;
using TestTaskINT20H.Domain.Orders.Services;
using TestTaskINT20H.Domain.Orders.Specifications;
using TestTaskINT20H.Domain.Orders.ValueObjects;

namespace TestTaskINT20H.Application.Orders.Services;

/// <summary>
/// Application service for order use cases.
/// Orchestrates domain objects and coordinates application logic.
/// </summary>
public sealed class OrderApplicationService(
    IOrderRepository orderRepository,
    ITaxCalculationService taxCalculationService,
    OrderMapper mapper)
{
    private readonly IOrderRepository _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    private readonly ITaxCalculationService _taxCalculationService = taxCalculationService ?? throw new ArgumentNullException(nameof(taxCalculationService));
    private readonly OrderMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    public OrderDto CreateOrder(CreateOrderDto request)
    {
        var location = new Location(request.Latitude, request.Longitude);
        var subtotal = new Money(request.Subtotal);

        var order = Order.Create(location, subtotal, request.Timestamp);

        var taxCalculation = _taxCalculationService.CalculateTax(order.Location, order.Subtotal);
        order.ApplyTaxCalculation(taxCalculation);

        _orderRepository.Add(order);

        return _mapper.MapToDto(order);
    }

    public List<OrderDto> ImportOrders(List<CreateOrderDto> requests)
    {
        if (requests.Count == 0)
            return [];

        // Pre-allocate capacity for better performance
        var orders = new List<Order>(requests.Count);
        var orderDtos = new List<OrderDto>(requests.Count);

        // Create all orders in memory first (without individual repository calls)
        foreach (var request in requests)
        {
            var location = new Location(request.Latitude, request.Longitude);
            var subtotal = new Money(request.Subtotal);
            var order = Order.Create(location, subtotal, request.Timestamp);

            var taxCalculation = _taxCalculationService.CalculateTax(order.Location, order.Subtotal);
            order.ApplyTaxCalculation(taxCalculation);

            orders.Add(order);
        }

        // Batch insert all orders at once (single lock acquisition)
        _orderRepository.AddRange(orders);

        // Map to DTOs after persistence
        foreach (var order in orders)
        {
            orderDtos.Add(_mapper.MapToDto(order));
        }

        return orderDtos;
    }

    public OrderDto? GetOrder(Guid orderId)
    {
        var order = _orderRepository.GetById(orderId);
        return order == null ? null : _mapper.MapToDto(order);
    }

    public Page<OrderDto> GetOrders(
        DateTime? fromDate,
        DateTime? toDate,
        decimal? minTotal,
        decimal? maxTotal,
        string? jurisdiction,
        int page,
        int size)
    {
        if (page < 1) page = 1;
        if (size < 1) size = 10;
        if (size > 100) size = 100;

        var skip = (page - 1) * size;
        var spec = new OrderSpecification(
            fromDate, toDate, minTotal, maxTotal, jurisdiction, skip, size);

        var orders = _orderRepository.Find(spec);
        var totalCount = _orderRepository.Count(spec);
        var totalPages = (int)Math.Ceiling(totalCount / (double)size);

        var orderDtos = orders.Select(order => _mapper.MapToDto(order)).ToList();

        return new Page<OrderDto>(size, page, totalPages, orderDtos);
    }
}