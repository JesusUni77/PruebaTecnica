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