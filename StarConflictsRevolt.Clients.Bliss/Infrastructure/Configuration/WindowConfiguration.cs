using Bliss.CSharp.Windowing;
using Veldrid;

namespace StarConflictsRevolt.Clients.Bliss.Infrastructure.Configuration;
public class WindowConfiguration
{
    public decimal Ratio { get; private set; }
    public WindowType Type { get; private set; }
    public int Width { get; private set; }
    
    public int Height { get; private set; }
    public string Title { get; private set; }
    public WindowState State { get; private set; }
    public GraphicsDeviceOptions GraphicsDeviceOptions { get; private set; }
    public GraphicsBackend GraphicsBackend { get; private set; }

    public WindowConfiguration () => Create16x9(); // Default to 16:9 aspect ratio

    public WindowConfiguration Set(WindowConfiguration windowConfig)
    {
        windowConfig.Ratio = Ratio;
        windowConfig.Type = Type;
        windowConfig.Width = Width;
        windowConfig.Title = Title;
        windowConfig.State = State;
        windowConfig.GraphicsDeviceOptions = GraphicsDeviceOptions;
        windowConfig.GraphicsBackend = GraphicsBackend;
        return windowConfig;
    }

    private WindowConfiguration(decimal ratio, WindowType type, int width, string title, WindowState state)
    {
        Ratio = ratio;
        Type = type;
        Width = width;
        Height = Convert.ToInt32(Math.Round(width / ratio, MidpointRounding.AwayFromZero));
        Title = title;
        State = state;
        GraphicsDeviceOptions = new GraphicsDeviceOptions
        {
            SyncToVerticalBlank = true,
            PreferStandardClipSpaceYDirection = true,
            ResourceBindingModel = ResourceBindingModel.Improved,
        };
        GraphicsBackend = type switch
        {
            WindowType.Sdl3 => GraphicsBackend.Vulkan,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public static WindowConfiguration Create16x9(int width = 1280, string title = "Star Conflicts Revolt", WindowType type = WindowType.Sdl3, WindowState state = WindowState.AlwaysOnTop | WindowState.Resizable) => new(16.0m / 9.0m, type, width, title, state);

    public static WindowConfiguration Create4x3(int width = 1024, string title = "Star Conflicts Revolt", WindowType type = WindowType.Sdl3, WindowState state = WindowState.AlwaysOnTop | WindowState.Resizable) => new(4.0m / 3.0m, type, width, title, state);

    public static WindowConfiguration CreateCustom(decimal ratio, int width, string title = "Star Conflicts Revolt", WindowType type = WindowType.Sdl3, WindowState state = WindowState.AlwaysOnTop | WindowState.Resizable) => new(ratio, type, width, title, state);
}