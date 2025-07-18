# linkapi
API net core con SQL Server para sistema SmartStock

## Agregamos este paquete para usar sqlite
[x] - dotnet add package Microsoft.EntityFrameworkCore.Sqlite


{
  "ConnectionStrings": {
    "DefaultConnection": "Server=db14287.public.databaseasp.net; Database=db14287; User Id=db14287; Password=Y@p65hK=_S4n; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "settings": {
    "secretKey": "oPV7v3I2pLQfKgY1MTIzNDU2Nzg5MGFiY2RlZjA5OQ==",
    "correoAPI": "jesus@gmail.com",
    "claveAPI": "1234"
  }
}
#######################

var command = connection.CreateCommand();
               command.CommandText = @"INSERT INTO Cliente (nombre, fecha_nacimiento, sexo, ingresos) VALUES (@nombre, @fecha_nacimiento, @sexo, @ingresos)";
               command.Parameters.AddWithValue("@nombre", (object?)cliente.nombre ?? DBNull.Value);
               command.Parameters.AddWithValue("@fecha_nacimiento", (object?)cliente.fecha_nacimiento ?? DBNull.Value);
               command.Parameters.AddWithValue("@sexo", (object?)cliente.sexo ?? DBNull.Value);
               command.Parameters.AddWithValue("@ingresos", (object?)cliente.ingresos ?? DBNull.Value);
               int result = command.ExecuteNonQuery();
               if (result > 0)
               {
                  return Ok(new { mensaje = "Cliente agregado correctamente" });
               }
               else
               {
                  return BadRequest(new { mensaje = "No se pudo agregar el cliente" });
                }

connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT cliente_id, nombre, fecha_nacimiento, sexo, ingresos FROM Cliente";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clientes.Add(new Cliente
                        {
                            cliente_id = reader.GetInt32(0),
                            nombre = reader.IsDBNull(1) ? null : reader.GetString(1),
                            fecha_nacimiento = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2),
                            sexo = reader.IsDBNull(3) ? null : reader.GetString(3),
                            ingresos = reader.IsDBNull(4) ? (decimal?)null : reader.GetDecimal(4)
                        });
                    }
                }



{
  "nombre": "jesus",
  "fecha_nacimiento": "2025-07-18T00:32:41.188Z",
  "sexo": "M",
  "ingresos": 0,
  "cuentas": [
    {
      "numero_cuenta": 122,
      "saldo": 100
    }
  ]
}

{
  "fecha": "2025-07-18T02:19:57.868Z",
  "monto": 100,
  "numero_cuenta": 122,
  "tipo": "Deposito"
}