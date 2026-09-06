using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Domain.Entities
{
    public class Profile
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string DisplayName { get; set; } = string.Empty;

        public string? Bio { get; set; }

        public string? AvatarUrl { get; set; }

        public string? PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public User User { get; set; } = null!;
    }
}
