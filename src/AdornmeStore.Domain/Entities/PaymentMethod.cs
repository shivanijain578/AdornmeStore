using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Domain.Entities
{
    public class PaymentMethod
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string CardHolderName { get; set; } = string.Empty;

        public string CardLast4 { get; set; } = string.Empty;

        public string CardType { get; set; } = string.Empty;

        public int ExpiryMonth { get; set; }

        public int ExpiryYear { get; set; }

        public bool IsDefault { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
    }
}
