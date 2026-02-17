namespace HomeBanking.Core.Entities;

/// <summary>
/// Represents a user's bank account.
/// </summary>
public class Account
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the ID of the owning user.</summary>
    public required Guid UserId { get; set; }

    /// <summary>Gets or sets the account number (last 4 digits visible to user).</summary>
    public required string AccountNumber { get; set; }

    /// <summary>Gets or sets the account type.</summary>
    public AccountType Type { get; set; }

    /// <summary>Gets or sets the current balance. Must be <c>decimal</c> for monetary precision.</summary>
    public decimal Balance { get; set; }

    /// <summary>Gets or sets the currency code (ISO 4217).</summary>
    public string Currency { get; set; } = "USD";

    /// <summary>Gets or sets whether the account is active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets the UTC timestamp when the account was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the UTC timestamp when the account was last updated.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the owning user (navigation property).</summary>
    public User User { get; set; } = null!;

    /// <summary>Gets the collection of transactions for this account.</summary>
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

/// <summary>
/// Types of bank accounts.
/// </summary>
public enum AccountType
{
    /// <summary>Standard checking account.</summary>
    Checking = 0,

    /// <summary>Savings account.</summary>
    Savings = 1,

    /// <summary>Credit card account (balance may be negative).</summary>
    CreditCard = 2,

    /// <summary>Investment / brokerage account.</summary>
    Investment = 3
}
