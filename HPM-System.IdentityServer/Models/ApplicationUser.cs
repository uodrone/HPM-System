using Microsoft.AspNetCore.Identity;

namespace HPM_System.IdentityServer.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Patronymic { get; set; }
        public string? Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
