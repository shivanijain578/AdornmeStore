using AdornmeStore.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);

        Task<AuthResponse> LoginAsync(LoginRequest request);

        Task<AuthResponse> RefreshTokenAsync(
            RefreshTokenRequest request);

        Task RevokeRefreshTokenAsync(
            RefreshTokenRequest request);
    }
}
