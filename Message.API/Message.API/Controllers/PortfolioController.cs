using Message.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace Message.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PortfolioController : ControllerBase
{
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _config;

    public PortfolioController(IEmailSender emailSender, IConfiguration config)
    {
        _emailSender = emailSender;
        _config = config;
    }

    [HttpPost("send-contact-form")]
    public async Task<IActionResult> SendContactForm([FromBody] ContactForm model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Usa la clave de tu configuración para obtener el correo de destino
            string recipientEmail = _config.GetSection("Owner:Email").Value;

            // El asunto del correo
            string subject = $"Nuevo mensaje desde tu Portafolio de {model.Name}";

            // El cuerpo del correo en formato HTML
            string htmlMessage =
                $"<p><strong>Nombre:</strong> {model.Name}</p>"
                + $"<p><strong>Email:</strong> {model.Email}</p>"
                + $"<p><strong>Mensaje:</strong></p>"
                + $"<p>{model.Message}</p>";

            await _emailSender.SendEmailAsync(recipientEmail, subject, htmlMessage);

            return Ok(new { message = "Correo enviado con éxito." });
        }
        catch (Exception ex)
        {
            return StatusCode(
                500,
                new { message = "Error al enviar el correo.", error = ex.Message }
            );
        }
    }
}
