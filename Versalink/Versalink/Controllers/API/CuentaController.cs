using Microsoft.AspNetCore.Mvc;
using Versalink.Modelo;
using Microsoft.Data.Sqlite;

namespace Versalink.Controllers.API
{
   [ApiController]
   [Route("api/[controller]")]
   public class CuentaController : ControllerBase
   {
      private readonly string _connectionString = "Data Source=database.db";
      [HttpPost]
      public async Task<IActionResult> CrearCuenta([FromBody] Cuenta cuenta) {
         try
         {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"INSERT INTO Cuenta (numero_cuenta, saldo, cliente_id) 
                          VALUES (@numero_cuenta, @saldo, @cliente_id)";
            return Ok("Cuenta creada exitosamente.");
         }
         catch (Exception ex)
         {
            return StatusCode(500, "Error al crear la cuenta: " + ex.Message);
         }
      }
   }
}