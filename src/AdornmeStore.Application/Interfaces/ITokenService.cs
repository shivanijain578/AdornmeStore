using AdornmeStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Application.Interfaces
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) GenerateAccessToken(User user);

        string GenerateRefreshToken();

        string HashRefreshToken(string token);
    }
}
