using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using PhysicsX.App.ViewModels;
using PhysicsX.App.Views;
using PhysicsX.Core.Utils;

namespace PhysicsX.App;

public partial class App : Application
{
    private readonly Logger _logger = Logger.Instance;

    public override void Initialize()
    {
        // 配置日志
        _logger.Configure(
            minLevel: LogLevel.Debug,
            writeToConsole: true,
            writeToFile: true
        );

        _logger.Info("=================================================", "App");
        _logger.Info("PhysicsX Application Starting", "App");
        _logger.Info($"Log file: {_logger.GetLogFilePath()}", "App");
        _logger.Info("=================================================", "App");

        AvaloniaXamlLoader.Load(this);

        _logger.Info("XAML loaded", "App");
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _logger.Info("Initializing desktop application", "App");

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };

            _logger.Info("MainWindow created", "App");
        }

        base.OnFrameworkInitializationCompleted();

        _logger.Info("Framework initialization completed", "App");

        // 清理旧日志（保留7天）
        _logger.CleanOldLogs(7);
    }
}
