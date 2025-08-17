using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Web;
using Message.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
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
    private readonly IEmailSender _emailSender;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        IEmailSender emailSender
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _emailSender = emailSender;
    }

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
            EmailConfirmed = true,
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        await _userManager.AddToRoleAsync(user, "Cliente");

        return Ok(new { message = "Registro exitoso. Ya puedes iniciar sesión." });
    }

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

        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            return Unauthorized(
                new { message = "Debes confirmar tu correo antes de iniciar sesión." }
            );
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

        if (result.Succeeded)
        {
            var token = await GenerateJwtToken(user);
            return Ok(new { token });
        }

        return Unauthorized(new { message = "Usuario o contraseña incorrectos." });
    }

    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, user.Email),
        };

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

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            return Ok(new { message = "Email confirmado correctamente." });
        }

        return BadRequest(result.Errors);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
        {
            return BadRequest("Usuario no encontrado o email no confirmado.");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetUrlBase = _configuration["Frontend:ResetPasswordUrl"];
        var resetLink = $"{resetUrlBase}?email={dto.Email}&token={WebUtility.UrlEncode(token)}";

        await _emailSender.SendEmailAsync(
            dto.Email,
            "Recuperar contraseña",
            $"<p>Hola {user.Name},</p><p>Haz clic <a href='{resetLink}'>aquí</a> para cambiar tu contraseña.</p>"
        );

        return Ok(new { message = "Correo de recuperación enviado." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return NotFound();

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (result.Succeeded)
        {
            return Ok(new { message = "Contraseña actualizada correctamente." });
        }

        return BadRequest(result.Errors);
    }
}

public class RegisterDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    public required string LastName { get; set; }

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    public required string Password { get; set; }
}

public class LoginDto
{
    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    public required string Password { get; set; }
}

public class ForgotPasswordDto
{
    public required string Email { get; set; }
}

public class ResetPasswordDto
{
    public required string Email { get; set; }
    public required string Token { get; set; }
    public required string NewPassword { get; set; }
}
