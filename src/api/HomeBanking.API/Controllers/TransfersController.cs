using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeBanking.Contracts.Requests;
using HomeBanking.Contracts.Responses;
using HomeBanking.Core.Entities;
using HomeBanking.Infrastructure.Data;
using System.Security.Claims;

namespace HomeBanking.API.Controllers;

[ApiController]
[Route("api/v1/transfers")]
[Authorize]
public class TransfersController : ControllerBase
{
    private readonly HomeBankingDbContext _context;
    
    public TransfersController(HomeBankingDbContext context)
    {
        _context = context;
    }
    
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(TransferResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTransfer([FromBody] TransferRequest request)
    {
        var userId = GetCurrentUserId();
        
        // Fetch accounts with ownership check
        var fromAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.FromAccountId && a.UserId == userId);
        
        var toAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.ToAccountId && a.UserId == userId);
        
        if (fromAccount == null || toAccount == null)
        {
            return NotFound(new { message = "One or both accounts not found" });
        }
        
        // Check sufficient balance
        if (fromAccount.Balance < request.Amount)
        {
            return BadRequest(new
            {
                message = "Insufficient funds",
                detail = $"Available: {fromAccount.Balance:F2}"
            });
        }
        
        // Create transfer atomically (no explicit transaction needed for InMemory provider)
        var referenceNumber = $"TXN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
        var debitId = Guid.NewGuid();
        var creditId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        
        // Debit transaction (from account)
        var debitTransaction = new Transaction
        {
            Id = debitId,
            AccountId = fromAccount.Id,
            Type = TransactionType.Transfer,
            Amount = -request.Amount,
            BalanceAfter = fromAccount.Balance - request.Amount,
            Description = request.Description ?? $"Transfer to {toAccount.AccountNumber}",
            Category = TransactionCategory.Other,
            Timestamp = now,
            ReferenceNumber = referenceNumber,
            RelatedTransactionId = creditId
        };
        
        // Credit transaction (to account)
        var creditTransaction = new Transaction
        {
            Id = creditId,
            AccountId = toAccount.Id,
            Type = TransactionType.Transfer,
            Amount = request.Amount,
            BalanceAfter = toAccount.Balance + request.Amount,
            Description = request.Description ?? $"Transfer from {fromAccount.AccountNumber}",
            Category = TransactionCategory.Other,
            Timestamp = now,
            ReferenceNumber = referenceNumber,
            RelatedTransactionId = debitId
        };
        
        // Update balances
        fromAccount.Balance -= request.Amount;
        fromAccount.UpdatedAt = now;
        
        toAccount.Balance += request.Amount;
        toAccount.UpdatedAt = now;
        
        _context.Transactions.AddRange(debitTransaction, creditTransaction);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(
            nameof(GetTransfer),
            new { id = debitId },
            new TransferResponse(
                debitId,
                debitId,
                creditId,
                fromAccount.Balance,
                toAccount.Balance,
                now
            ));
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTransfer(Guid id)
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
            return NotFound(new { message = "Transfer not found" });
        }
        
        return Ok(transaction);
    }
}
