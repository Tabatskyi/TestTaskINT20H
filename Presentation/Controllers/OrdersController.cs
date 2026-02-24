using Microsoft.AspNetCore.Mvc;
using TestTaskINT20H.Application.Orders.DTOs;
using TestTaskINT20H.Application.Orders.Services;
using TestTaskINT20H.Application.Shared;

namespace TestTaskINT20H.Presentation.Controllers;

[ApiController]
[Route("orders")]
[Produces("application/json")]
public sealed class OrdersController(OrderApplicationService orderService, CsvImportService csvImportService) : ControllerBase
{
    private readonly OrderApplicationService _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
    private readonly CsvImportService _csvImportService = csvImportService ?? throw new ArgumentNullException(nameof(csvImportService));

    /// <summary>
    /// Create a new order manually with tax calculation
    /// </summary>
    /// <param name="request">Order details including coordinates and subtotal</param>
    /// <returns>The created order with calculated taxes</returns>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public IActionResult CreateOrder([FromBody] CreateOrderDto request)
    {
        try
        {
            var order = _orderService.CreateOrder(request);
            return Created($"/orders/{order.Id}", order);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
    }

    /// <summary>
    /// Import orders from CSV file
    /// </summary>
    /// <param name="file">CSV file with orders (columns: latitude, longitude, subtotal, timestamp)</param>
    /// <returns>List of imported orders with calculated taxes</returns>
    [HttpPost("import")]
    [ProducesResponseType(typeof(ImportOrdersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public IActionResult ImportOrders(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new ErrorResponse { Error = "No file uploaded" });

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new ErrorResponse { Error = "File must be a CSV" });

        var startTime = System.Diagnostics.Stopwatch.GetTimestamp();

        try
        {
            using var stream = file.OpenReadStream();
            var parseResult = _csvImportService.ParseCsv(stream);

            if (parseResult.Orders.Count == 0)
                return BadRequest(new ErrorResponse
                {
                    Error = "No valid orders found in CSV",
                    SkippedRows = parseResult.SkippedRows
                });

            var orders = _orderService.ImportOrders(parseResult.Orders);

            var elapsedMs = System.Diagnostics.Stopwatch.GetElapsedTime(startTime).TotalMilliseconds;

            var response = new ImportOrdersResponse
            {
                Message = $"Successfully imported {orders.Count} orders for {elapsedMs / 1000.0:F2} seconds, Glory to C#, Glory to KSE",
                ImportedCount = orders.Count,
                SkippedCount = parseResult.SkippedCount,
                SkippedRows = parseResult.SkippedRows,
                ProcessingTimeMs = (long)elapsedMs
            };

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse { Error = ex.Message });
        }
    }

    /// <summary>
    /// Get orders with pagination and filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Page<OrderDto>), StatusCodes.Status200OK)]
    public IActionResult GetOrders(
        [FromQuery(Name = "from_date")] DateTime? fromDate,
        [FromQuery(Name = "to_date")] DateTime? toDate,
        [FromQuery(Name = "min_total")] decimal? minTotal,
        [FromQuery(Name = "max_total")] decimal? maxTotal,
        [FromQuery] string? jurisdiction,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        var orders = _orderService.GetOrders(
            fromDate, toDate, minTotal, maxTotal, jurisdiction, page, size);
        return Ok(orders);
    }

    /// <summary>
    /// Get a specific order by ID
    /// </summary>
    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult GetOrder(Guid orderId)
    {
        var order = _orderService.GetOrder(orderId);
        if (order is null)
            return NotFound(new ErrorResponse { Error = "Order not found" });

        return Ok(order);
    }
}