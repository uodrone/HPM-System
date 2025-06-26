using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;

namespace HPM_System.IdentityServer
{
    public static class IdentityConfiguration
    {
        // Пользователи для тестирования
        public static List<TestUser> Users =
            new List<TestUser>();

        // Ресурсы идентичности (OpenID Connect)
        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };

        // Облачка доступа к API
        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                new ApiScope("api.read", "Read Access to API"),
                new ApiScope("api.write", "Write Access to API")
            };

        // Клиенты, которые могут использовать этот IdentityServer
        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
                new Client
                {
                    ClientId = "hpmsystem.client",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    AllowOfflineAccess = true,
                    AccessTokenLifetime = 3600,

                    AllowedScopes =
                    {
                        "openid",
                        "profile",
                        "api.read"
                    },
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    }
                }
            };
    }
}