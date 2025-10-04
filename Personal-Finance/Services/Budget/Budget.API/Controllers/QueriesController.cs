using System.Collections.Generic;
using System.Linq;
using Budget.Application.Interfaces;
using Budget.Application.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Budget.API.Controllers;

[ApiController]
[Route("api")]
public class QueriesController : ControllerBase
{
    private readonly ITransactionRepository _transactionRepo;
    private readonly IWalletRepository _walletRepo;
    private readonly ICurrencyConverter _currencyConverter;
    public QueriesController(ITransactionRepository transactionRepo,  IWalletRepository walletRepo, ICurrencyConverter currencyConverter)
    {
        _transactionRepo = transactionRepo;
        _walletRepo = walletRepo;
        _currencyConverter = currencyConverter;
    }

    [HttpGet("wallets/{walletId:guid}/transactions")]
    public async Task<IActionResult> GetTransactions(Guid walletId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? categoryName)
    {
        var transactions = await this._transactionRepo.GetForWalletAsync(walletId: walletId, startDate: startDate,
            endDate: endDate, categoryName: categoryName);

        var transactionDtos = transactions.Select(t => new TransactionDto(
            t.Id,
            t.Amount,
            t.TransactionType.ToString(),
            t.Description,
            t.Date,
            t.Currency,
            t.CategoryName
        ));
        return Ok(transactionDtos);
    }

    [HttpGet("wallets/{walletId:guid}/summary/monthly")]
    public async Task<IActionResult> GetMonthlySummary(Guid walletId, [FromQuery] int month, [FromQuery] int year)
    {

        var wallet = await this._walletRepo.GetByIdAsync(walletId: walletId);
        if (wallet == null)
        {
            return this.NotFound("Wallet not found.");
        }

        var baseCurrency = wallet.Currency;
        var transactions =
            await this._transactionRepo.GetExpenseTransactionsMonthAsync(walletId: walletId, month: month, year: year);

        var spendingByCategory = new Dictionary<string, decimal>();

        foreach (var tx in transactions)
        {
            decimal convertedAmount = tx.Amount;
            if (tx.Currency != baseCurrency)
            {
                convertedAmount = await this._currencyConverter.ConvertAsync(tx.Currency, baseCurrency, tx.Amount);

            }

            if (spendingByCategory.ContainsKey(tx.CategoryName))
            {
                spendingByCategory[tx.CategoryName] += convertedAmount;
            }
            else
            {
                spendingByCategory[tx.CategoryName] = convertedAmount;
            }

        }

        var summaryDtos = spendingByCategory
            .Select(kvp => new CategorySpendingDto(
                kvp.Key,
                Math.Round(kvp.Value, 2)
            ))
            .OrderByDescending(dto => dto.TotalAmount)
            .ToList();

        return this.Ok(summaryDtos);
    }
}
