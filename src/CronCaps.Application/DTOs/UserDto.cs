using CronCaps.Domain.Enums;

namespace CronCaps.Application.DTOs;

public record UserDto(
    Guid Id,
    string Name,
    string Email,
    UserRole Role,
    Guid? TeamId,
    string? TeamName,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    bool IsActive
);

public record CreateUserDto(
    string Name,
    string Email,
    string Password,
    UserRole Role,
    Guid? TeamId
);

public record UpdateUserDto(
    string? Name,
    string? Email,
    UserRole? Role,
    Guid? TeamId,
    bool? IsActive
);

public record UserLoginDto(
    string Email,
    string Password
);

public record AuthenticationResultDto(
    string Token,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User
);

public record ChangePasswordDto(
    string CurrentPassword,
    string NewPassword
);

public record ResetPasswordDto(
    string Email
);

public record SetPasswordDto(
    string Token,
    string NewPassword
);