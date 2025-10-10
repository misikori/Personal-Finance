namespace Budget.Application.Reports;

public record GenerateReportRequest(
    Guid UserId,
    string Username,
    string EmailAddress,
    Guid WalletId,
    DateTime StartDate,
    DateTime EndDate
);

public interface IReportService
{
    Task GenerateTransactionReport(GenerateReportRequest request);
}
