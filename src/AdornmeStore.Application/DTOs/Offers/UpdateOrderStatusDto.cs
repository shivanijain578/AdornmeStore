using AdornmeStore.Domain.Enums;

namespace AdornmeStore.Application.DTOs.Orders;

public class UpdateOrderStatusDto
{
    public OrderStatus Status { get; set; }
}