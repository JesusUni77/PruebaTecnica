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
               clientes.Add(new Cliente
               {
                  cliente_id = reader.GetInt32(reader.GetOrdinal("cliente_id")),
                  nombre = reader.GetString(reader.GetOrdinal("nombre")),
                  fecha_nacimiento = DateTime.Parse(reader.GetString(reader.GetOrdinal("fecha_nacimiento"))),
                  sexo = reader.GetString(reader.GetOrdinal("sexo")),
                  ingresos = reader.IsDBNull(reader.GetOrdinal("ingresos")) ? null : reader.GetDecimal(reader.GetOrdinal("ingresos"))
               });
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
               var clienteID = (long)await getIdCmd.ExecuteScalarAsync();  

               //int filasAfectadas = await command.ExecuteNonQueryAsync();
               if (filasAfectadas > 0)
               {
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
    }
}