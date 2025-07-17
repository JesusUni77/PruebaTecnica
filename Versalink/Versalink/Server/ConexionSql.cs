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

        /* public async Task<List<Dictionary<string, object>>> EjecutarConsultaAsync(string query)
        {
            var resultados = new List<Dictionary<string, object>>();

            using (var conexion = AbrirConexion())
            using (var comando = new SqliteCommand(query, conexion))
            using (var reader = await comando.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var fila = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        fila[reader.GetName(i)] = reader.GetValue(i);
                    }
                    resultados.Add(fila);
                }
            }

            return resultados;
        } */
    }
}
