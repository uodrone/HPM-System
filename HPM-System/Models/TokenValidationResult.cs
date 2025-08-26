using System.Security.Claims;

namespace HPM_System.Models
{
    public class TokenValidationResult
    {
        public bool IsValid { get; set; }
        public IEnumerable<Claim>? Claims { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
