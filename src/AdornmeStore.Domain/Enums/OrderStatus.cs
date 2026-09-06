using System;
using System.Collections.Generic;
using System.Text;

namespace AdornmeStore.Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 1,
        Confirmed = 2,
        Shipped = 3,
        Delivered = 4,
        Cancelled = 5,
        Returned = 6
    }
}
