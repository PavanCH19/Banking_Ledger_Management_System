
using System.ComponentModel.DataAnnotations;

namespace BankingLedgerApi.DTOs;

public class CreateCustomerRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone must contain exactly 10 digits.")]
    public string Phone { get; set; } = string.Empty;
}
