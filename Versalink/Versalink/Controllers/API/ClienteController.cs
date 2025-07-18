using Microsoft.AspNetCore.Mvc;
using Versalink.Modelo; // Ajusta el namespace si es diferente
using Microsoft.Data.Sqlite;

namespace Versalink.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClienteController : ControllerBase
    {
        private readonly string _connectionString = "Data Source=database.db";
      [HttpGet]
      public async Task<IActionResult> GetClientes()
      {
         try
         {
            var clientes = new List<Cliente>();
            using var connecion = new SqliteConnection(_connectionString);
            await connecion.OpenAsync();

            var query = "SELECT * FROM Cliente ORDER BY nombre";
            using var command = new SqliteCommand(query, connecion);
            using var reader = await command.ExecuteReaderAsync();
            //
            while (await reader.ReadAsync())
            {
               var cliente = new Cliente
               {
                  cliente_id = reader.GetInt32(reader.GetOrdinal("cliente_id")),
                  nombre = reader.GetString(reader.GetOrdinal("nombre")),
                  fecha_nacimiento = DateTime.Parse(reader.GetString(reader.GetOrdinal("fecha_nacimiento"))),
                  sexo = reader.GetString(reader.GetOrdinal("sexo")),
                  ingresos = reader.IsDBNull(reader.GetOrdinal("ingresos")) ? null : reader.GetDecimal(reader.GetOrdinal("ingresos")),
                  cuentas = new List<Cuenta>()
               };
               var cuentasQuery = "SELECT * FROM Cuenta WHERE ClienteId = @clienteId";
               using var cuentasCmd = new SqliteCommand(cuentasQuery, connecion);
               cuentasCmd.Parameters.AddWithValue("@clienteId", cliente.cliente_id);
               using var cuentasReader = await cuentasCmd.ExecuteReaderAsync();
               while (await cuentasReader.ReadAsync())
               {
                  cliente.cuentas.Add(new Cuenta
                  {
                     cuenta_id = cuentasReader.GetInt32(cuentasReader.GetOrdinal("cuenta_id")),
                     numero_cuenta = cuentasReader.GetInt32(cuentasReader.GetOrdinal("numero_cuenta")),
                     saldo = cuentasReader.GetDecimal(cuentasReader.GetOrdinal("saldo")),
                     cliente_id = cuentasReader.GetInt32(cuentasReader.GetOrdinal("ClienteId"))
                  });
               }

               clientes.Add(cliente);
            }
            return Ok(clientes);
         }
         catch (Exception ex)
         {
            Console.WriteLine("Error al abrir la conexión a la base de datos.", ex);
            return StatusCode(500, "Error al abrir la conexión a la base de datos.");
         }

        }
        
        [HttpPost]
        public async Task<IActionResult> AgregarCliente([FromBody] Cliente cliente)
        {
            try
            {
               using var connection = new SqliteConnection(_connectionString);
               await connection.OpenAsync();
               var query = @"INSERT INTO Cliente (nombre, fecha_nacimiento, sexo, ingresos) 
                     VALUES (@nombre, @fecha_nacimiento, @sexo, @ingresos)";
               using var command = new SqliteCommand(query, connection);
               command.Parameters.AddWithValue("@nombre", cliente.nombre);
               command.Parameters.AddWithValue("@fecha_nacimiento", (object?)cliente.fecha_nacimiento ?? DBNull.Value);
               command.Parameters.AddWithValue("@sexo", cliente.sexo);
               command.Parameters.AddWithValue("@ingresos", cliente.ingresos);
               
               //await command.ExecuteNonQueryAsync();

               int filasAfectadas = await command.ExecuteNonQueryAsync(); // Solo una vez
               using var getIdCmd = new SqliteCommand("SELECT last_insert_rowid();", connection);
               /*
                  Aqui pase investigando como agarraba el id del cliente que se acababa de agregar, es la unica forma que se me vino para poder obtener el id del cliente y asi relacionarlos con las cuentas que llegara a tener
               */
               var clienteID = (long)await getIdCmd.ExecuteScalarAsync();  

               //int filasAfectadas = await command.ExecuteNonQueryAsync();
               if (filasAfectadas > 0)
               {  
                  /*
                     de todos los incisos a realizar este fue el que mas me costo ya que no conocia la sintaxis, fue uno de los primeros que hice, la logica la tenia clara, solo era poder implementarla. Entonces lo que hice fue recorrer la lista de cuentas del cliente (si en caso se agregaron cuentas).
                  */
                  if (cliente.cuentas != null && cliente.cuentas.Count > 0)
               {
                  foreach (var cuenta in cliente.cuentas)
                  {
                     var queryCuenta = @"INSERT INTO Cuenta (numero_cuenta, saldo, ClienteId) 
                              VALUES (@numero_cuenta, @saldo, @cliente_id)";
                     //
                     using var commandCuenta = new SqliteCommand(queryCuenta, connection);
                     //
                     commandCuenta.Parameters.AddWithValue("@numero_cuenta", cuenta.numero_cuenta);
                     commandCuenta.Parameters.AddWithValue("@saldo", cuenta.saldo);
                     commandCuenta.Parameters.AddWithValue("@cliente_id", clienteID);
                     await commandCuenta.ExecuteNonQueryAsync();
                  }//

               }
                  // Cliente agregado correctamente 
                  Console.WriteLine($"Cliente agregado con ID: {clienteID}");
                  return Ok(new { mensaje = "Cliente agregado correctamente." });
               }
               else
               {
                  return BadRequest(new { mensaje = "No se pudo agregar el cliente." });
               }
            }
            catch (Exception ex)
            {
               Console.WriteLine("Error al abrir la conexión a la base de datos.", ex);
               return StatusCode(500, "Error al abrir la conexión a la base de datos.");
            }
            
        }
        [HttpGet("saldo/{numeroCuenta}")]
         public async Task<IActionResult> ConsultarSaldo(int numeroCuenta)
         {
            /*
               Bueno aqui fue una consulta bien basica, solamente hice un where con el numero de cuenta para obtener el saldo de la cuenta, al final solo era consultar el saldo
            */
            try
         {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT saldo FROM Cuenta WHERE numero_cuenta = @numeroCuenta";
            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@numeroCuenta", numeroCuenta);

            var result = await command.ExecuteScalarAsync();

            if (result != null)
            {
               decimal saldo = Convert.ToDecimal(result);
               return Ok(new { numero_cuenta = numeroCuenta, saldo });
            }
            else
            {
               return NotFound(new { mensaje = "Cuenta no encontrada." });
            }
         }
         catch (Exception ex)
         {
            Console.WriteLine("Error al consultar el saldo.", ex);
            return StatusCode(500, "Error al consultar el saldo.");
         }
         }
         [HttpPost("transaccion")]
         public async Task<IActionResult> Transaccion([FromBody] Transaccion request) {
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