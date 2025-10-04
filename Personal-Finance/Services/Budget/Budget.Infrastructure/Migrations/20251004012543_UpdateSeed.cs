using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Budget.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("35a39ee9-e8b6-4646-a716-b979f5295a9e"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("417de232-ff5d-472a-aa4e-e47fd104daaa"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("cf77eee4-07bf-407f-8643-17933f94eacb"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("21ab4bab-9e03-48f3-ac91-0c7b77bcb66d"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("c2accf38-664f-4a62-acd3-bce1084a1763"));

            migrationBuilder.DeleteData(
                table: "Wallets",
                keyColumn: "Id",
                keyValue: new Guid("19b36d12-7547-4bec-b37e-d8c920038686"));

            migrationBuilder.DeleteData(
                table: "Wallets",
                keyColumn: "Id",
                keyValue: new Guid("39a72b70-69e2-490a-98c1-fc1d316c55d8"));

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "UserId" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Groceries", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "Bills", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "Salary", new Guid("11111111-1111-1111-1111-111111111111") }
                });

            migrationBuilder.InsertData(
                table: "Wallets",
                columns: new[] { "Id", "Currency", "CurrentBalance", "Name", "UserId" },
                values: new object[,]
                {
                    { new Guid("55555555-5555-5555-5555-555555555555"), "USD", 2500m, "Main funds", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("66666666-6666-6666-6666-666666666666"), "EUR", 800m, "Vacation funds", new Guid("11111111-1111-1111-1111-111111111111") }
                });

            migrationBuilder.InsertData(
                table: "Transactions",
                columns: new[] { "Id", "Amount", "CategoryName", "Currency", "Date", "Description", "TransactionType", "UserId", "WalletId" },
                values: new object[,]
                {
                    { new Guid("77777777-7777-7777-7777-777777777777"), 125m, "Groceries", "USD", new DateTime(2025, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Weekly grocery run", 1, new Guid("11111111-1111-1111-1111-111111111111"), new Guid("55555555-5555-5555-5555-555555555555") },
                    { new Guid("88888888-8888-8888-8888-888888888888"), 50m, "Bills", "USD", new DateTime(2025, 10, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Internet", 1, new Guid("11111111-1111-1111-1111-111111111111"), new Guid("55555555-5555-5555-5555-555555555555") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"));

            migrationBuilder.DeleteData(
                table: "Wallets",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"));

            migrationBuilder.DeleteData(
                table: "Wallets",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "UserId" },
                values: new object[,]
                {
                    { new Guid("35a39ee9-e8b6-4646-a716-b979f5295a9e"), "Groceries", new Guid("c327a4c2-635a-42e4-a76e-05590708eece") },
                    { new Guid("417de232-ff5d-472a-aa4e-e47fd104daaa"), "Salary", new Guid("c327a4c2-635a-42e4-a76e-05590708eece") },
                    { new Guid("cf77eee4-07bf-407f-8643-17933f94eacb"), "Bills", new Guid("c327a4c2-635a-42e4-a76e-05590708eece") }
                });

            migrationBuilder.InsertData(
                table: "Wallets",
                columns: new[] { "Id", "Currency", "CurrentBalance", "Name", "UserId" },
                values: new object[,]
                {
                    { new Guid("19b36d12-7547-4bec-b37e-d8c920038686"), "EUR", 800m, "Vacation funds", new Guid("c327a4c2-635a-42e4-a76e-05590708eece") },
                    { new Guid("39a72b70-69e2-490a-98c1-fc1d316c55d8"), "USD", 2500m, "Main funds", new Guid("c327a4c2-635a-42e4-a76e-05590708eece") }
                });

            migrationBuilder.InsertData(
                table: "Transactions",
                columns: new[] { "Id", "Amount", "CategoryName", "Currency", "Date", "Description", "TransactionType", "UserId", "WalletId" },
                values: new object[,]
                {
                    { new Guid("21ab4bab-9e03-48f3-ac91-0c7b77bcb66d"), 125m, "Groceries", "USD", new DateTime(2025, 10, 3, 1, 22, 20, 518, DateTimeKind.Utc).AddTicks(1170), "Weekly grocery run", 1, new Guid("c327a4c2-635a-42e4-a76e-05590708eece"), new Guid("39a72b70-69e2-490a-98c1-fc1d316c55d8") },
                    { new Guid("c2accf38-664f-4a62-acd3-bce1084a1763"), 50m, "Bills", "USD", new DateTime(2025, 10, 4, 1, 22, 20, 518, DateTimeKind.Utc).AddTicks(1980), "Internet", 1, new Guid("c327a4c2-635a-42e4-a76e-05590708eece"), new Guid("39a72b70-69e2-490a-98c1-fc1d316c55d8") }
                });
        }
    }
}
