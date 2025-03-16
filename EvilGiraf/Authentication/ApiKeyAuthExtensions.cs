using Microsoft.AspNetCore.Authentication;

namespace EvilGiraf.Authentication;

public static class ApiKeyAuthExtensions
{
    public static AuthenticationBuilder AddApiKeyAuth(
        this IServiceCollection services,
        Action<ApiKeyAuthOptions> options)
    {
        return services.AddAuthentication(ApiKeyAuthOptions.DefaultScheme)
            .AddScheme<ApiKeyAuthOptions, ApiKeyAuthHandler>(
                ApiKeyAuthOptions.DefaultScheme, options);
    }
}