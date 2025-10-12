using Budget.Application.Interfaces;
using Budget.Application.Transactions;
using Budget.Domain.Entities;
using Budget.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Budget.API.BackgroundWorker;

public class RecurringTransactionWorker : BackgroundService
{
    private readonly ILogger<RecurringTransactionWorker> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public RecurringTransactionWorker(ILogger<RecurringTransactionWorker> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this._serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this._logger.LogInformation("Recurring Transaction Worker is starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                this._logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                using (var scope = this._serviceScopeFactory.CreateScope())
                {
                    var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
                    var dbContext = scope.ServiceProvider.GetRequiredService<BudgetDbContext>();

                    var dueTransactions = await dbContext.RecurringTransactions
                        .Where(rt => rt.IsActive && rt.NextDueDate <= DateTime.UtcNow)
                        .ToListAsync(stoppingToken);

                    if (!dueTransactions.Any())
                    {
                        this._logger.LogInformation("No recurring transaction are due for processing.");
                    }
                    else
                    {
                        this._logger.LogInformation("Found {Count} recurring transactions to process.",
                            dueTransactions.Count);
                    }

                    foreach (var recurringTx in dueTransactions)
                    {
                        var dto = new CreateTransactionDto(
                            UserId: recurringTx.UserId,
                            WalletId: recurringTx.WalletId,
                            Amount: recurringTx.Amount,
                            Type: recurringTx.TransactionType.ToString(),
                            Description: recurringTx.Description,
                            Date: recurringTx.NextDueDate,
                            Currency: recurringTx.Currency,
                            CategoryName: recurringTx.Category
                        );

                        await transactionService.CreateTransactionAsync(dto);
                        this._logger.LogInformation("Created transaction for recurring item : {Category}",
                            recurringTx.Category);

                        // calculate the next due date based on frequency

                        var newNextDueDate = recurringTx.RecurrenceFrequency switch
                        {
                            RecurrenceFrequency.Weekly => recurringTx.NextDueDate.AddDays(7),
                            RecurrenceFrequency.Monthly => recurringTx.NextDueDate.AddMonths(1),
                            RecurrenceFrequency.Yearly => recurringTx.NextDueDate.AddYears(1),
                            _ => throw new ArgumentOutOfRangeException(nameof(recurringTx.RecurrenceFrequency))
                        };

                        // if the new due date is past the end date, deactivate the recurring transaction.
                        if (recurringTx.EndDate.HasValue && newNextDueDate > recurringTx.EndDate.Value)
                        {
                            recurringTx.IsActive = false;
                            this._logger.LogInformation(
                                "Deactivating recurring transaction for item : {Category}, as it reached its end date.",
                                recurringTx.Category);
                        }
                        else
                        {
                            recurringTx.NextDueDate = newNextDueDate;
                        }
                    }

                    if (dueTransactions.Any())
                    {
                        await dbContext.SaveChangesAsync(stoppingToken);
                        this._logger.LogInformation("Successfully processed {Count} recurring transactions.",
                            dueTransactions.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "An error ocurred while processing recurring transactions.");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
