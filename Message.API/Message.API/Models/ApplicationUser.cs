using Microsoft.AspNetCore.Identity;

namespace Message.API.Models;

public class ApplicationUser : IdentityUser
{
    public required string Name { get; set; }
    public required string LastName { get; set; }
}
