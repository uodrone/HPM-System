namespace HPM_System.IdentityServer.Services.AccountService
{
    public class RegistrationResultData
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool UserServiceCreated { get; set; }
    }
}
