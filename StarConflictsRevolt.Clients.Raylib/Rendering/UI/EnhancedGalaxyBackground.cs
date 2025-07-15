using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.UI;

public class EnhancedGalaxyBackground
{
    private int _frame = 0;
    private readonly Random _random = new(42); // Fixed seed for consistent starfield
    private readonly List<StarData> _stars = new();
    private const int NumStars = 5000;
    private const float SpiralArms = 4.0f;
    private const float SpiralTightness = 0.3f;
    private const float SpiralSpread = 0.4f;
    private const float MaxRadius = 1.5f; // Larger radius for more expansive feel

    public EnhancedGalaxyBackground()
    {
        GenerateStarfield();
    }

    private void GenerateStarfield()
    {
        _stars.Clear();
        
        for (int i = 0; i < NumStars; i++)
        {
            var star = GenerateStar(i);
            _stars.Add(star);
        }
    }

    private StarData GenerateStar(int index)
    {
        // Improved spiral galaxy generation using logarithmic spiral
        var t = (float)index / NumStars;
        var radius = t * MaxRadius;
        
        // Logarithmic spiral with multiple arms
        var armIndex = index % (int)SpiralArms;
        var armOffset = (2.0f * Math.PI * armIndex) / SpiralArms;
        var angle = SpiralTightness * radius + armOffset + _random.NextDouble() * 0.5f;
        
        // Add some randomness to the spiral
        var radiusVariation = (float)(_random.NextDouble() - 0.5f) * SpiralSpread;
        var finalRadius = radius + radiusVariation;
        
        // Convert to screen coordinates
        var screenWidth = Raylib_CSharp.Windowing.Window.GetScreenWidth();
        var screenHeight = Raylib_CSharp.Windowing.Window.GetScreenHeight();
        var centerX = screenWidth / 2.0f;
        var centerY = screenHeight / 2.0f;
        var maxScreenRadius = Math.Min(centerX, centerY) * MaxRadius;
        
        var x = centerX + (float)(Math.Cos(angle) * finalRadius * maxScreenRadius);
        var y = centerY + (float)(Math.Sin(angle) * finalRadius * maxScreenRadius);
        
        // Add some random offset for natural distribution
        x += (float)(_random.NextDouble() - 0.5f) * 20;
        y += (float)(_random.NextDouble() - 0.5f) * 20;
        
        // Star properties
        var starType = _random.Next(100);
        var brightness = _random.Next(50, 200);
        var twinkleSpeed = _random.Next(10, 50);
        var twinklePhase = _random.Next(0, 100);
        
        Color color;
        if (starType < 5) // Bright blue stars (rare)
        {
            color = new Color((byte)(150 + brightness), (byte)(150 + brightness), 255, 255);
        }
        else if (starType < 15) // Yellow stars (uncommon)
        {
            color = new Color(255, (byte)(200 + brightness), (byte)(100 + brightness), 255);
        }
        else if (starType < 30) // Orange stars (uncommon)
        {
            color = new Color(255, (byte)(150 + brightness), (byte)(50 + brightness), 255);
        }
        else // White/blue-white stars (common)
        {
            color = new Color((byte)(200 + brightness), (byte)(200 + brightness), (byte)(220 + brightness), 255);
        }
        
        return new StarData
        {
            X = x,
            Y = y,
            BaseColor = color,
            TwinkleSpeed = twinkleSpeed,
            TwinklePhase = twinklePhase,
            Brightness = brightness
        };
    }

    public void Draw()
    {
        _frame++;
        
        // Draw stars with enhanced twinkling
        foreach (var star in _stars)
        {
            var twinkle = CalculateTwinkle(star);
            var color = ApplyTwinkle(star.BaseColor, twinkle);
            
            // Draw star with glow effect for bright stars
            if (star.Brightness > 150)
            {
                // Glow effect
                for (int r = 3; r > 0; r--)
                {
                    var glowColor = new Color(color.R, color.G, color.B, (byte)(50 / r));
                    Graphics.DrawCircle((int)star.X, (int)star.Y, r, glowColor);
                }
            }
            
            // Main star
            Graphics.DrawPixel((int)star.X, (int)star.Y, color);
            
            // Cross pattern for very bright stars
            if (star.Brightness > 180)
            {
                var crossColor = new Color(color.R, color.G, color.B, (byte)(color.A * 0.7f));
                Graphics.DrawLine((int)star.X - 2, (int)star.Y, (int)star.X + 2, (int)star.Y, crossColor);
                Graphics.DrawLine((int)star.X, (int)star.Y - 2, (int)star.X, (int)star.Y + 2, crossColor);
            }
        }
        
        // Draw nebula effects
        DrawNebulae();
    }

    private float CalculateTwinkle(StarData star)
    {
        var time = (_frame + star.TwinklePhase) / (float)star.TwinkleSpeed;
        var twinkle = (float)(Math.Sin(time) * 0.3 + 0.7); // Varies between 0.4 and 1.0
        
        // Add some randomness to twinkling
        if (_random.Next(1000) < 5) // 0.5% chance per frame
        {
            twinkle *= 1.5f; // Bright flash
        }
        
        return Math.Clamp(twinkle, 0.2f, 1.5f);
    }

    private Color ApplyTwinkle(Color baseColor, float twinkle)
    {
        return new Color(
            (byte)(baseColor.R * twinkle),
            (byte)(baseColor.G * twinkle),
            (byte)(baseColor.B * twinkle),
            baseColor.A
        );
    }

    private void DrawNebulae()
    {
        var screenWidth = Raylib_CSharp.Windowing.Window.GetScreenWidth();
        var screenHeight = Raylib_CSharp.Windowing.Window.GetScreenHeight();
        
        // Draw distant nebula clouds
        for (int i = 0; i < 3; i++)
        {
            var nebulaX = (float)(_random.NextDouble() * screenWidth);
            var nebulaY = (float)(_random.NextDouble() * screenHeight);
            var nebulaSize = _random.Next(100, 300);
            var nebulaAlpha = (byte)(_random.Next(10, 30));
            
            // Nebula color based on position
            Color nebulaColor;
            if (nebulaX < screenWidth / 3)
            {
                nebulaColor = new Color(100, 50, 150, nebulaAlpha); // Purple
            }
            else if (nebulaX < 2 * screenWidth / 3)
            {
                nebulaColor = new Color(50, 100, 150, nebulaAlpha); // Blue
            }
            else
            {
                nebulaColor = new Color(150, 100, 50, nebulaAlpha); // Orange
            }
            
            // Draw nebula as a soft circle
            for (int r = nebulaSize; r > 0; r -= 5)
            {
                var alpha = (byte)(nebulaAlpha * (1.0f - (float)r / nebulaSize));
                var color = new Color(nebulaColor.R, nebulaColor.G, nebulaColor.B, alpha);
                Graphics.DrawCircle((int)nebulaX, (int)nebulaY, r, color);
            }
        }
    }

    public void Regenerate()
    {
        GenerateStarfield();
    }

    private class StarData
    {
        public float X { get; set; }
        public float Y { get; set; }
        public Color BaseColor { get; set; }
        public int TwinkleSpeed { get; set; }
        public int TwinklePhase { get; set; }
        public int Brightness { get; set; }
    }
} 