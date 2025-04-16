using Microsoft.EntityFrameworkCore;
using SecureTaskManager.API.Models;

namespace SecureTaskManager.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> Users { get; set; }
    }
}
