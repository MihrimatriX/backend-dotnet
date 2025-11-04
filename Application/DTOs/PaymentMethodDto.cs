using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.Application.DTOs
{
    public class PaymentMethodDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string CardHolderName { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty; // Masked
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; } // Masked
        public string? AccountHolderName { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreatePaymentMethodDto
    {
        [Required(ErrorMessage = "Payment method type is required")]
        [StringLength(50, ErrorMessage = "Payment method type cannot exceed 50 characters")]
        public string Type { get; set; } = string.Empty;

        [Required(ErrorMessage = "Card holder name is required")]
        [StringLength(100, ErrorMessage = "Card holder name cannot exceed 100 characters")]
        public string CardHolderName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Card number is required")]
        [StringLength(20, ErrorMessage = "Card number cannot exceed 20 characters")]
        public string CardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Expiry month is required")]
        [Range(1, 12, ErrorMessage = "Expiry month must be between 1 and 12")]
        public int ExpiryMonth { get; set; }

        [Required(ErrorMessage = "Expiry year is required")]
        [Range(2024, 2050, ErrorMessage = "Expiry year must be between 2024 and 2050")]
        public int ExpiryYear { get; set; }

        [StringLength(10, ErrorMessage = "CVV cannot exceed 10 characters")]
        public string? Cvv { get; set; }

        [StringLength(100, ErrorMessage = "Bank name cannot exceed 100 characters")]
        public string? BankName { get; set; }

        [StringLength(50, ErrorMessage = "Account number cannot exceed 50 characters")]
        public string? AccountNumber { get; set; }

        [StringLength(100, ErrorMessage = "Account holder name cannot exceed 100 characters")]
        public string? AccountHolderName { get; set; }

        public bool IsDefault { get; set; } = false;
    }

    public class UpdatePaymentMethodDto
    {
        [Required(ErrorMessage = "Payment method type is required")]
        [StringLength(50, ErrorMessage = "Payment method type cannot exceed 50 characters")]
        public string Type { get; set; } = string.Empty;

        [Required(ErrorMessage = "Card holder name is required")]
        [StringLength(100, ErrorMessage = "Card holder name cannot exceed 100 characters")]
        public string CardHolderName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Card number is required")]
        [StringLength(20, ErrorMessage = "Card number cannot exceed 20 characters")]
        public string CardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Expiry month is required")]
        [Range(1, 12, ErrorMessage = "Expiry month must be between 1 and 12")]
        public int ExpiryMonth { get; set; }

        [Required(ErrorMessage = "Expiry year is required")]
        [Range(2024, 2050, ErrorMessage = "Expiry year must be between 2024 and 2050")]
        public int ExpiryYear { get; set; }

        [StringLength(10, ErrorMessage = "CVV cannot exceed 10 characters")]
        public string? Cvv { get; set; }

        [StringLength(100, ErrorMessage = "Bank name cannot exceed 100 characters")]
        public string? BankName { get; set; }

        [StringLength(50, ErrorMessage = "Account number cannot exceed 50 characters")]
        public string? AccountNumber { get; set; }

        [StringLength(100, ErrorMessage = "Account holder name cannot exceed 100 characters")]
        public string? AccountHolderName { get; set; }

        public bool IsDefault { get; set; }
    }
}
