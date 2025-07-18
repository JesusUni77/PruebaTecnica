using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

[Index(nameof(numero_cuenta), IsUnique = true)]
public class Cuenta
{
   [Key]
   public int cuenta_id { get; set; }

   public int numero_cuenta { get; set; }

   public decimal saldo { get; set; }

   public int cliente_id { get; set; }
   
   public Cliente? cliente { get; set; }

}