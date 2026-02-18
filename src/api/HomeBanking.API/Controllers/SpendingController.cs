using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeBanking.Contracts.Responses;
using HomeBanking.Infrastructure.Data;
using System.Security.Claims;

namespace HomeBanking.API.Controllers;

/// <summary>
/// Controller for spending analytics and category breakdowns.
/// </summary>
[ApiController]
[Route("api/v1/spending")]
[Authorize]
public class SpendingController : ControllerBase
{
    private readonly HomeBankingDbContext _context;

    public SpendingController(HomeBankingDbContext context)
    {
        _context = context;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// Retrieves a transaction summary grouped by category.
    /// Optionally filter by a specific account and direction (spending or savings).
    /// </summary>
    /// <param name="accountId">Optional account ID to filter by.</param>
    /// <param name="direction">"spending" (default) for debits, "savings" for credits.</param>
    /// <param name="fromDate">Start of date range (defaults to 30 days ago).</param>
    /// <param name="toDate">End of date range (defaults to now).</param>
    /// <response code="200">Returns the summary.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not own the specified account.</response>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(SpendingSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSpendingSummary(
        [FromQuery] Guid? accountId = null,
        [FromQuery] string direction = "spending",
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var userId = GetCurrentUserId();
        var to = toDate ?? DateTime.UtcNow;
        var from = fromDate ?? to.AddDays(-30);
        var isSavings = string.Equals(direction, "savings", StringComparison.OrdinalIgnoreCase);

        var query = _context.Transactions
            .Include(t => t.Account)
            .Where(t => t.Account.UserId == userId && t.Account.IsActive)
            .Where(t => t.Timestamp >= from && t.Timestamp <= to)
            .AsQueryable();

        // Filter by direction: negative amounts = spending (debits), positive = savings (credits)
        query = isSavings
            ? query.Where(t => t.Amount > 0)
            : query.Where(t => t.Amount < 0);

        if (accountId.HasValue)
        {
            // Verify ownership
            var account = await _context.Accounts.FindAsync(accountId.Value);
            if (account == null || account.UserId != userId)
                return Forbid();

            query = query.Where(t => t.AccountId == accountId.Value);
        }

        var transactions = await query.ToListAsync();

        var totalAmount = transactions.Sum(t => Math.Abs(t.Amount));
        var days = Math.Max(1, (to - from).Days);
        var averageDaily = totalAmount / days;

        var categories = transactions
            .GroupBy(t => t.Category)
            .Select(g => new CategorySpendingDto(
                g.Key.ToString(),
                g.Sum(t => Math.Abs(t.Amount)),
                g.Count(),
                totalAmount > 0
                    ? Math.Round(g.Sum(t => Math.Abs(t.Amount)) / totalAmount * 100, 1)
                    : 0
            ))
            .OrderByDescending(c => c.Total)
            .ToList();

        return Ok(new SpendingSummaryDto(from, to, totalAmount, categories, Math.Round(averageDaily, 2)));
    }
}
