using System.ComponentModel.DataAnnotations;

public class Cliente
{
   [Key]
   public int cliente_id { get; set; }

   [Required]
   [MaxLength(50)]
   public string? nombre { get; set; }

   [Required]
   public DateTime? fecha_nacimiento { get; set; }

   [Required]
   public string? sexo { get; set; }

   [Required]
   public decimal? ingresos { get; set; }

   public List<Cuenta> cuentas { get; set; } = new();
}