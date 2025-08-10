namespace HPM_System.IdentityServer.Models.ApiResponses
{
    public class LoginResponse
    {
        public string Message { get; set; }
        public string Token { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
