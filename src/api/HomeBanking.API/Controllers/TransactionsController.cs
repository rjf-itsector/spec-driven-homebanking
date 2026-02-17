using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeBanking.Contracts.Responses;
using HomeBanking.Infrastructure.Data;
using System.Security.Claims;

namespace HomeBanking.API.Controllers;

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

    [HttpGet("accounts/{accountId:guid}/transactions")]
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

    [HttpGet("transactions/{id:guid}")]
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
