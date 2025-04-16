namespace SecureTaskManager.API.DTOs
{
    public class AuthResponse
    {
        public string UserName { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
