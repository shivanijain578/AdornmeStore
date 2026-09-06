using AdornmeStore.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int AddressId { get; set; }

        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;

        public Address Address { get; set; } = null!;

        public ICollection<OrderItem> Items { get; set; } =
            new List<OrderItem>();

        public Payment? Payment { get; set; }
    }
}
