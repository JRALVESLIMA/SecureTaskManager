namespace SecureTaskManager.API.DTOs
{
    public class UpdateUserRoleRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
