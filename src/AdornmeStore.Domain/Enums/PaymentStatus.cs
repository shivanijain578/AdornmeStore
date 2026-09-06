using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Domain.Enums
{
    public enum PaymentStatus
    {
        Pending = 1,
        Paid = 2,
        Failed = 3,
        Refunded = 4
    }
}
