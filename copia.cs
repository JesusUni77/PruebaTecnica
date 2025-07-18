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
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();
                
                var query = "SELECT * FROM Cliente ORDER BY nombre";
                using var command = new SqliteCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();
                
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
                Console.WriteLine($"Error al obtener clientes: {ex.Message}");
                return StatusCode(500, "Error al obtener los clientes de la base de datos.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AgregarCliente([FromBody] Cliente cliente)
        {
            if (cliente == null)
            {
                return BadRequest(new { mensaje = "Los datos del cliente son requeridos." });
            }

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            
            // Iniciar transacción para asegurar consistencia
            using var transaction = connection.BeginTransaction();
            
            try
            {
                // Insertar cliente
                var queryCliente = @"INSERT INTO Cliente (nombre, fecha_nacimiento, sexo, ingresos)
                                   VALUES (@nombre, @fecha_nacimiento, @sexo, @ingresos);
                                   SELECT last_insert_rowid();";
                
                using var commandCliente = new SqliteCommand(queryCliente, connection, transaction);
                commandCliente.Parameters.AddWithValue("@nombre", cliente.nombre ?? (object)DBNull.Value);
                commandCliente.Parameters.AddWithValue("@fecha_nacimiento", cliente.fecha_nacimiento?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
                commandCliente.Parameters.AddWithValue("@sexo", cliente.sexo ?? (object)DBNull.Value);
                commandCliente.Parameters.AddWithValue("@ingresos", cliente.ingresos ?? (object)DBNull.Value);
                
                // Obtener el ID del cliente insertado
                var clienteIdResult = await commandCliente.ExecuteScalarAsync();
                if (clienteIdResult == null)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, new { mensaje = "Error al insertar el cliente." });
                }
                
                long clienteId = Convert.ToInt64(clienteIdResult);
                
                // Insertar cuentas si existen
                if (cliente.cuentas != null && cliente.cuentas.Count > 0)
                {
                    foreach (var cuenta in cliente.cuentas)
                    {
                        var queryCuenta = @"INSERT INTO Cuenta (numero_cuenta, saldo, cliente_id)
                                          VALUES (@numero_cuenta, @saldo, @cliente_id)";
                        
                        using var commandCuenta = new SqliteCommand(queryCuenta, connection, transaction);
                        commandCuenta.Parameters.AddWithValue("@numero_cuenta", cuenta.numero_cuenta);
                        commandCuenta.Parameters.AddWithValue("@saldo", cuenta.saldo);
                        commandCuenta.Parameters.AddWithValue("@cliente_id", clienteId);
                        
                        // EJECUTAR el comando de cuenta (esto faltaba)
                        await commandCuenta.ExecuteNonQueryAsync();
                    }
                }
                
                // Confirmar transacción
                await transaction.CommitAsync();
                
                return Ok(new { 
                    mensaje = "Cliente agregado correctamente.",
                    cliente_id = clienteId
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error al agregar cliente: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error al agregar el cliente a la base de datos." });
            }
        }
    }
}
