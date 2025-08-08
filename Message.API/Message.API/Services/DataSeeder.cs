using Message.API.Models;
using Microsoft.AspNetCore.Identity;

namespace Message.API.Services;

public class DataSeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public DataSeeder(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration
    )
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _configuration = configuration;
    }

    // Método principal para sembrar los datos
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
            await _roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    private async Task CreateOwnerUserAsync()
    {
        var ownerEmail = _configuration["Owner:Email"];
        var ownerPassword = _configuration["Owner:Password"];

        // Si el email de la dueña no está configurado, no hacemos nada.
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
                // Asignamos el rol de "Dueña" al usuario recién creado
                await _userManager.AddToRoleAsync(newOwner, "Admin");
            }
        }
    }
}
