namespace HomeBanking.Core.Entities;

/// <summary>
/// Represents a banking application user.
/// </summary>
public class User
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the user's email address (unique).</summary>
    public required string Email { get; set; }

    /// <summary>Gets or sets the hashed password.</summary>
    public required string PasswordHash { get; set; }

    /// <summary>Gets or sets the user's first name.</summary>
    public string FirstName { get; set; } = "Demo";

    /// <summary>Gets or sets the user's last name.</summary>
    public string LastName { get; set; } = "User";

    /// <summary>Gets or sets the UTC timestamp when the user was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets the collection of accounts owned by this user.</summary>
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
}
