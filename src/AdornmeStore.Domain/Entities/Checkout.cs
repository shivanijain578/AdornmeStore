using System;
using System.Collections.Generic;
using System.Text;
using AdornmeStore.Domain.Enums;

namespace AdornmeStore.Domain.Entities
{
    public class Checkout
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int? PaymentId { get; set; }

        public CheckoutStatus Status { get; set; } = CheckoutStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Order Order { get; set; } = null!;

        public Payment? Payment { get; set; }
    }
}
