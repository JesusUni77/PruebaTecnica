# Prueba tecnica

!hola!
Buenas noches / buenos dias.

Antes de iniciar con el pequeno manual, me gustaria mencionarle que me disculpe
por la hora en que estoy terminando, lo que pasa es que mi laptop no tiene muy buenas
capacidad para soportar programas pesados, y se me puso bastante lenta, incluso paso como una hora sin encender, porque tuve que reinciarla. Entonces inicie tarde la prueba tecnica.

Por otro lado no me dio tiempo de poder terminar la autenticacion de token que habia
hecho, al final me falto relacionar la tabla usuario con la de cliente, la configuracion de esto esta en el archivo ```appsettings.json ahi se encuentra la configuracion para la base de datos con sqlite y el email y clave al cual le agrego
un token de autenticacion

## Como ejecutar el proyecto

1. Navegue con `cd` hasta la segunda carpeta Versalink
2. Ejecute el siguiente comando para levantar la app

``` bash
dotnet run
```
luego ponga en el navegador la siguiente url
==> http://localhost:5183/swagger/index.html

# Nota
=> le deje un archivo "loqueocupeendb.sql" ahi tengo las tablas que cree en el browser de sqlite

## Buueno este fue el package que agregue al para usar sqlite
[x] - dotnet add package Microsoft.EntityFrameworkCore.Sqlite

# Ejemplos JSON para la API

Este JSON es para agregar un cliente y sus cuentas(si acaso tiene)
<pre>
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
</pre>

En este es el post para realizar una transaccion
</pre>
{
  "monto": 100,
  "numero_cuenta": 122,
  "tipo": "Deposito"
}
</pre>

## #################### ME PASO ALGO CON LOS TEST ######################## ##

Estuve tratando de instalar XUnit, lo instale pero no me reconoce cuando lo importo.

"Comandos que habia utilizado para instalar XUnit"
[x] - dotnet add package xunit
[x] - dotnet add package xunit.runner.visualstudio
[x] - dotnet add reference ..\..\Versalink\Versalink.csproj

lo elimine porque no dejara correr el proyecto debido al error que me salia,
ya he trabajado XUnit en la universidad, aunque en pruebas unitarias basicas.