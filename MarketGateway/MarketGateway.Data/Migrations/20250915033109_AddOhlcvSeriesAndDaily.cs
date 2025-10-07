using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketGateway.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOhlcvSeriesAndDaily : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OhlcvDaily_Ticker_TimestampUtc",
                table: "OhlcvDaily");

            migrationBuilder.DropColumn(
                name: "Ticker",
                table: "OhlcvDaily");

            migrationBuilder.DropColumn(
                name: "Vendor",
                table: "OhlcvDaily");

            migrationBuilder.AddColumn<int>(
                name: "SeriesId",
                table: "OhlcvDaily",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "OhlcvSeries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Vendor = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Symbol = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Exchange = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                    Currency = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    Granularity = table.Column<int>(type: "INTEGER", nullable: false),
                    Adjustment = table.Column<int>(type: "INTEGER", nullable: false),
                    Partial = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OhlcvSeries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OhlcvDaily_SeriesId_TimestampUtc",
                table: "OhlcvDaily",
                columns: new[] { "SeriesId", "TimestampUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OhlcvSeries_Vendor_Symbol_Granularity_Adjustment",
                table: "OhlcvSeries",
                columns: new[] { "Vendor", "Symbol", "Granularity", "Adjustment" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OhlcvDaily_OhlcvSeries_SeriesId",
                table: "OhlcvDaily",
                column: "SeriesId",
                principalTable: "OhlcvSeries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OhlcvDaily_OhlcvSeries_SeriesId",
                table: "OhlcvDaily");

            migrationBuilder.DropTable(
                name: "OhlcvSeries");

            migrationBuilder.DropIndex(
                name: "IX_OhlcvDaily_SeriesId_TimestampUtc",
                table: "OhlcvDaily");

            migrationBuilder.DropColumn(
                name: "SeriesId",
                table: "OhlcvDaily");

            migrationBuilder.AddColumn<string>(
                name: "Ticker",
                table: "OhlcvDaily",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Vendor",
                table: "OhlcvDaily",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_OhlcvDaily_Ticker_TimestampUtc",
                table: "OhlcvDaily",
                columns: new[] { "Ticker", "TimestampUtc" },
                unique: true);
        }
    }
}
