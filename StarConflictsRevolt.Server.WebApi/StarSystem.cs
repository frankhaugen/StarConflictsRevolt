using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StarConflictsRevolt.Server.WebApi;

/// <summary>Options for discifying 3D galactic star data into a 2D star map.</summary>
public sealed class DiscifyOptions
{
    /// <summary>Maximum distance from Sol (parsecs). Stars beyond this are excluded. Default 500.</summary>
    public double MaxDistanceFromSolParsecs { get; init; } = 500;

    /// <summary>Maximum absolute Z (height above/below galactic plane, parsecs). Keeps a thin disc. Default 100.</summary>
    public double MaxAbsZParsecs { get; init; } = 100;

    /// <summary>Maximum number of stars to return (subsampling). Default 4000.</summary>
    public int MaxStars { get; init; } = 4000;

    /// <summary>Output coordinate range (symmetric). Map X,Y into [-ScaleToRange, +ScaleToRange]. Default 1000 (matches GalaxyMap).</summary>
    public double ScaleToRange { get; init; } = 1000;
}

public static class StarSystemHelper
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>Load real star systems from embedded resource (starsystems.json).</summary>
    public static async IAsyncEnumerable<StarSystem> LoadStarSystemsFromEmbeddedAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var assembly = typeof(StarSystemHelper).Assembly;
        await using var stream = assembly.GetManifestResourceStream("StarConflictsRevolt.Server.WebApi.starsystems.json");
        if (stream == null)
            yield break;
        var jsonDocument = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        foreach (var element in jsonDocument.RootElement.EnumerateArray())
            yield return element.Deserialize<StarSystem>(JsonSerializerOptions)!;
    }

    /// <summary>Discify 3D galactic coordinates to a 2D star map: keep X,Y (galactic plane), filter by distance and |Z|, subsample, scale to map range.</summary>
    public static async Task<IReadOnlyList<(string Name, double MapX, double MapY)>> DiscifyAsync(IAsyncEnumerable<StarSystem> systems, DiscifyOptions options, CancellationToken cancellationToken = default)
    {
        var maxDist = options.MaxDistanceFromSolParsecs;
        var maxZ = options.MaxAbsZParsecs;
        var results = new List<(string Name, double X, double Y)>();
        double minX = double.MaxValue, maxX = double.MinValue, minY = double.MaxValue, maxY = double.MinValue;

        await foreach (var s in systems.WithCancellation(cancellationToken))
        {
            var c = s.Coordinates;
            if (c?.X == null || c.Y == null || c.Z == null || c.DistanceFromSol == null)
                continue;
            if (c.DistanceFromSol.Value > maxDist || Math.Abs(c.Z.Value) > maxZ)
                continue;

            var name = s.Star?.Proper?.Trim();
            if (string.IsNullOrEmpty(name))
                name = s.Star?.Hip ?? s.Star?.Gl ?? s.Star?.Hd ?? "?";
            results.Add((name, c.X.Value, c.Y.Value));
            minX = Math.Min(minX, c.X.Value);
            maxX = Math.Max(maxX, c.X.Value);
            minY = Math.Min(minY, c.Y.Value);
            maxY = Math.Max(maxY, c.Y.Value);
        }

        if (results.Count == 0)
            return Array.Empty<(string, double, double)>();

        var rangeX = Math.Max(maxX - minX, 1);
        var rangeY = Math.Max(maxY - minY, 1);
        var scale = options.ScaleToRange;
        var scaleX = 2 * scale / rangeX;
        var scaleY = 2 * scale / rangeY;
        var centerX = (minX + maxX) / 2;
        var centerY = (minY + maxY) / 2;

        var mapped = results
            .Select(r => (r.Name, MapX: (r.X - centerX) * scaleX, MapY: (r.Y - centerY) * scaleY))
            .ToList();

        if (mapped.Count <= options.MaxStars)
            return mapped.Select(r => (r.Name, r.MapX, r.MapY)).ToList();

        var step = (int)Math.Ceiling((double)mapped.Count / options.MaxStars);
        var subsampled = new List<(string Name, double MapX, double MapY)>();
        for (var i = 0; i < mapped.Count && subsampled.Count < options.MaxStars; i += step)
            subsampled.Add((mapped[i].Name, mapped[i].MapX, mapped[i].MapY));
        return subsampled;
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