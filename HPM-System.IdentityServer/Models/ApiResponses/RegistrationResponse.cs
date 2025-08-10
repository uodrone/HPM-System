namespace HPM_System.IdentityServer.Models.ApiResponses
{
    public class RegistrationResponse
    {
        public string Message { get; set; }
        public bool UserServiceCreated { get; set; }
        public string UserId { get; set; }
        public List<ApiError> Errors { get; set; }
    }
}
