using Message.API.Models;
using Microsoft.AspNetCore.Identity;

namespace Message.API.Services;

public class DataSeeder
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public DataSeeder(
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration
    )
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task SeedRolesAndAdminUserAsync()
    {
        await CreateRoleAsync("Admin");
        await CreateRoleAsync("Cliente");

        await CreateOwnerUserAsync();
    }

    private async Task CreateRoleAsync(string roleName)
    {
        var roleExist = await _roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        }
    }

    private async Task CreateOwnerUserAsync()
    {
        var ownerEmail = _configuration["Owner:Email"];
        var ownerPassword = _configuration["Owner:Password"];

        if (string.IsNullOrEmpty(ownerEmail) || string.IsNullOrEmpty(ownerPassword))
        {
            return;
        }

        var ownerUser = await _userManager.FindByEmailAsync(ownerEmail);
        if (ownerUser == null)
        {
            var newOwner = new ApplicationUser
            {
                UserName = ownerEmail,
                Email = ownerEmail,
                Name = _configuration["Owner:Name"],
                LastName = _configuration["Owner:LastName"],
            };

            var createResult = await _userManager.CreateAsync(newOwner, ownerPassword);
            if (createResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(newOwner, "Admin");
            }
        }
    }
}
