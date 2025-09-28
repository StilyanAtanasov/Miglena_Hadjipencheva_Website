using MHAuthorWebsite.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MHAuthorWebsite.Data.Seeding;

public static class AdminSeeder
{
    private const string AdminRole = "Admin";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        UserManager<ApplicationUser> userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        RoleManager<IdentityRole> roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();

        string? adminPassword = configuration["AdminPassword"];
        string? adminEmail = configuration["AdminEmail"];
        if (string.IsNullOrWhiteSpace(adminPassword) || string.IsNullOrWhiteSpace(adminEmail))
            throw new InvalidOperationException("Missing Admin credentials in user secrets or configuration.");

        if (!await roleManager.RoleExistsAsync(AdminRole))
            await roleManager.CreateAsync(new IdentityRole(AdminRole));

        ApplicationUser? adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                Name = "Системен администратор"
            };

            IdentityResult result = await userManager.CreateAsync(adminUser, adminPassword);
            if (!result.Succeeded)
                throw new InvalidOperationException("Failed to create admin user: " + string.Join(", ", result.Errors.Select(e => e.Description)));

            await userManager.AddToRoleAsync(adminUser, AdminRole);
        }
    }
}