using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Message.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Message.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    // Endpoint para registrar un nuevo usuario
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            Name = model.Name,
            LastName = model.LastName,
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            // **PASO CLAVE**: Asignar el rol de "Cliente" por defecto a los nuevos usuarios.
            // Esta es la parte que vamos a verificar con tu prueba.
            await _userManager.AddToRoleAsync(user, "Cliente");
            return Ok(new { message = "Registro de usuario exitoso." });
        }

        return BadRequest(result.Errors);
    }

    // Endpoint para iniciar sesión
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return Unauthorized(new { message = "Usuario o contraseña incorrectos." });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

        if (result.Succeeded)
        {
            var token = await GenerateJwtToken(user);
            return Ok(new { token });
        }

        return Unauthorized(new { message = "Usuario o contraseña incorrectos." });
    }

    // Método para generar el token JWT con los roles del usuario
    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, user.Email),
        };

        // **PASO CLAVE**: Obtener todos los roles del usuario y añadirlos como claims.
        // Si el usuario no tiene roles, esta lista estará vacía y no se añadirán claims.
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// DTOs (Data Transfer Objects) para el registro
public class RegisterDto
{
    public required string Name { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}

// DTOs para el login
public class LoginDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
