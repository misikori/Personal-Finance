using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Report.Application.Abstractions;
using Report.Domain.Entities;

namespace Report.Infrastructure.Pdf
{
    public class QuestPdfReportGenerator : IPdfReportGenerator
    {
        public async Task<byte[]> GenerateTransactionsReportAsync(string userName, IEnumerable<Transaction> transactions)
        {
            var list = transactions.OrderByDescending(t => t.Date).ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Header().Text($"Transactions Report – {userName}")
                        .SemiBold().FontSize(20).AlignCenter();

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(100);
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                            cols.ConstantColumn(80);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(H).Text("Date");
                            header.Cell().Element(H).Text("Wallet");
                            header.Cell().Element(H).Text("Category");
                            header.Cell().Element(H).Text("Amount");

                            static IContainer H(IContainer c) =>
                                c.DefaultTextStyle(x => x.SemiBold())
                                 .Background(Colors.Grey.Lighten3).Padding(5);
                        });

                        foreach (var t in list)
                        {
                            table.Cell().Element(C).Text(t.Date.ToString("yyyy-MM-dd"));
                            table.Cell().Element(C).Text(t.WalletName);
                            table.Cell().Element(C).Text(t.CategoryName);
                            table.Cell().Element(C).Text($"{(t.IsIncome ? "+" : "-")}{t.Amount:0.00} {t.Currency}");

                            static IContainer C(IContainer c) =>
                                c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4);
                        }
                    });
                });
            });

            using var stream = new MemoryStream();
            document.GeneratePdf(stream);
            return await Task.FromResult(stream.ToArray());
        }
    }
}
