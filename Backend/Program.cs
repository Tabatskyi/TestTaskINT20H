using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System.Reflection;
using TestTaskINT20H.Application.Orders.Mappers;
using TestTaskINT20H.Application.Orders.Services;
using TestTaskINT20H.Domain.Orders.Repositories;
using TestTaskINT20H.Domain.Orders.Services;
using TestTaskINT20H.Infrastructure.GIS;
using TestTaskINT20H.Infrastructure.Orders;
using TestTaskINT20H.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    options.AddPolicy("Default", policy => policy
        .WithOrigins(origins)
        .AllowAnyMethod()
        .AllowAnyHeader());
});

// PostGIS — EF Core with Npgsql + NetTopologySuite
// AddDbContextFactory registers IDbContextFactory<T> (singleton) and keeps OrderDbContext available as scoped
builder.Services.AddDbContextFactory<OrderDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.UseNetTopologySuite()
    ));
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

// GIS Services - load shapefiles at startup
builder.Services.AddSingleton(provider =>
{
    var countyLookup = new ShapefileCountyLookupService();
    var shapefilePath = Path.Combine(AppContext.BaseDirectory, "Data", "ny_counties.shp");
    countyLookup.LoadShapefile(shapefilePath);
    return countyLookup;
});

builder.Services.AddSingleton(provider =>
{
    var cityLookup = new ShapefileCityLookupService();
    var shapefilePath = Path.Combine(AppContext.BaseDirectory, "Data", "ny_places.shp");
    cityLookup.LoadShapefile(shapefilePath);
    return cityLookup;
});

// Domain Services
builder.Services.AddSingleton<ITaxCalculationService, TaxCalculationService>();

// Infrastructure
builder.Services.AddScoped<IOrderRepository, PostgresOrderRepository>();

// Application Services
builder.Services.AddScoped<OrderApplicationService>();
builder.Services.AddSingleton<CsvImportService>();
builder.Services.AddSingleton<OrderMapper>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("Default");

app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapControllers();

app.Run();