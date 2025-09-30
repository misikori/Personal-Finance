using Budget.Application.Interfaces;
using Budget.Application.SpendingLimits;
using Microsoft.AspNetCore.Mvc;

namespace Budget.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpendingLimitsController : ControllerBase
{
    private readonly ISpendingLimitService _service;
    private readonly ISpendingLimitRepository _repository;

    public SpendingLimitsController(ISpendingLimitService service, ISpendingLimitRepository repository)
    {
        _service = service;
        _repository = repository;
    }

    [HttpPost]
    public async Task<IActionResult> CreateLimit([FromBody] CreateLimitDto createLimitDto)
    {
        try
        {
            var limit = await this._service.CreateLimitAsync(createLimitDto);
            return Ok(limit);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpGet("wallet/{walletId:guid}")]
    public async Task<IActionResult> GetLimitsForWallet(Guid walletId)
    {
        var limits = await this._repository.GetLimitsForWalletAsync(walletId);

        var dtos = limits.Select(l => new ResponseLimitDto(
            Id: l.Id,
            Amount: l.Amount,
            Month: l.Month,
            Year: l.Year,
            CategoryName: l.CategoryName
        ));
        return Ok(dtos);
    }
}
