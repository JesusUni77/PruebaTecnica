using System.ComponentModel.DataAnnotations;

public class Cliente
{
   public int? cliente_id { set; get; }
   [Required]
   public string? name { set; get; }
   public DateTime date_of_birth { set; get; }
   [Required]
   public string? sexo { set; get; }
   [Required]
   public decimal? ingresos { set; get; }

   public List<Accounts> accounts { set; get; } = [];
}