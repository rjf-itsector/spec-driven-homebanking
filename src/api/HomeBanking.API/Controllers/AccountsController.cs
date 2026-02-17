using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeBanking.Contracts.Responses;
using HomeBanking.Infrastructure.Data;
using System.Security.Claims;

namespace HomeBanking.API.Controllers;

/// <summary>
/// Controller for managing user bank accounts.
/// </summary>
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

    /// <summary>
    /// Retrieves all active accounts for the authenticated user.
    /// </summary>
    /// <returns>A list of the user's active bank accounts.</returns>
    /// <response code="200">Returns the list of accounts.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Retrieves a specific account by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the account.</param>
    /// <returns>The account details including transaction count.</returns>
    /// <response code="200">Returns the account details.</response>
    /// <response code="403">User does not own the account.</response>
    /// <response code="404">Account not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AccountDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
