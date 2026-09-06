namespace AdornmeStore.API.Models;

public class AdminSummaryDto
{
    public int OrdersCount { get; set; }
    public int UsersCount { get; set; }
    public int PaymentsCount { get; set; }
    public int BannersCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public int ReceivedOrders { get; set; }
    public int PendingOrders { get; set; }
    public int ShippedOrders { get; set; }
    public int DeliveredOrders { get; set; }
    public int ReturnedOrders { get; set; }
    public int CancelledOrders { get; set; }
}

public class AddressDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string UserEmail { get; set; } = string.Empty;
}
