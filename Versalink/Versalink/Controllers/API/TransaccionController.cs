using Microsoft.AspNetCore.Mvc;
using Versalink.Modelo; 
using Microsoft.Data.Sqlite;

namespace Versalink.Controllers.API
{
   [ApiController]
   [Route("api/[controller]")]
   public class TransaccionController : ControllerBase
   {
      private readonly string _connectionString = "Data Source=database.db";
      [HttpPost("transaccion")]
      public async Task<IActionResult> Transaccion([FromBody] Transaccion request)
      {
         try
         {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var saldoQuery = "SELECT saldo FROM Cuenta WHERE numero_cuenta = @numeroCuenta";
            using var saldoCmd = new SqliteCommand(saldoQuery, connection);
            saldoCmd.Parameters.AddWithValue("@numeroCuenta", request.numero_cuenta);
            var saldoResult = await saldoCmd.ExecuteScalarAsync();

            if (saldoResult == null)
            {
               return NotFound(new { mensaje = "Cuenta no encontrada." });
            }
            /*
               Bien esta parte se que es mejorable, pero espero tome en cuenta que nunca habia usado .Net(a como le comente hoy en la entrevista), y estoy aprendiendo con este ejercicio, se que es muy mejorable si.

               Bien entonces lo que hice fue validar mediante if anidados  el monto y el tipo de deposito o retiro, bastante sencillo de entender
            */
            decimal saldo_actual = Convert.ToDecimal(saldoResult);
            if (request.tipo == "Retiro" && request.monto > saldo_actual)
            {
               return BadRequest(new { mensaje = "Saldo insuficiente para realizar el retiro." });
            }
            else if (request.tipo == "Deposito" && request.monto <= 0)
            {
               return BadRequest(new { mensaje = "El monto del depósito debe ser mayor a cero." });
            }
            else if (request.tipo != "Deposito" && request.tipo != "Retiro")
            {
               return BadRequest(new { mensaje = "Tipo de transacción inválido. Debe ser 'Deposito' o 'Retiro'." });
            }
            else if (request.tipo == "Deposito")
            {
               saldo_actual += request.monto;
            }
            else if (request.tipo == "Retiro")
            {
               saldo_actual -= request.monto;
            }
            /*
               bueno aqui solo hice una peticion a la bd para actualizar el saldo de una cuenta.
            */
            var updateSaldoQuery = "UPDATE Cuenta SET saldo = @saldo WHERE numero_cuenta = @numeroCuenta";
            using var updateSaldoCmd = new SqliteCommand(updateSaldoQuery, connection);
            updateSaldoCmd.Parameters.AddWithValue("@saldo", saldo_actual);
            updateSaldoCmd.Parameters.AddWithValue("@numeroCuenta", request.numero_cuenta);
            await updateSaldoCmd.ExecuteNonQueryAsync();

            /*
               una vez he actualizado el saldo de la cuenta, lo siguiente
               que hice fue registrar esa transaccion en la tabla Transaccion, para guardar cuando se hizo, cuanto se hizo y que tipo de transaccion fue
            */
            var insertTransaccionQuery = @"INSERT INTO Transaccion (fecha, monto, cuenta_id, numero_cuenta, tipo) 
               VALUES (@fecha, @monto, (SELECT cuenta_id FROM Cuenta WHERE numero_cuenta = @numeroCuenta), @numeroCuenta, @tipo)";
            using var insertTransaccionCmd = new SqliteCommand(insertTransaccionQuery, connection);
            insertTransaccionCmd.Parameters.AddWithValue("@fecha", DateTime.Now);
            insertTransaccionCmd.Parameters.AddWithValue("@monto", request.monto);
            insertTransaccionCmd.Parameters.AddWithValue("@numeroCuenta", request.numero_cuenta);
            insertTransaccionCmd.Parameters.AddWithValue("@tipo", request.tipo);
            await insertTransaccionCmd.ExecuteNonQueryAsync();

            return Ok(new { mensaje = "Transacción procesada con éxito." });
         }
         catch (Exception ex)
         {
            Console.WriteLine("Error al procesar la transacción.", ex);
            return StatusCode(500, "Error al procesar la transacción.");
         }
      }
      
      [HttpGet("transacciones/{numeroCuenta}")]
      public async Task<IActionResult> ResumenTransacciones(int numeroCuenta)
      {
         try
         {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            /*
               creo que no hace falta explicar aqui, solo es una consulta a la base de datos donde obtengo las transacciones mediante un numero de cuenta
            */
            var query = @"SELECT transaccion_id, fecha, monto, tipo 
                           FROM Transaccion 
                           WHERE numero_cuenta = @numeroCuenta 
                           ORDER BY fecha ASC";
            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@numeroCuenta", numeroCuenta);

            using var reader = await command.ExecuteReaderAsync();

            var transacciones = new List<object>();
            decimal saldo = 0;

            var saldoInicialQuery = "SELECT saldo FROM Cuenta WHERE numero_cuenta = @numeroCuenta";
            using var saldoCmd = new SqliteCommand(saldoInicialQuery, connection);
            saldoCmd.Parameters.AddWithValue("@numeroCuenta", numeroCuenta);
            var saldoResult = await saldoCmd.ExecuteScalarAsync();
            if (saldoResult == null)
                  return NotFound(new { mensaje = "Cuenta no encontrada." });

            var transaccionesTemp = new List<dynamic>();
            while (await reader.ReadAsync())
            {
                  transaccionesTemp.Add(new
                  {
                     transaccion_id = reader.GetInt32(reader.GetOrdinal("transaccion_id")),
                     fecha = reader.GetDateTime(reader.GetOrdinal("fecha")),
                     monto = reader.GetDecimal(reader.GetOrdinal("monto")),
                     tipo = reader.GetString(reader.GetOrdinal("tipo"))
                  });
            }
            /*
               para sacar el saldo final solo fui sumando los montos de las transacciones,
               y de esa manera obtuve el saldo final, y tambien saque el **saldo despues**, este es el saldo que tenia antes de realizar una transaccion nueva
            */
            decimal saldoActual = 0;
            foreach (var t in transaccionesTemp)
            {
                  if (t.tipo == "Deposito")
                     saldoActual += t.monto;
                  else if (t.tipo == "Retiro")
                     saldoActual -= t.monto;

                  transacciones.Add(new
                  {
                     t.transaccion_id, t.fecha,
                     t.tipo, t.monto,
                     saldo_despues = saldoActual 
                  });
            }

            return Ok(new
            {
               numero_cuenta = numeroCuenta,
               transacciones,
               saldo_final = saldoActual
            });
         }
         catch (Exception ex)
         {
            Console.WriteLine("Error al obtener el resumen de transacciones.", ex);
            return StatusCode(500, "Error al obtener el resumen de transacciones.");
         }
      }
   }
}