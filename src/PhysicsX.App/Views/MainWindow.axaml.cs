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
            }
        }
    }
}
