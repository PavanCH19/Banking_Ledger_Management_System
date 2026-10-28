
using System.ComponentModel.DataAnnotations;

namespace BankingLedgerApi.DTOs;

public class DepositRequest
{
    [Range(typeof(decimal), "0.01", "1000000000")]
    public decimal Amount { get; set; }
}
