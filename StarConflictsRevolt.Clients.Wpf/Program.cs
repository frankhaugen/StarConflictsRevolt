using System.Runtime.InteropServices;
using System.Windows;
using StarConflictsRevolt.Clients.Wpf.Extensions;

namespace StarConflictsRevolt.Clients.Wpf;

internal class Program
{
    [STAThread]
    public static void Main(params string[] args)
    {
        AllocConsole();

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                context.SetContentPathToApplicationDirectory();

                services.AddScoped<Application>();

                services.AddHostedService<Worker>();

                services.AddScoped<MainWindow>();
                services.AddHostedService<WindowHost>();
            })
            .Build();

        host.Run();
    }

    [DllImport("kernel32")]
    static extern bool AllocConsole();
}