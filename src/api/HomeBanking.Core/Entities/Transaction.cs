namespace HomeBanking.Core.Entities;

/// <summary>
/// Represents a financial transaction on an account.
/// </summary>
public class Transaction
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the ID of the account this transaction belongs to.</summary>
    public required Guid AccountId { get; set; }

    /// <summary>Gets or sets the transaction type.</summary>
    public TransactionType Type { get; set; }

    /// <summary>
    /// Gets or sets the transaction amount.
    /// Positive values represent credits; negative values represent debits.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the account balance immediately after this transaction.
    /// Stored as a snapshot for historical accuracy.
    /// </summary>
    public decimal BalanceAfter { get; set; }

    /// <summary>Gets or sets a human-readable description of the transaction.</summary>
    public required string Description { get; set; }

    /// <summary>Gets or sets the transaction category.</summary>
    public TransactionCategory Category { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the transaction occurred.</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the reference/confirmation number (e.g. "TXN-20260217-001234").
    /// </summary>
    public required string ReferenceNumber { get; set; }

    /// <summary>
    /// Gets or sets the ID of a related transaction (e.g. the other side of a transfer).
    /// Null for non-transfer transactions.
    /// </summary>
    public Guid? RelatedTransactionId { get; set; }

    /// <summary>Gets or sets the parent account (navigation property).</summary>
    public Account Account { get; set; } = null!;
}

/// <summary>
/// Types of financial transactions.
/// </summary>
public enum TransactionType
{
    /// <summary>Money deposited into the account.</summary>
    Deposit = 0,

    /// <summary>Money withdrawn from the account.</summary>
    Withdrawal = 1,

    /// <summary>Money transferred between accounts.</summary>
    Transfer = 2,

    /// <summary>Payment made from the account.</summary>
    Payment = 3,

    /// <summary>Fee charged by the bank.</summary>
    Fee = 4,

    /// <summary>Interest credited to the account.</summary>
    Interest = 5
}

/// <summary>
/// Categories for classifying transactions.
/// </summary>
public enum TransactionCategory
{
    /// <summary>Grocery purchases.</summary>
    Groceries = 0,

    /// <summary>Restaurant and dining expenses.</summary>
    Dining = 1,

    /// <summary>Transportation costs (fuel, transit, ride-share).</summary>
    Transportation = 2,

    /// <summary>General retail shopping.</summary>
    Shopping = 3,

    /// <summary>Utilities, rent, subscriptions.</summary>
    Bills = 4,

    /// <summary>Medical and healthcare expenses.</summary>
    Healthcare = 5,

    /// <summary>Movies, games, streaming services.</summary>
    Entertainment = 6,

    /// <summary>Uncategorised transactions.</summary>
    Other = 7
}
