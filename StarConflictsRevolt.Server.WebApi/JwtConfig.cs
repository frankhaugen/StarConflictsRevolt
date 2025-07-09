using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace StarConflictsRevolt.Server.WebApi;

public static class JwtConfig
{
    public const string Issuer = "StarConflictsRevoltIdP";
    public const string Audience = "StarConflictsRevoltApi";
    // For demo: use a static key. In production, use a secure, rotated key or asymmetric keys.
    public const string Secret = "SuperSecretKeyForJwtTokenGeneration123!";

    public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
}
