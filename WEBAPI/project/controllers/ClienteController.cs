using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using project.server;

namespace project.controllers {
   [ApiController]
   [Route("api/[controller]")]
   public class ClienteController : ControllerBase
   {
      private string ValidateFieldsClients(Cliente cliente)
      {
         if (string.IsNullOrEmpty(cliente.name) || string.IsNullOrEmpty(cliente.sexo) || cliente.name.Equals("string") || !cliente.ingresos.HasValue)
            return "Todos los campos son requeridos";
         else if (cliente.sexo != "F" && cliente.sexo != "M")
            return "El sexo debe ser F o M";

         if (cliente.accounts.Count > 0)
         {
            foreach (var account in cliente.accounts)
            {
               if (string.IsNullOrEmpty(account.account_number) || !account.saldo.HasValue)
                  return "Todos los campos de la cuenta son requeridos";

               if (account.saldo < 100)
                  return "El saldo tiene que como minimo 100";
            }
         }

         return "";
      }
      private readonly string queryRegisterClient = @"
         INSERT INTO Clientes (name, data_birth, sexo, ingresos) 
         VALUES (@name, @data_birth, @sexo, @ingresos)";
      private readonly string queryRegisterAccount = @"
         INSERT INTO Accounts (account_number, saldo, create_at, cliente_fk) 
         VALUES (@account_number, @saldo, @create_at, @cliente_fk)";

      [HttpPost]
      public async Task<IActionResult> CrearCliente([FromBody] Cliente cliente)
      {
         string message = ValidateFieldsClients(cliente);
         if (message != "")
            return StatusCode(500, message);
         try
         {
            ConexionSqlite conexionSqlite = new();
            SqliteConnection cnx = await conexionSqlite.ConnectionSqlite();

            if (cnx.State == ConnectionState.Closed)
               throw new Exception("Error de conexion de la bd");

            try
            {
               using var commandClient = new SqliteCommand(queryRegisterClient, cnx);
               commandClient.Parameters.AddWithValue("@name", cliente.name);
               commandClient.Parameters.AddWithValue("@data_birth", cliente.date_of_birth);
               commandClient.Parameters.AddWithValue("@sexo", cliente.sexo);
               commandClient.Parameters.AddWithValue("@ingresos", cliente.ingresos);

               int affectedRow = await commandClient.ExecuteNonQueryAsync();
               using var getIdCmd = new SqliteCommand("SELECT last_insert_rowid();", cnx);
               var cliente_id = (long?)await getIdCmd.ExecuteScalarAsync();
               if (cliente_id == null)
                  throw new Exception("Error en cliente_id");

               if (affectedRow > 0 && cliente_id != null)
               {
                  foreach (var ac in cliente.accounts)
                  {
                     using var commandAccount = new SqliteCommand(queryRegisterAccount, cnx);
                     if (commandAccount == null)
                        return StatusCode(500, "Ha ocurrido un error al crear el commandAccount");
                     commandAccount.Parameters.AddWithValue("@account_number", ac.account_number);
                     commandAccount.Parameters.AddWithValue("@saldo", ac.saldo);
                     commandAccount.Parameters.AddWithValue("@create_at", DateTime.Now);
                     commandAccount.Parameters.AddWithValue("@cliente_fk", cliente_id);

                     await commandAccount.ExecuteNonQueryAsync();
                  }
               }
            }
            catch (SqliteException sqlex)
            {
               return StatusCode(400, $"Error al registrar el cliente: {sqlex}");
            }

            Console.WriteLine($"name: {cliente.name} dateBirth: {cliente.date_of_birth} sexo: {cliente.sexo} ingresos: {cliente.ingresos}");

            await Task.Delay(100);
            return Ok("Cliente creado correctamente");
         }
         catch (Exception ex)
         {
            return StatusCode(500, $"Error 500: {ex.Message}");
         }
      }

      private readonly string queryShowClients = @"SELECT *FROM Clientes ORDER BY cliente_id DESC;";
      private readonly string queryAccountsClient = @"SELECT *FROM Accounts WHERE cliente_fk = @cliente_fk;";
      [HttpGet]
      public async Task<IActionResult> MostrarClientes()
      {
         try
         {
            ConexionSqlite conexionSqlite = new();
            SqliteConnection cnx = await conexionSqlite.ConnectionSqlite();
            await cnx.OpenAsync();

            var lista_clientes = new List<Cliente>();
            try
            {
               using var commandClient = new SqliteCommand(queryShowClients, cnx);
               if (commandClient == null)
                  return StatusCode(500, "Error al crear el commandClient");

               using var reader = await commandClient.ExecuteReaderAsync();
               while (await reader.ReadAsync())
               {
                  var cliente = new Cliente
                  {
                     cliente_id = int.Parse(reader.GetString("cliente_id")),
                     name = reader.GetString("name"),
                     date_of_birth = DateTime.Parse(reader.GetString("data_birth")),
                     sexo = reader.GetString("sexo"),
                     ingresos = Decimal.Parse(reader.GetString("ingresos")),
                  };
                  using var accountsCmd = new SqliteCommand(queryAccountsClient, cnx);
                  accountsCmd.Parameters.AddWithValue(@"cliente_fk", cliente.cliente_id);
                  using var accountsReader = await accountsCmd.ExecuteReaderAsync();
                  while (await accountsReader.ReadAsync())
                  {
                     cliente.accounts.Add(new Accounts
                     {
                        account_number = accountsReader.GetString("account_number"),
                        create_at = DateTime.Parse(accountsReader.GetString("create_at")),
                        saldo = Decimal.Parse(accountsReader.GetString("saldo"))
                     });
                  }

                  lista_clientes.Add(cliente);
               }

            }
            catch (Exception ex)
            {
               return StatusCode(500, $"Error: {ex.Message}");
            }

            if (cnx.State == ConnectionState.Closed)
               return StatusCode(500, "Error de conexion de la base de datos");

            return Ok(lista_clientes);
         }
         catch (Exception ex)
         {
            return StatusCode(500, $"Error al mostrar los clientes: {ex.Message}");
         }
      }

      private readonly string queryFilterByNA = @"SELECT saldo FROM Accounts WHERE account_number=@cliente_fk;";
      [HttpGet("filtrar/saldo/{numberAccount}")]
      public async Task<IActionResult> saldoByNumberAccount(string numberAccount)
      {
         try
         {
            ConexionSqlite connection = new ConexionSqlite();
            SqliteConnection cnx = await connection.ConnectionSqlite();
            await cnx.OpenAsync();

            using var saldoCmd = new SqliteCommand(queryFilterByNA, cnx);
            saldoCmd.Parameters.AddWithValue(@"cliente_fk", numberAccount);
            var result = await saldoCmd.ExecuteScalarAsync();

            if (result != null)
            {
               decimal saldo = Convert.ToDecimal(result);
               return Ok(new { number_account = numberAccount, saldo });
            }
            else
            {
               return NotFound("Cuenta no encontrada");
            }

         } catch (Exception ex) {
            return StatusCode(500, $"Error: {ex.Message}");
         }
      }
   }
}