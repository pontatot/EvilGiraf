using Microsoft.AspNetCore.Authentication;

namespace EvilGiraf.Authentication;

public class ApiKeyAuthOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "ApiKey";
    public string ApiKey { get; set; } = string.Empty;
}