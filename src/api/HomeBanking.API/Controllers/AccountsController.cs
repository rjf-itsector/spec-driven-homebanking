using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeBanking.Contracts.Responses;
using HomeBanking.Infrastructure.Data;
using System.Security.Claims;

namespace HomeBanking.API.Controllers;

[ApiController]
[Route("api/v1/accounts")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly HomeBankingDbContext _context;

    public AccountsController(HomeBankingDbContext context)
    {
        _context = context;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    [HttpGet]
    public async Task<IActionResult> GetAccounts()
    {
        var userId = GetCurrentUserId();
        var accounts = await _context.Accounts
            .Where(a => a.UserId == userId && a.IsActive)
            .Select(a => new AccountDto(
                a.Id,
                a.AccountNumber,
                a.Type.ToString(),
                a.Balance,
                a.Currency,
                a.IsActive,
                a.CreatedAt,
                a.UpdatedAt
            ))
            .ToListAsync();

        return Ok(new { accounts });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAccount(Guid id)
    {
        var userId = GetCurrentUserId();

        var account = await _context.Accounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (account == null)
        {
            return NotFound(new { message = "Account not found" });
        }

        if (account.UserId != userId)
        {
            return StatusCode(403, new { message = "Access denied" });
        }

        var dto = new AccountDetailDto(
            account.Id,
            account.AccountNumber,
            account.Type.ToString(),
            account.Balance,
            account.Currency,
            account.IsActive,
            account.Transactions.Count,
            account.CreatedAt,
            account.UpdatedAt
        );

        return Ok(dto);
    }
}
