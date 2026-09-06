using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Application.DTOs.Auth
{
    public class RegisterRequest
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        // Optional profile fields shown on user profile
        public string? PhoneNumber { get; set; }

        public string? AvatarUrl { get; set; }

        public DateTime? DateOfBirth { get; set; }
    }
}
