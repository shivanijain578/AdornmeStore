namespace AdornmeStore.API.Models;

using System.ComponentModel.DataAnnotations;

public class CreateAddressDto
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public string AddressLine1 { get; set; } = string.Empty;

    public string? AddressLine2 { get; set; }

    [Required]
    public string City { get; set; } = string.Empty;

    [Required]
    public string State { get; set; } = string.Empty;

    [Required]
    public string PostalCode { get; set; } = string.Empty;

    public bool IsDefault { get; set; }
}

public class CheckoutRequestDto
{
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public int? PaymentMethodId { get; set; }
    public int AddressId { get; set; }
}

public class CheckoutResultDto
{
    public bool Success { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class ProfileDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<AddressDto> Addresses { get; set; } = new();
}
