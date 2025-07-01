namespace SecureTaskManager.API.DTOs
{
    public class PromoteUserRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string NewRole { get; set; } = string.Empty;
    }
}
