using Avalonia.Controls;
using PhysicsX.App.ViewModels;
using PhysicsX.Core.Utils;

namespace PhysicsX.App.Views;

public partial class MainWindow : Window
{
    private readonly Logger _logger = Logger.Instance;

    public MainWindow()
    {
        _logger.Info("MainWindow constructor called", "MainWindow");

        InitializeComponent();

        _logger.Info("MainWindow initialized", "MainWindow");

        // 延迟到窗口加载后再连接
        this.Loaded += OnWindowLoaded;
    }

    private void OnWindowLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _logger.Info("MainWindow loaded event triggered", "MainWindow");

        // 连接 ViewModel 和 PhysicsCanvas
        if (DataContext is MainWindowViewModel viewModel)
        {
            _logger.Info("DataContext is MainWindowViewModel", "MainWindow");

            var canvas = this.FindControl<Controls.PhysicsCanvas>("PhysicsCanvas");
            if (canvas != null)
            {
                _logger.Info("PhysicsCanvas control found", "MainWindow");

                // 绑定控制回调
                viewModel.OnPlayPause = () =>
                {
                    if (viewModel.IsRunning)
                    {
                        _logger.Info("Starting simulation", "MainWindow");
                        canvas.Start();
                    }
                    else
                    {
                        _logger.Info("Pausing simulation", "MainWindow");
                        canvas.Pause();
                    }
                };

                viewModel.OnReset = () =>
                {
                    _logger.Info("Resetting simulation", "MainWindow");
                    canvas.Reset();
                };

                viewModel.OnGravityChangedAction = (g) =>
                {
                    _logger.Info($"Gravity changed to {g}", "MainWindow");
                    canvas.SetGravity(g);
                };

                // 连接场景编辑器
                viewModel.SceneEditor.OnSceneLoaded = (project) =>
                {
                    _logger.Info($"Scene loaded: {project.Name}", "MainWindow");
                    canvas.LoadScene(project);
                };

                viewModel.SceneEditor.OnSceneCleared = () =>
                {
                    _logger.Info("Scene cleared", "MainWindow");
                    canvas.ClearScene();
                };

                // 监听场景对象变化
                viewModel.SceneEditor.SceneObjects.CollectionChanged += (s, e) =>
                {
                    _logger.Info($"SceneObjects collection changed: {e.Action}", "MainWindow");
                    canvas.SyncSceneObjects(viewModel.SceneEditor.SceneObjects);
                };

                // 初始同步：如果编辑器已有对象，立即同步
                if (viewModel.SceneEditor.SceneObjects.Count > 0)
                {
                    _logger.Info($"Initial sync: {viewModel.SceneEditor.SceneObjects.Count} objects", "MainWindow");
                    canvas.SyncSceneObjects(viewModel.SceneEditor.SceneObjects);
                }
                else
                {
                    _logger.Info("No initial objects to sync", "MainWindow");
                }

                _logger.Info("MainWindow event bindings complete", "MainWindow");
            }
            else
            {
                _logger.Error("PhysicsCanvas control NOT found!", null, "MainWindow");
            }
        }
        else
        {
            _logger.Error("DataContext is NOT MainWindowViewModel!", null, "MainWindow");
        }
    }
}
