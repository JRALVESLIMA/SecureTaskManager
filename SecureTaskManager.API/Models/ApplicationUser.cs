using Microsoft.AspNetCore.Identity;

namespace SecureTaskManager.API.Models

{
    public class ApplicationUser : IdentityUser
    {

        public string FullName { get; set; } = string.Empty;
    }
}
