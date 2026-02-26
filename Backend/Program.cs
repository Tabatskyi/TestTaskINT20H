using TestTaskINT20H.Application.Orders.Mappers;
using TestTaskINT20H.Application.Orders.Services;
using TestTaskINT20H.Domain.Orders.Repositories;
using TestTaskINT20H.Domain.Orders.Services;
using TestTaskINT20H.Infrastructure.Orders;
using Microsoft.OpenApi;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Wellness Kit Orders API",
        Version = "v1",
        Description = "API for managing wellness kit orders with NY State tax calculation"
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// Domain Services
builder.Services.AddSingleton<ITaxCalculationService, TaxCalculationService>();

// Infrastructure
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();

// Application Services
builder.Services.AddSingleton<OrderApplicationService>();
builder.Services.AddSingleton<CsvImportService>();
builder.Services.AddSingleton<OrderMapper>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapControllers();

app.Run();