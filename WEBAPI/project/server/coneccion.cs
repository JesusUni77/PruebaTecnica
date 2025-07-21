using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace project.server
{
   public class ConexionSqlite
   {
      private readonly string _connectionString = "Data Source=database.db";
      public async Task<SqliteConnection> ConnectionSqlite()
      {
         var connection = new SqliteConnection(_connectionString);
         try
         {
            await connection.OpenAsync();
            return connection;
         }
         catch (Exception ex)
         {
            Console.WriteLine($"Error al conectar la bd: {ex.Message}");
            return null!;
         }

      }
   }
}