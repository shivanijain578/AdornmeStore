using AdornmeStore.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Domain.Entities
{
    public class Payment
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public decimal Amount { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public string? TransactionId { get; set; }

        public DateTime? PaidAt { get; set; }

        public Order Order { get; set; } = null!;
    }
}
