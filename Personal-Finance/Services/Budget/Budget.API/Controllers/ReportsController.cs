using Budget.Application.Reports;
using Microsoft.AspNetCore.Mvc;

namespace Budget.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        this._reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
    }

    [HttpPost("transactions")]
    public async Task<IActionResult> GenerateTransactionReport([FromBody] GenerateReportRequest request)
    {
        await this._reportService.GenerateTransactionReport(request);

        return this.Accepted(new { Message = "Report generation has been queued." });
    }
}
