using Microsoft.AspNetCore.Mvc;
using Versalink.Modelo; 
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
         
    }
}