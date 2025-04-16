namespace SecureTaskManager.API.Models
{
    public class ApplicationUser
    {
        public int Id { get; set; } // PK
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User"; // Default

        public string FullName { get; set; } = string.Empty;
    }
}
