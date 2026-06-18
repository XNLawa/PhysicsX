using Avalonia.Controls;
using PhysicsX.App.ViewModels;

namespace PhysicsX.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // 连接 ViewModel 和 PhysicsCanvas
        if (DataContext is MainWindowViewModel viewModel)
        {
            var canvas = this.FindControl<Controls.PhysicsCanvas>("PhysicsCanvas");
            if (canvas != null)
            {
                // 绑定控制回调
                viewModel.OnPlayPause = () =>
                {
                    if (viewModel.IsRunning)
                        canvas.Start();
                    else
                        canvas.Pause();
                };

                viewModel.OnReset = () => canvas.Reset();
                viewModel.OnGravityChangedAction = (g) => canvas.SetGravity(g);

                // 连接场景编辑器
                viewModel.SceneEditor.OnSceneLoaded = (project) =>
                {
                    canvas.LoadScene(project);
                };

                viewModel.SceneEditor.OnSceneCleared = () =>
                {
                    canvas.ClearScene();
                };

                // 监听场景对象变化
                viewModel.SceneEditor.SceneObjects.CollectionChanged += (s, e) =>
                {
                    canvas.SyncSceneObjects(viewModel.SceneEditor.SceneObjects);
                };
            }
        }
    }
}
