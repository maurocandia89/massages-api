using Message.API.Infrastructure.Data;
using Message.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Message.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AppointmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AppointmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointments(
        string? date = null,
        [FromQuery] string? clientName = null,
        string? sortBy = null,
        string? sortDirection = null
    )
    {
        IQueryable<Appointment> query = _context.Appointments;

        query = query.Where(a => a.IsEnabled);

        if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime parsedDate))
        {
            parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
            query = query.Where(a => a.AppointmentDate.Date == parsedDate.Date);
        }

        if (!string.IsNullOrEmpty(clientName))
        {
            query = query.Where(a => a.ClientName.ToLower().Contains(clientName.ToLower()));
        }

        if (!string.IsNullOrEmpty(sortBy))
        {
            var isDescending = sortDirection?.ToLower() == "desc";

            query = sortBy.ToLower() switch
            {
                "clientname" => isDescending
                    ? query.OrderByDescending(a => a.ClientName)
                    : query.OrderBy(a => a.ClientName),
                "appointmentdate" => isDescending
                    ? query.OrderByDescending(a => a.AppointmentDate)
                    : query.OrderBy(a => a.AppointmentDate),
                "starttime" => isDescending
                    ? query.OrderByDescending(a => a.StartTime)
                    : query.OrderBy(a => a.StartTime),
                _ => query.OrderBy(a => a.Id),
            };
        }
        else
        {
            query = query.OrderBy(a => a.Id);
        }

        return await query.ToListAsync();
    }

    // GET: api/Appointments/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Appointment>> GetAppointment(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);

        if (appointment == null)
        {
            return NotFound();
        }

        return appointment;
    }

    [HttpPost]
    public async Task<ActionResult<Appointment>> PostAppointment(Appointment appointment)
    {
        try
        {
            // Convierte la fecha del turno a UTC antes de guardarla
            appointment.AppointmentDate = DateTime.SpecifyKind(
                appointment.AppointmentDate,
                DateTimeKind.Utc
            );

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAppointment", new { id = appointment.Id }, appointment);
        }
        catch (Exception ex)
        {
            var innerExceptionMessage = ex.InnerException?.Message;
            Console.WriteLine($"Error al guardar el turno: {innerExceptionMessage}");
            return StatusCode(500, $"Error al guardar el turno: {innerExceptionMessage}");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutAppointment(int id, Appointment appointment)
    {
        appointment.Id = id;

        appointment.ModifiedAt = DateTime.UtcNow;
        appointment.AppointmentDate = DateTime.SpecifyKind(
            appointment.AppointmentDate,
            DateTimeKind.Utc
        );

        _context.Entry(appointment).State = EntityState.Modified;

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

    // DELETE: api/Appointments/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAppointment(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null)
        {
            return NotFound();
        }

        appointment.IsEnabled = false;
        appointment.ModifiedAt = DateTime.UtcNow;

        _context.Entry(appointment).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
