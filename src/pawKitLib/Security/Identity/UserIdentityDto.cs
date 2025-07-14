using pawKitLib.Models;

namespace pawKitLib.Security.Identity;

// This DTO is not used anywhere yet and is currently provided to demonstrate DTO design principles.
// It is designed for extensibility and maintainability, following the open/closed principle, and will be used in future implementations.
// For example, a future improvement may be to support alternative email addresses. This can be achieved by storing a list of DTOs in the ExtraProperties dictionary,
// where each DTO represents a user's preferences for receiving notifications at specific email addresses. This pattern allows for flexible, user-driven extensions
// without modifying the core DTO structure.
[Obsolete("This DTO is for demonstration purposes only and should not be used in production code until integrated into the system.")]
public class UserIdentityDto<TRole> : BaseDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public List<TRole> Roles { get; set; } = [];
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastLoginAtUtc { get; set; }
}
