using System.Security.Claims;
using Message.API.Infrastructure.Data;
using Message.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Message.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Proteger todos los endpoints del controlador por defecto
public class AppointmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AppointmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Appointments
    // Solo accesible para administradores
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAppointments()
    {
        var appointments = await _context
            .Appointments.Include(a => a.Client)
            .Include(b => b.Treatment)
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                Description = a.Description,
                AppointmentDate = a.AppointmentDate,
                ClientId = a.ClientId,
                ClientName = $"{a.Client!.Name} {a.Client.LastName}",
                TreatmentId = a.TreatmentId,
                TreatmentTitle = a.Treatment!.Title,
            })
            .ToListAsync();

        return Ok(appointments);
    }

    // NUEVO: GET: api/Appointments/my-appointments
    // Permite al cliente ver solo sus propios turnos
    [HttpGet("my-appointments")]
    [Authorize(Roles = "Cliente")]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetMyAppointments()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdString == null)
        {
            return Unauthorized();
        }

        var clientAppointments = await _context
            .Appointments.Include(a => a.Client)
            .Include(b => b.Treatment)
            .Where(a => a.ClientId == Guid.Parse(userIdString))
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                Description = a.Description,
                AppointmentDate = a.AppointmentDate,
                ClientId = a.ClientId,
                ClientName = $"{a.Client!.Name} {a.Client.LastName}",
                TreatmentId = a.TreatmentId,
                TreatmentTitle = a.Treatment!.Title,
            })
            .ToListAsync();

        return Ok(clientAppointments);
    }

    // GET: api/Appointments/{id}
    // Este endpoint puede ser usado por la dueña o el cliente de ese turno
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<AppointmentDto>> GetAppointment(Guid id)
    {
        var appointment = await _context
            .Appointments.Include(a => a.Client)
            .Include(b => b.Treatment)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null)
        {
            return NotFound();
        }

        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!User.IsInRole("Admin") && Guid.Parse(userIdString) != appointment.ClientId)
        {
            return Forbid();
        }

        var appointmentDto = new AppointmentDto
        {
            Id = appointment.Id,
            Description = appointment.Description,
            AppointmentDate = appointment.AppointmentDate,
            ClientId = appointment.ClientId,
            ClientName = $"{appointment.Client!.Name} {appointment.Client.LastName}",
            TreatmentId = appointment.TreatmentId,
            TreatmentTitle = appointment.Treatment!.Title,
        };

        return Ok(appointmentDto);
    }

    // POST: api/Appointments
    // Solo un cliente puede crear un turno para sí mismo
    [HttpPost]
    [Authorize(Roles = "Cliente")]
    public async Task<ActionResult<Appointment>> PostAppointment(
        AppointmentCreateDto appointmentDto
    )
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }

        var treatment = await _context.Treatments.FindAsync(appointmentDto.TreatmentId);
        if (treatment == null)
        {
            return BadRequest("TreatmentId provided is not valid.");
        }

        var appointment = new Appointment
        {
            Description = appointmentDto.Description,
            AppointmentDate = appointmentDto.AppointmentDate.ToUniversalTime().AddHours(1),
            ClientId = Guid.Parse(userId),
            TreatmentId = appointmentDto.TreatmentId,
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, appointment);
    }

    // NUEVO: PUT: api/Appointments/{id}
    // Permite a un cliente actualizar su propio turno
    [HttpPut("{id}")]
    [Authorize(Roles = "Cliente")]
    public async Task<IActionResult> PutAppointment(Guid id, AppointmentUpdateDto appointmentDto)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdString == null)
        {
            return Unauthorized();
        }

        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null)
        {
            return NotFound();
        }

        // Validar que el usuario que intenta actualizar es el dueño del turno
        if (appointment.ClientId != Guid.Parse(userIdString))
        {
            return Forbid();
        }

        appointment.Description = appointmentDto.Description;
        appointment.AppointmentDate = appointmentDto.AppointmentDate.ToUniversalTime();
        appointment.TreatmentId = appointmentDto.TreatmentId;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Appointments.Any(e => e.Id == id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // NUEVO: DELETE: api/Appointments/{id}
    // Permite a un cliente eliminar su propio turno
    [HttpDelete("{id}")]
    [Authorize(Roles = "Cliente")]
    public async Task<IActionResult> DeleteAppointment(Guid id)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdString == null)
        {
            return Unauthorized();
        }

        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null)
        {
            return NotFound();
        }

        // Validar que el usuario que intenta borrar es el dueño del turno
        if (appointment.ClientId != Guid.Parse(userIdString))
        {
            return Forbid();
        }

        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class AppointmentCreateDto
{
    public required string Description { get; set; }
    public required DateTime AppointmentDate { get; set; }

    public required Guid TreatmentId { get; set; }
}

public class AppointmentUpdateDto
{
    public required string Description { get; set; }
    public required DateTime AppointmentDate { get; set; }

    public required Guid TreatmentId { get; set; }
}

public class AppointmentDto
{
    public Guid Id { get; set; }
    public required string Description { get; set; }
    public required DateTime AppointmentDate { get; set; }
    public Guid ClientId { get; set; }
    public string? ClientName { get; set; }
    public Guid TreatmentId { get; set; }
    public string? TreatmentTitle { get; set; }
}


//// DTO para devolver los datos de los turnos
//public class AppointmentDto
//{
//    public Guid Id { get; set; }
//    public required string Description { get; set; }
//    public DateTime AppointmentDate { get; set; }
//    public Guid ClientId { get; set; }
//    public string ClientName { get; set; }
//}

//// DTO para la creación de turnos
//public class AppointmentCreateDto
//{
//    public required string Description { get; set; }
//    public required DateTime AppointmentDate { get; set; }
//}

//// NUEVO DTO para la actualización de turnos
//public class AppointmentUpdateDto
//{
//    public required string Description { get; set; }
//    public required DateTime AppointmentDate { get; set; }
//}
