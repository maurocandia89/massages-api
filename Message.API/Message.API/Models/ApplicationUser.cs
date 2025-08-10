using Message.API.Models;
using Microsoft.AspNetCore.Identity;

namespace Message.API.Models
{
    // Cambiamos el tipo base de IdentityUser a IdentityUser<Guid>
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }
    }
}
