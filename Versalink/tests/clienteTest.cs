using Xunit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System;
using Versalink.Controllers.API;

public class ClienteTest
{
   [Fact]
   public async Task agregarClienteTest()
   {
      var controller = new ClienteController();
      var nuevoCliente = new Cliente
      {
         nombre = "jesus silva",
         fecha_nacimiento = DateTime.Parse("2003-08-08"),
         sexo = "M",
         ingresos = 500,
         cuentas = new System.Collections.Generic.List<Cuenta>()
      };

      var result = await controller.AgregarCliente(nuevoCliente);
      Console.WriteLine($"resultado {result}");
      var objectResult = Assert.IsType<ObjectResult>(result);
      Console.WriteLine($"controlador {objectResult.Value}");
   }
}
