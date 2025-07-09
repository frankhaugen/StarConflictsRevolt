using Frank.Security.Cryptography;

namespace StarConflictsRevolt.Server.WebApi;

public static class SecurityHelper
{
    public static string GeneratePassphrase(int wordCount = 5)
    {
        var builder = new PassPhraseBuilder(wordCount)
            .IncludeNouns()
            .IncludeAdjectives()
            .IncludeVerbs();
        
        return builder.Build();
    }
}