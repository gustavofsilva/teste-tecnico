using System.ComponentModel.DataAnnotations;

namespace UserProfile.Api.Contracts;

public sealed class RegisterRequest
{
    [Required, MinLength(3), MaxLength(120)] public required string Name { get; init; }
    [Required, EmailAddress, MaxLength(254)] public required string Email { get; init; }
    [Required, MinLength(6), MaxLength(128)] public required string Password { get; init; }
    [Required] public required string ConfirmPassword { get; init; }
}

public sealed class LoginRequest
{
    [Required, EmailAddress, MaxLength(254)] public required string Email { get; init; }
    [Required, MaxLength(128)] public required string Password { get; init; }
}

public sealed class UpdateProfileRequest
{
    [Required, MinLength(3), MaxLength(120)] public required string Name { get; init; }
    [Required, EmailAddress, MaxLength(254)] public required string Email { get; init; }
    [MaxLength(128)] public string? Password { get; init; }
    public string? ConfirmPassword { get; init; }
}

public sealed record UserResponse(Guid Id, string Name, string Email);
public sealed record AuthResponse(string Token, UserResponse User);
public sealed record MessageResponse(string Message);
