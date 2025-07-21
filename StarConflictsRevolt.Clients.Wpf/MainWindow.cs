using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using StarConflictsRevolt.Clients.Models;

namespace StarConflictsRevolt.Clients.Wpf;

internal class MainWindow : Window
{
    private readonly ILogger<MainWindow> _logger;
    private readonly MyTextBox _textBox;

    public MainWindow(ILogger<MainWindow> logger)
    {
        _logger = logger;
        _textBox = new MyTextBox("Write something", (o, args) => MessageBox.Show((o as TextBox)?.Text ?? ""));

        ConfigureWindow();

        var stack = new StackPanel();
        stack.Children.Add(_textBox);
        var launchBlissButton = new Button { Content = "Launch Galaxy (Bliss)", Margin = new Thickness(10) };
        launchBlissButton.Click += LaunchBlissButton_Click;
        stack.Children.Add(launchBlissButton);
        Content = stack;
    }

    private void ConfigureWindow()
    {
        MinWidth = 512;
        MinHeight = 256;

        SizeToContent = SizeToContent.WidthAndHeight;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        _logger.LogInformation("Closing");
        base.OnClosing(e);
    }

    private void LaunchBlissButton_Click(object sender, RoutedEventArgs e)
    {
        // Example config, replace with actual setup data
        var config = new GameClientConfiguration
        {
            GameServerUrl = "http://localhost:5153",
            GameServerHubUrl = "http://localhost:5153/gamehub",
            ApiBaseUrl = "http://localhost:5153"
        };
        var configPath = Path.Combine(Path.GetTempPath(), "galaxyConfig.json");
        File.WriteAllText(configPath, JsonSerializer.Serialize(config));
        var blissExe = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "StarConflictsRevolt.Clients.Bliss", "bin", "Debug", "net9.0", "StarConflictsRevolt.Clients.Bliss.exe"));
        var psi = new ProcessStartInfo
        {
            FileName = blissExe,
            Arguments = $"--config \"{configPath}\"",
            UseShellExecute = false
        };
        Process.Start(psi);
    }
}