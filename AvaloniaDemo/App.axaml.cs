﻿using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using AvaloniaDemo.ViewModels;
using AvaloniaDemo.Views;

namespace AvaloniaDemo;

public partial class App : Application
{
    public override void Initialize()
    {   
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {  
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new EnvironmentCheckViewModel()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new EnvironmentCheckViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
