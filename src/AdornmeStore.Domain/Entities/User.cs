using AdornmeStore.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.User;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Address> Addresses { get; set; } = new List<Address>();

        public ICollection<WishlistItem> WishlistItems { get; set; } =
            new List<WishlistItem>();

        public ICollection<CartItem> CartItems { get; set; } =
            new List<CartItem>();

        public ICollection<Order> Orders { get; set; } =
            new List<Order>();

        public ICollection<RefreshToken> RefreshTokens { get; set; }
    = new List<RefreshToken>();
    }
}
