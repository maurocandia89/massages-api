using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Message.API.Infrastructure.Data;
using Message.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Message.API.Controllers
{
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
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    ClientId = a.ClientId,
                    ClientName = $"{a.Client!.Name} {a.Client.LastName}",
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
                .Where(a => a.ClientId == Guid.Parse(userIdString))
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    ClientId = a.ClientId,
                    ClientName = $"{a.Client!.Name} {a.Client.LastName}",
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
                Title = appointment.Title,
                Description = appointment.Description,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                ClientId = appointment.ClientId,
                ClientName = $"{appointment.Client!.Name} {appointment.Client.LastName}",
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

            var appointment = new Appointment
            {
                Title = appointmentDto.Title,
                Description = appointmentDto.Description,
                StartTime = appointmentDto.StartTime.ToUniversalTime(),
                EndTime = appointmentDto.StartTime.ToUniversalTime().AddHours(1),
                ClientId = Guid.Parse(userId),
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetAppointment),
                new { id = appointment.Id },
                appointment
            );
        }

        // NUEVO: PUT: api/Appointments/{id}
        // Permite a un cliente actualizar su propio turno
        [HttpPut("{id}")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> PutAppointment(
            Guid id,
            AppointmentUpdateDto appointmentDto
        )
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

            appointment.Title = appointmentDto.Title;
            appointment.Description = appointmentDto.Description;
            // La fecha de inicio también debe convertirse a UTC
            appointment.StartTime = appointmentDto.StartTime.ToUniversalTime();
            appointment.EndTime = appointmentDto.StartTime.ToUniversalTime().AddHours(1);

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

    // DTO para devolver los datos de los turnos
    public class AppointmentDto
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Guid ClientId { get; set; }
        public string ClientName { get; set; }
    }

    // DTO para la creación de turnos
    public class AppointmentCreateDto
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required DateTime StartTime { get; set; }
    }

    // NUEVO DTO para la actualización de turnos
    public class AppointmentUpdateDto
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required DateTime StartTime { get; set; }
    }
}
