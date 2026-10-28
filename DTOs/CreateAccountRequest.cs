
using System.ComponentModel.DataAnnotations;

namespace BankingLedgerApi.DTOs;

public class CreateAccountRequest
{
    [Required]
    public string CustomerId { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(Savings|Current)$", ErrorMessage = "Account type must be Savings or Current.")]
    public string AccountType { get; set; } = "Savings";

    [Range(typeof(decimal), "0", "1000000000")]
    public decimal InitialDeposit { get; set; } = 0m;
}
