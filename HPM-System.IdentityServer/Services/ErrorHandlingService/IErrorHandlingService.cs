using Microsoft.AspNetCore.Identity;

namespace HPM_System.IdentityServer.Services.ErrorHandlingService
{
    public interface IErrorHandlingService
    {
        string GetDetailedErrorMessage(IdentityError error);
        string GetDefaultPasswordRequirements();
    }
}
