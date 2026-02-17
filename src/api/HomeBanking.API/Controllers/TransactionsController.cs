using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeBanking.Contracts.Responses;
using HomeBanking.Infrastructure.Data;
using System.Security.Claims;

namespace HomeBanking.API.Controllers;

/// <summary>
/// Controller for viewing account transactions.
/// </summary>
[ApiController]
[Route("api/v1")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly HomeBankingDbContext _context;

    public TransactionsController(HomeBankingDbContext context)
    {
        _context = context;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// Retrieves a paginated list of transactions for a specific account.
    /// </summary>
    /// <param name="accountId">The unique identifier of the account.</param>
    /// <param name="cursor">Optional cursor for pagination (transaction ID to start after).</param>
    /// <param name="limit">Number of transactions to return (1-100, default 20).</param>
    /// <returns>A paginated list of transactions with cursor-based pagination metadata.</returns>
    /// <response code="200">Returns the list of transactions.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Account not found or not owned by user.</response>
    [HttpGet("accounts/{accountId:guid}/transactions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransactions(
        Guid accountId,
        [FromQuery] Guid? cursor = null,
        [FromQuery] int limit = 20)
    {
        var userId = GetCurrentUserId();

        // Verify account ownership
        var accountExists = await _context.Accounts
            .AnyAsync(a => a.Id == accountId && a.UserId == userId);

        if (!accountExists)
        {
            return NotFound(new { message = "Account not found" });
        }

        // Validate limit
        limit = Math.Clamp(limit, 1, 100);

        // Build query with cursor
        IQueryable<HomeBanking.Core.Entities.Transaction> query = _context.Transactions
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.Timestamp)
            .ThenByDescending(t => t.Id);

        if (cursor.HasValue)
        {
            var cursorTransaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == cursor.Value);

            if (cursorTransaction != null)
            {
                query = _context.Transactions
                    .Where(t => t.AccountId == accountId)
                    .Where(t =>
                        t.Timestamp < cursorTransaction.Timestamp ||
                        (t.Timestamp == cursorTransaction.Timestamp && t.Id.CompareTo(cursor.Value) < 0))
                    .OrderByDescending(t => t.Timestamp)
                    .ThenByDescending(t => t.Id);
            }
        }

        // Fetch limit + 1 to check if more exist
        var transactions = await query
            .Take(limit + 1)
            .Select(t => new TransactionDto(
                t.Id,
                t.Type.ToString(),
                t.Amount,
                t.BalanceAfter,
                t.Description,
                t.Category.ToString(),
                t.Timestamp,
                t.ReferenceNumber,
                t.RelatedTransactionId
            ))
            .ToListAsync();

        var hasMore = transactions.Count > limit;
        var items = hasMore ? transactions.Take(limit).ToList() : transactions;
        var nextCursor = hasMore ? items.Last().Id : (Guid?)null;

        return Ok(new
        {
            transactions = items,
            pagination = new
            {
                hasMore,
                nextCursor
            }
        });
    }

    /// <summary>
    /// Retrieves a specific transaction by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the transaction.</param>
    /// <returns>The transaction details.</returns>
    /// <response code="200">Returns the transaction details.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Transaction not found.</response>
    [HttpGet("transactions/{id:guid}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransaction(Guid id)
    {
        var userId = GetCurrentUserId();

        var transaction = await _context.Transactions
            .Where(t => t.Id == id && t.Account.UserId == userId)
            .Select(t => new TransactionDto(
                t.Id,
                t.Type.ToString(),
                t.Amount,
                t.BalanceAfter,
                t.Description,
                t.Category.ToString(),
                t.Timestamp,
                t.ReferenceNumber,
                t.RelatedTransactionId
            ))
            .FirstOrDefaultAsync();

        if (transaction == null)
        {
            return NotFound(new { message = "Transaction not found" });
        }

        return Ok(transaction);
    }
}
