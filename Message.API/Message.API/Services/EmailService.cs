using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

public class EmailService : IEmailSender
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlMessage)
    {
        var smtp = _config.GetSection("Smtp");

        Console.WriteLine($"📤 SMTP Host: {smtp["Host"]}");
        Console.WriteLine($"📤 SMTP Username: {smtp["Username"]}");

        var client = new SmtpClient(smtp["Host"], int.Parse(smtp["Port"]))
        {
            Credentials = new NetworkCredential(smtp["Username"], smtp["Password"]),
            EnableSsl = true,
        };

        var mail = new MailMessage
        {
            From = new MailAddress(smtp["Username"], "Massage App"),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true,
        };

        mail.To.Add(to);

        try
        {
            await client.SendMailAsync(mail);
            Console.WriteLine(" Correo enviado correctamente a " + to);
        }
        catch (Exception ex)
        {
            Console.WriteLine(" Error al enviar correo: " + ex.Message);
            throw;
        }
    }
}
