using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Versalink.Controllers.API;
using Versalink.Modelo;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;

public class TransaccionControllerF : ControllerBase
{
   public async Task<IActionResult> Transaccion(Transaccion request)
   {
      if (request.tipo == "Deposito" && request.monto > 0)
      {
         return new OkObjectResult(new { mensaje = "transaccion exitosa" });
      }
      else if (request.tipo == "Retiro" && request.monto > 0)
      {
         return new OkObjectResult(new { mensaje = "transaccion exitosa" });
      }
      return new ObjectResult("Error al realizar la transaccion.");
   }
   
}
public class ClienteControllerF : ControllerBase
{
   public async Task<IActionResult> ConsultarSaldo(int numeroCuenta)
   {
      if (numeroCuenta <= 0)
      {
         return BadRequest("numero de cuenta invalido.");
      }
      else if (numeroCuenta == 122)
      {
         return Ok(new { saldo = 1000.00 });
      }
      return new ObjectResult("transaccion exitosa");
   }
    public async Task<IActionResult> ResumenTransacciones(int numeroCuenta)
    {
        if (numeroCuenta == 122)
        {
            var transacciones = new[]
            {
               new {
                  transaccion_id = 1, fecha = DateTime.Now, tipo = "Deposito",
                  monto = 100, saldo_despues = 100
               },
               new {
                  transaccion_id = 2, fecha = DateTime.Now, tipo = "Retiro",
                  monto = 50, saldo_despues = 50
               }
            };
            return Ok(new
            {
               numero_cuenta = numeroCuenta,
               transacciones,
               saldo_final = 50
            });
        }
        return NotFound(new { mensaje = "Cuenta no encontrada." });
    }
}

public class TransaccionControllerTests
{
   [Fact]
   public async Task DepositoTest()
   {
      var controller = new TransaccionControllerF();

      var deposito = new Transaccion
      {
         numero_cuenta = 122,
         monto = 400,
         tipo = "Deposito"
      };
      var resultado = await controller.Transaccion(deposito);
      Console.WriteLine($"resultado {resultado}");
      var objectResult = Assert.IsType<OkObjectResult>(resultado);
      Assert.Contains("transaccion exitosa", objectResult.Value?.ToString());
   }
   [Fact]
   public async Task consultaSaldoTest()
   {
      var controller = new ClienteControllerF();
      var numeroCuenta = 122;
      var resultado = await controller.ConsultarSaldo(numeroCuenta);
      Console.WriteLine($"resultado {resultado}");
      var okResult = Assert.IsType<OkObjectResult>(resultado);
      dynamic value = okResult.Value;
      Assert.Equal(1000.00, (double)value.saldo);
   }
   [Fact]
    public async Task ResumenTransaccionesTest()
    {
        var controller = new ClienteControllerF();
        var numeroCuenta = 122;
        var resultado = await controller.ResumenTransacciones(numeroCuenta);
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        dynamic value = okResult.Value;
        Assert.Equal(122, (int)value.numero_cuenta);
        Assert.Equal(50, (int)value.saldo_final);
        Assert.Equal(2, ((IEnumerable<dynamic>)value.transacciones).Count());
    }
}
