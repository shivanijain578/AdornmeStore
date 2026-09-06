using AdornmeStore.Application.DTOs.Auth;
using AdornmeStore.Application.Interfaces;
using AdornmeStore.Domain.Entities;
using AdornmeStore.Domain.Enums;
using AdornmeStore.Infrastructure.Data;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace AdornmeStore.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthService(
        ApplicationDbContext context,
        ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var existingUser = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == email);

        if (existingUser != null)
        {
            throw new InvalidOperationException(
                "An account with this email already exists.");
        }

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,

            PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                request.Password),

            Role = UserRole.User,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        // create initial profile record (addresses are optional and not required at registration)
        var profile = new Profile
        {
            UserId = user.Id,
            DisplayName = $"{user.FirstName} {user.LastName}".Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            AvatarUrl = request.AvatarUrl,
            DateOfBirth = request.DateOfBirth
        };

        _context.Add(profile);

        await _context.SaveChangesAsync();

        return await CreateAuthResponseAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == email);

        if (user == null)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "Your account has been deactivated.");
        }

        var passwordValid = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash);

        if (!passwordValid)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        return await CreateAuthResponseAsync(user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(
        RefreshTokenRequest request)
    {
        var tokenHash = _tokenService.HashRefreshToken(
            request.RefreshToken);

        var refreshToken = await _context.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x =>
                x.TokenHash == tokenHash);

        if (refreshToken == null ||
            !refreshToken.IsActive)
        {
            throw new UnauthorizedAccessException(
                "Invalid or expired refresh token.");
        }

        refreshToken.RevokedAt = DateTime.UtcNow;

        return await CreateAuthResponseAsync(
            refreshToken.User);
    }

    public async Task RevokeRefreshTokenAsync(
        RefreshTokenRequest request)
    {
        var tokenHash = _tokenService.HashRefreshToken(
            request.RefreshToken);

        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(x =>
                x.TokenHash == tokenHash);

        if (refreshToken != null &&
            refreshToken.IsActive)
        {
            refreshToken.RevokedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(
        User user)
    {
        var accessToken = _tokenService
            .GenerateAccessToken(user);

        var refreshToken =
            _tokenService.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,

            TokenHash =
                _tokenService.HashRefreshToken(
                    refreshToken),

            ExpiresAt =
                DateTime.UtcNow.AddDays(7),

            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshTokenEntity);

        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.ToString(),

            AccessToken = accessToken.Token,
            AccessTokenExpiresAt = accessToken.ExpiresAt,

            RefreshToken = refreshToken
        };
    }
}