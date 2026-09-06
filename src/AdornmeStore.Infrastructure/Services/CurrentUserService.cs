using AdornmeStore.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AdornmeStore.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?
            .User
            .Identity?
            .IsAuthenticated
        ?? false;

    public int UserId
    {
        get
        {
            var userId =
                _httpContextAccessor.HttpContext?
                    .User
                    .FindFirstValue(
                        ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out var id))
            {
                throw new UnauthorizedAccessException(
                    "User ID could not be determined.");
            }

            return id;
        }
    }
}