using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StarConflictsRevolt.Server.WebApi;

public static class StarSystemHelper
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = true, ReferenceHandler = ReferenceHandler.IgnoreCycles, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull};

    public static async IAsyncEnumerable<StarSystem> LoadStarSystemsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var assembly = typeof(StarSystemHelper).Assembly;
        await using var stream = assembly.GetManifestResourceStream("StarConflictsRevolt.Server.WebApi.Data.stars.json")!;
        
        var jsonDocument = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        foreach (var element in jsonDocument.RootElement.EnumerateArray())
        {            
            yield return element.Deserialize<StarSystem>(JsonSerializerOptions)!;
        }
    }
}

public record Coordinates(
    double? X,
    double? Y,
    double? Z,
    double? DistanceFromSol
);

public record StarSystem(
    Star Star,
    Coordinates Coordinates,
    string Type,
    double? Size
);

public record Star(
    int? Id,
    string Proper,
    double? Ra,
    double? Dec,
    double? Dist,
    double? Pmra,
    double? Pmdec,
    double? Rv,
    double? Mag,
    double? Absmag,
    string Spect,
    string Ci,
    double? X,
    double? Y,
    double? Z,
    double? Lum,
    string Hip,
    string Hd,
    string Gl,
    string Hr,
    string Bf,
    int? Flam,
    string Bayer
);