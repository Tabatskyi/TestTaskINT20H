using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Reflection;
using System.Text;
using TestTaskINT20H.Application.Auth.Services;
using TestTaskINT20H.Application.Orders.Mappers;
using TestTaskINT20H.Application.Orders.Services;
using TestTaskINT20H.Domain.Auth.Repositories;
using TestTaskINT20H.Domain.Auth.Services;
using TestTaskINT20H.Domain.Orders.Repositories;
using TestTaskINT20H.Domain.Orders.Services;
using TestTaskINT20H.Infrastructure.Auth;
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

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("Jwt:Key is not configured. Set it via appsettings or the Jwt__Key environment variable.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };
    });

builder.Services.AddAuthorization();

// PostGIS — EF Core with Npgsql + NetTopologySuite
// AddDbContextFactory registers IDbContextFactory<T> (singleton) and keeps OrderDbContext available as scoped
builder.Services.AddDbContextFactory<OrderDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("OrdersConnection"),
        npgsql => npgsql.UseNetTopologySuite()
    ));

// Separate database for admin accounts
builder.Services.AddDbContext<AdminDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AdminsConnection")));
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Wellness Kit Orders API",
        Version = "v1",
        Description = "API for managing wellness kit orders with NY State tax calculation"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token. The 'Bearer ' prefix is added automatically."
    });

    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", doc), [] }
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

// Auth
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<AuthApplicationService>();

// Infrastructure
builder.Services.AddScoped<IOrderRepository, PostgresOrderRepository>();
builder.Services.AddHostedService<DatabaseInitializer>();

// Application Services
builder.Services.AddScoped<OrderApplicationService>();
builder.Services.AddSingleton<CsvImportService>();
builder.Services.AddSingleton<OrderMapper>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("Default");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapControllers();

app.Run();
