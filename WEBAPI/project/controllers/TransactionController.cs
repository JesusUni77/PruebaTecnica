using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using project.server;
using System.Data;

namespace project.controllers
{
   [ApiController]
   [Route("api/[controller]")]
   public class TransactionController : ControllerBase
   {
      private string ValidateFieldsTransaction(string type, decimal amount, decimal saldo_actual)
      {
         if (type == "Retiro" && amount > saldo_actual)
         {
            return "Saldo insuficiente para realizar el retiro.";
         }
         else if (type == "Deposito" && amount <= 0)
         {
            return "El monto del depósito debe ser mayor a cero.";
         }
         else if (type != "Deposito" && type != "Retiro")
         {
            return "Tipo de transacción inválido. Debe ser 'Deposito' o 'Retiro'.";
         }
         return "";
      }
      private readonly string queryMakeTransaction = @"
         INSERT INTO Transaccion (fecha, amount, cuenta_fk, number_acount, typeTransaction) 
         VALUES (@fecha, @amount, (SELECT account_id FROM Accounts WHERE account_number= @accountNumber), @accountNumber, @typeTransaction)";

      private readonly string queryUpdateSaldo = "UPDATE Accounts SET saldo = @saldo WHERE account_number = @accountNumber";
      private readonly string queryGetSaldo = "SELECT saldo FROM Accounts WHERE account_number = @accountNumber";
       [HttpPost("transacciones")]
      public async Task<IActionResult> makeTransaction([FromBody] Transaction ts)
      {
         try
         {
            ConexionSqlite connection = new ConexionSqlite();
            SqliteConnection cnx = await connection.ConnectionSqlite();
            if (cnx.State == ConnectionState.Closed)
               return StatusCode(500, "Error en la conexion de la base de datos");

            using var saldoCmd = new SqliteCommand(queryGetSaldo, cnx);
            saldoCmd.Parameters.AddWithValue(@"accountNumber", ts.number_account);
            var result = await saldoCmd.ExecuteScalarAsync();
            
            if (result == null)
               return NotFound(new { mensaje = "Cuenta no encontrada." });

            decimal saldo_actual = Convert.ToDecimal(result);
            string message = ValidateFieldsTransaction(ts.typeTransaction ?? "", ts.amount, saldo_actual);
            if(message != "")
               return StatusCode(500, message); 

            if (ts.typeTransaction == "Deposito")
                  saldo_actual += ts.amount;
            if(ts.typeTransaction == "Retiro")
               saldo_actual -= ts.amount;

            using var updateSaldoCmd = new SqliteCommand(queryUpdateSaldo, cnx);
            updateSaldoCmd.Parameters.AddWithValue(@"saldo", saldo_actual);
            updateSaldoCmd.Parameters.AddWithValue(@"accountNumber", ts.number_account);
            await updateSaldoCmd.ExecuteNonQueryAsync();

            try
            {
               using var tscmd = new SqliteCommand(queryMakeTransaction, cnx);
               tscmd.Parameters.AddWithValue(@"accountNumber", ts.number_account);
               tscmd.Parameters.AddWithValue(@"fecha", DateTime.Now);
               tscmd.Parameters.AddWithValue(@"amount", ts.amount);
               tscmd.Parameters.AddWithValue(@"cuenta_fk", ts.cuenta_fk);
               tscmd.Parameters.AddWithValue(@"typeTransaction", ts.typeTransaction);

               await tscmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
               return StatusCode(500, $"Error: {ex.Message}");
            }

            return Ok("Transaccion exitosa.");
         }
         catch (Exception ex)
         {
            return StatusCode(500, $"Erro: {ex.Message}");
         }
      }
   }
}