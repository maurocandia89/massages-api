using Message.API.Infrastructure.Data;
using Message.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace massage_app.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class TreatmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TreatmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Treatments
        // Obtiene todos los tratamientos.
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Treatment>>> GetTreatments()
        {
            return await _context.Treatments.ToListAsync();
        }

        // GET: api/Treatments/{id}
        // Obtiene un tratamiento específico por ID.
        [HttpGet("{id}")]
        public async Task<ActionResult<Treatment>> GetTreatment(Guid id)
        {
            var treatment = await _context.Treatments.FindAsync(id);

            if (treatment == null)
            {
                return NotFound();
            }

            return treatment;
        }

        // POST: api/Treatments
        // Crea un nuevo tratamiento. Solo los administradores pueden hacer esto.
        [HttpPost]
        public async Task<ActionResult<Treatment>> PostTreatment(Treatment treatment)
        {
            _context.Treatments.Add(treatment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTreatment), new { id = treatment.Id }, treatment);
        }

        // PUT: api/Treatments/{id}
        // Actualiza un tratamiento existente. Solo los administradores pueden hacer esto.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTreatment(Guid id, Treatment treatment)
        {
            if (id != treatment.Id)
            {
                return BadRequest();
            }

            _context.Entry(treatment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TreatmentExists(id))
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

        // DELETE: api/Treatments/{id}
        // Elimina un tratamiento. Solo los administradores pueden hacer esto.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTreatment(Guid id)
        {
            var treatment = await _context.Treatments.FindAsync(id);
            if (treatment == null)
            {
                return NotFound();
            }

            _context.Treatments.Remove(treatment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TreatmentExists(Guid id)
        {
            return _context.Treatments.Any(e => e.Id == id);
        }
    }
}
