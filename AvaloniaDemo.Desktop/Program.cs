using System;

using Avalonia;
using Avalonia.ReactiveUI;

namespace AvaloniaDemo.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        //var settings = new CefSettings
        //{
        //    // 跨域配置
        //    CommandLineArgs =
        //{
        //    "--disable-web-security",
        //    "--allow-running-insecure-content"
        //}
        //};

        return AppBuilder.Configure<App>()
             .UsePlatformDetect()
             .WithInterFont()
             .LogToTrace()
             .UseReactiveUI();
    }
        
}
