using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using System.Text.Json;
using TestTaskINT20H.Domain.Orders.Entities;
using TestTaskINT20H.Domain.Orders.ValueObjects;

namespace TestTaskINT20H.Infrastructure.Persistence;

public sealed class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("postgis");

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Id).HasColumnName("id");
            entity.Property(o => o.Timestamp).HasColumnName("timestamp");

            entity.OwnsOne(o => o.Location, loc =>
            {
                loc.Property(l => l.Latitude)
                    .HasColumnName("latitude");
                loc.Property(l => l.Longitude)
                    .HasColumnName("longitude");
                loc.Ignore(l => l.Point);

                // Stored generated PostGIS column — usable in spatial queries (ST_DWithin, ST_Distance, etc.)
                loc.Property<Point>("point")
                    .HasColumnName("point")
                    .HasColumnType("geometry(Point,4326)")
                    .HasComputedColumnSql("ST_SetSRID(ST_MakePoint(longitude, latitude), 4326)", stored: true);
            });

            entity.OwnsOne(o => o.Subtotal, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("subtotal_amount")
                    .HasColumnType("numeric(18,2)");
                money.Property(m => m.Currency)
                    .HasColumnName("subtotal_currency")
                    .HasMaxLength(3);
            });

            entity.OwnsOne(o => o.TaxCalculation, tc =>
            {
                tc.OwnsOne(t => t.Breakdown, breakdown =>
                {
                    breakdown.Property(b => b.StateRate)
                        .HasColumnName("state_rate")
                        .HasColumnType("numeric(8,6)");
                    breakdown.Property(b => b.CountyRate)
                        .HasColumnName("county_rate")
                        .HasColumnType("numeric(8,6)");
                    breakdown.Property(b => b.CityRate)
                        .HasColumnName("city_rate")
                        .HasColumnType("numeric(8,6)");
                    breakdown.Property(b => b.SpecialRates)
                        .HasColumnName("special_rates")
                        .HasColumnType("numeric(8,6)");
                });

                tc.OwnsOne(t => t.TaxAmount, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("tax_amount")
                        .HasColumnType("numeric(18,2)");
                    money.Property(m => m.Currency)
                        .HasColumnName("tax_currency")
                        .HasMaxLength(3);
                });

                tc.Property(t => t.Jurisdictions)
                    .HasColumnName("jurisdictions")
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => (IReadOnlyList<string>)JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null)!
                    );
            });
        });
    }
}
