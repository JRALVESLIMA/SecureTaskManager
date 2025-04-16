using Microsoft.AspNetCore.Identity;
using SecureTaskManager.API.Models;

namespace SecureTaskManager.API.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedAdminUserAsync(ApplicationDbContext context)
        {
            var email = "master@admin.com";

            // Se o usuário já existe, não faz nada
            if (context.Users.Any(u => u.Email == email)) return;

            var master = new ApplicationUser
            {
                UserName = "adminmaster",
                Email = email,
                Role = "Admin"
            };

            var passwordHasher = new PasswordHasher<ApplicationUser>();
            master.PasswordHash = passwordHasher.HashPassword(master, "Admin123!"); // Senha forte e segura

            context.Users.Add(master);
            await context.SaveChangesAsync();
        }
    }
}
