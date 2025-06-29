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
                new IdentityResources.Email(), // Добавляем email ресурс
            };

        // Области доступа к API
        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                new ApiScope("api.read", "Read Access to API"),
                new ApiScope("api.write", "Write Access to API")
            };

        // API Resources для включения claims в Access Token
        public static IEnumerable<ApiResource> ApiResources =>
            new List<ApiResource>
            {
                new ApiResource("hpm.api", "HPM System API")
                {
                    Scopes = { "api.read", "api.write" },
                    UserClaims = { "email", "given_name", "family_name", "phone_number", "patronymic" }
                }
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
                    
                    // Включаем claims в токены
                    AlwaysIncludeUserClaimsInIdToken = true,
                    AlwaysSendClientClaims = true,
                    IncludeJwtId = true,

                    AllowedScopes =
                    {
                        "openid",
                        "profile",
                        "email", // Добавляем email scope
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