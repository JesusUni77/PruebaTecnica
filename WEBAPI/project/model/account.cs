using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

[Index(nameof(account_number), IsUnique = true)]
public class Accounts
{
   [Required]
   public string? account_number { set; get; }
   [Required]
   public decimal? saldo { set; get; }
   public DateTime create_at { set; get; }
}