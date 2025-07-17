using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Versalink.Modelo;

namespace Versalink.Controllers.Seguridad
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutorizacionController : Controller
    {
        private readonly IConfiguration _configuration;
        public AutorizacionController(IConfiguration config)
        {
            _configuration = config;
        }
        [HttpPost]
        [Route("getToken")]
        public IActionResult getToken([FromBody] Usuario request)
        {
            if (request.correo == _configuration["settings:correoAPI"] && request.clave == _configuration["settings:claveAPI"])
            {

                var keyBytes = Encoding.ASCII.GetBytes(_configuration["settings:secretKey"] ?? "");
                var claims = new ClaimsIdentity();
                claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, request.correo));

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = claims,
                    Expires = DateTime.UtcNow.AddMinutes(5),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenConfig = tokenHandler.CreateToken(tokenDescriptor);

                string tokencreado = tokenHandler.WriteToken(tokenConfig);


                return StatusCode(StatusCodes.Status200OK, new { token = tokencreado });

            }
            else
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new { token = "" });
            }
        }
    }
}
