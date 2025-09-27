using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketGateway.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiCalls",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Vendor = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Identifier = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    RequestedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Succeeded = table.Column<bool>(type: "INTEGER", nullable: false),
                    HttpStatus = table.Column<int>(type: "INTEGER", nullable: true),
                    ResponseBytes = table.Column<int>(type: "INTEGER", nullable: true),
                    ParseLatencyMs = table.Column<int>(type: "INTEGER", nullable: true),
                    Error = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    StoredRows = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiCalls", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApiUsages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Vendor = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CallsMade = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiUsages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParseFailures",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Vendor = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    PrimaryIdentifier = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Error = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParseFailures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Quotes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Vendor = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Ticker = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", nullable: true),
                    Open = table.Column<decimal>(type: "TEXT", nullable: true),
                    High = table.Column<decimal>(type: "TEXT", nullable: true),
                    Low = table.Column<decimal>(type: "TEXT", nullable: true),
                    PrevClose = table.Column<decimal>(type: "TEXT", nullable: true),
                    Volume = table.Column<decimal>(type: "TEXT", nullable: true),
                    Currency = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vendors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    PerMinuteLimit = table.Column<int>(type: "INTEGER", nullable: false),
                    PerDayLimit = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendors", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiCalls_Vendor_Identifier_RequestedAtUtc",
                table: "ApiCalls",
                columns: new[] { "Vendor", "Identifier", "RequestedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ApiUsages_Vendor_Date",
                table: "ApiUsages",
                columns: new[] { "Vendor", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParseFailures_Vendor_Type_PrimaryIdentifier_OccurredAtUtc",
                table: "ParseFailures",
                columns: new[] { "Vendor", "Type", "PrimaryIdentifier", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_Vendor_Ticker_TimestampUtc",
                table: "Quotes",
                columns: new[] { "Vendor", "Ticker", "TimestampUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_Name",
                table: "Vendors",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiCalls");

            migrationBuilder.DropTable(
                name: "ApiUsages");

            migrationBuilder.DropTable(
                name: "ParseFailures");

            migrationBuilder.DropTable(
                name: "Quotes");

            migrationBuilder.DropTable(
                name: "Vendors");
        }
    }
}
