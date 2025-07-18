# Prueba tecnica

!hola!
Buenas noches / buenos dias.

Antes de iniciar con el pequeno manual, me gustaria mencionarle que me disculpe
por la hora en que estoy terminando, lo que pasa es que mi laptop no tiene muy buenas
capacidad para soportar programas pesados, y se me puso bastante lenta, incluso paso como una hora sin encender, porque tuve que reinciarla. Entonces inicie tarde la prueba tecnica.

Por otro lado no me dio tiempo de poder terminar la autenticacion de token que habia
hecho, al final me falto relacionar la tabla usuario con la de cliente, la configuracion de esto esta en el archivo ```appsettings.json ahi se encuentra la configuracion para la base de datos con sqlite y el email y clave al cual le agrego
un token de autenticacion

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

En este es el post para realizar una transaccion por "Deposito o Retiro"
</pre>
{
  "monto": 100,
  "numero_cuenta": 122,
  "tipo": "Deposito"
}
</pre>

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


## #################### TEST XUnit ######################## ##
Al instalar el Xunit tuve un problema con el sdk, tuve que crear un archivo global.json para indicarle la version directamente, y trabajara con la version 8.0.412 porque me estaba trabajando con la 9 no se porque si estaba usando el sdk 8

Aqui baje .NET 8.0
[x] - https://dotnet.microsoft.com/en-us/download

[x] - dotnet new xunit -n tests

en la carpeta tests del primer nivel del proyecto agregue en tests.csproj
para referenciar mi proyecto
<prev>
   <ItemGroup>
      <ProjectReference Include="../Versalink/Versalink.csproj" />
   </ItemGroup>
</prev>

```Dentro de la carpeta tests meti estas dependencias
[x] - dotnet add package xunit 
[x] - dotnet add package xunit.runner.visualstudio

para correr los tests solo hay que entrar a la siguiente ruta
==> Versalink/tests 
y ejecutar el comando `dotnet test`