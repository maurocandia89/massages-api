using System.Security.Claims;
using Message.API.Infrastructure.Data;
using Message.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Message.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AppointmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAppointments(
        [FromQuery] string? sortBy = "AppointmentDate",
        [FromQuery] string? sortDirection = "asc"
    )
    {
        var query = _context
            .Appointments.Include(a => a.Client)
            .Include(b => b.Treatment)
            .AsQueryable();

        // Aplicar ordenamiento dinámico
        query = (sortBy?.ToLower(), sortDirection?.ToLower()) switch
        {
            ("clientname", "asc") => query.OrderBy(a => a.Client!.Name),
            ("clientname", "desc") => query.OrderByDescending(a => a.Client!.Name),
            ("treatmenttitle", "asc") => query.OrderBy(a => a.Treatment!.Title),
            ("treatmenttitle", "desc") => query.OrderByDescending(a => a.Treatment!.Title),
            ("appointmentdate", "desc") => query.OrderByDescending(a => a.AppointmentDate),
            _ => query.OrderBy(a => a.AppointmentDate),
        };

        var appointments = await query
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                AppointmentDate = a.AppointmentDate,
                ClientId = a.ClientId,
                ClientName = $"{a.Client!.Name} {a.Client.LastName}",
                TreatmentId = a.TreatmentId,
                TreatmentTitle = a.Treatment!.Title,
            })
            .ToListAsync();

        return Ok(appointments);
    }

    [HttpGet("my-appointments")]
    [Authorize(Roles = "Cliente")]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetMyAppointments(
        [FromQuery] string? sortBy = "AppointmentDate",
        [FromQuery] string? sortDirection = "asc"
    )
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdString == null)
        {
            return Unauthorized();
        }

        var query = _context
            .Appointments.Include(a => a.Client)
            .Include(b => b.Treatment)
            .Where(a => a.ClientId == Guid.Parse(userIdString))
            .AsQueryable();

        // Aplicar ordenamiento dinámico
        query = (sortBy?.ToLower(), sortDirection?.ToLower()) switch
        {
            ("clientname", "asc") => query.OrderBy(a => a.Client!.Name),
            ("clientname", "desc") => query.OrderByDescending(a => a.Client!.Name),
            ("treatmenttitle", "asc") => query.OrderBy(a => a.Treatment!.Title),
            ("treatmenttitle", "desc") => query.OrderByDescending(a => a.Treatment!.Title),
            ("appointmentdate", "desc") => query.OrderByDescending(a => a.AppointmentDate),
            _ => query.OrderBy(a => a.AppointmentDate), // default
        };

        var clientAppointments = await query
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
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
        var hour = appointmentDto.AppointmentDate.Hour;
        var minutes = appointmentDto.AppointmentDate.Minute;

        if (hour < 9 || hour > 19 || minutes != 0)
        {
            return BadRequest(
                "Los turnos deben ser entre las 9:00 y las 20:00 hs, en bloques de 1 hora."
            );
        }

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

        var hour = appointmentDto.AppointmentDate.Hour;
        var minutes = appointmentDto.AppointmentDate.Minute;

        if (hour < 9 || hour > 19 || minutes != 0)
        {
            return BadRequest(
                "Los turnos deben ser entre las 9:00 y las 20:00 hs, en bloques de 1 hora."
            );
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

    [HttpPut("admin/approve/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveAppointment(Guid id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null)
            return NotFound();

        appointment.Estado = "Aprobado";
        appointment.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("admin/cancel/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CancelAppointment(Guid id, [FromBody] string motivo)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null)
            return NotFound();

        appointment.Estado = "Cancelado";
        appointment.MotivoCancelacion = motivo;
        appointment.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }
}

public class AppointmentCreateDto
{
    public required DateTime AppointmentDate { get; set; }

    public required Guid TreatmentId { get; set; }
}

public class AppointmentUpdateDto
{
    public required DateTime AppointmentDate { get; set; }

    public required Guid TreatmentId { get; set; }
}

public class AppointmentDto
{
    public Guid Id { get; set; }
    public required DateTime AppointmentDate { get; set; }
    public Guid ClientId { get; set; }
    public string? ClientName { get; set; }
    public Guid TreatmentId { get; set; }
    public string? TreatmentTitle { get; set; }

    public string Estado { get; set; } = "Pendiente";
    public string? MotivoCancelacion { get; set; }
}
