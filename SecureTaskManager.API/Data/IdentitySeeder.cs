using Microsoft.AspNetCore.Identity;
using SecureTaskManager.API.Models;

namespace SecureTaskManager.API.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAndAdminUserAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            string[] roles = new[] { "Master", "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var email = "master@admin.com";
            var userName = "adminmaster";

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = userName,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "Admin123!");
                if (!result.Succeeded)
                {
                    throw new Exception("Falha ao criar usuário admin: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                await userManager.AddToRoleAsync(user, "Master");
            }
        }
    }
}
