using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace TicketManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Genera un token JWT simulado para propósitos de prueba de seguridad.
    /// </summary>
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Validación básica (solo para propósitos del ejercicio)
        if (string.IsNullOrWhiteSpace(request.Email)) return BadRequest("Email requerido.");

        // Asignamos el rol 'Supervisor' solo si el correo contiene la palabra admin
        var role = request.Email.Contains("admin") ? "Supervisor" : "Operador";

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, request.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "ClaveSuperSecretaDeDesarrolloMinimo32Caracteres!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "TicketSystem",
            audience: _configuration["Jwt:Audience"] ?? "TicketSystemClients",
            claims: claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: creds
        );

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            role = role
        });
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
}
