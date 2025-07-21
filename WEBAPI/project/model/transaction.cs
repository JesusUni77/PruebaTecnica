public class Transaction
{
   public DateTime fecha { set; get; }
   public decimal amount { set; get; }
   public string? number_account { set; get; }
   public int cuenta_fk { set; get; }
   public string? typeTransaction { set; get; }
}