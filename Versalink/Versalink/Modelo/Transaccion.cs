public class Transaccion
{
   public int transaccion_id { get; set; }
   public DateTime fecha { get; set; }
   public decimal monto { get; set; }
   public int cuenta_id { get; set; }
   public int numero_cuenta { get; set; } 
   public string? tipo { get; set; } // "Deposito" o "Retiro"  

}