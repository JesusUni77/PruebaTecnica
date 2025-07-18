using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace Versalink.Server
{
    public class ConexionSqlite
    {
        private readonly IConfiguration configuration;

        public ConexionSqlite()
        {
            configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        public SqliteConnection AbrirConexion()
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection");
            SqliteConnection conexion = new SqliteConnection(connectionString);
            conexion.Open(); // Abre la conexión
            return conexion;
        }

        public void CerrarConexion(SqliteConnection conexion)
        {
            conexion.Close();
        }
    }
}
