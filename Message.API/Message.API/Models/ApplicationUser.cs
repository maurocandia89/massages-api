using Microsoft.AspNetCore.Identity;

namespace Message.API.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? Name { get; set; }
    public string? LastName { get; set; }
    public DateTime BirthDate { get; set; }
    public ICollection<Appointment>? Appointments { get; set; }
}
