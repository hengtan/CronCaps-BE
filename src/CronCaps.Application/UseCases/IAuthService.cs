using CronCaps.Application.DTOs;

namespace CronCaps.Application.UseCases;

public interface IAuthService
{
    Task<AuthenticationResultDto> LoginAsync(UserLoginDto loginDto, CancellationToken cancellationToken = default);
    Task<AuthenticationResultDto> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserDto> RegisterAsync(CreateUserDto createUserDto, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto, CancellationToken cancellationToken = default);
    Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto, CancellationToken cancellationToken = default);
    Task<bool> SetPasswordAsync(SetPasswordDto setPasswordDto, CancellationToken cancellationToken = default);
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
}