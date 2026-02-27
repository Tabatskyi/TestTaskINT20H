using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace TestTaskINT20H.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    point = table.Column<Point>(type: "geometry(Point,4326)", nullable: true, computedColumnSql: "ST_SetSRID(ST_MakePoint(longitude, latitude), 4326)", stored: true),
                    subtotal_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    subtotal_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    state_rate = table.Column<decimal>(type: "numeric(8,6)", nullable: true),
                    county_rate = table.Column<decimal>(type: "numeric(8,6)", nullable: true),
                    city_rate = table.Column<decimal>(type: "numeric(8,6)", nullable: true),
                    special_rates = table.Column<decimal>(type: "numeric(8,6)", nullable: true),
                    tax_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    tax_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    jurisdictions = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "orders");
        }
    }
}
