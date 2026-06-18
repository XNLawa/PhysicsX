using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PhysicsX.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "PhysicsX - 物理仿真与解题演示";

    [ObservableProperty]
    private bool _isRunning = false;

    [ObservableProperty]
    private double _gravity = 9.8;

    [ObservableProperty]
    private string _simulationTime = "0.00s";

    // 物理画布控件的引用（通过 View 设置）
    public Action? OnPlayPause { get; set; }
    public Action? OnReset { get; set; }
    public Action<double>? OnGravityChangedAction { get; set; }

    [RelayCommand]
    private void PlayPause()
    {
        IsRunning = !IsRunning;
        OnPlayPause?.Invoke();
    }

    [RelayCommand]
    private void Reset()
    {
        IsRunning = false;
        SimulationTime = "0.00s";
        OnReset?.Invoke();
    }

    [RelayCommand]
    private void SetEarthGravity()
    {
        Gravity = 9.8;
    }

    [RelayCommand]
    private void SetMoonGravity()
    {
        Gravity = 1.62;
    }

    [RelayCommand]
    private void SetMarsGravity()
    {
        Gravity = 3.71;
    }

    [RelayCommand]
    private void SetJupiterGravity()
    {
        Gravity = 24.79;
    }

    [RelayCommand]
    private void SetZeroGravity()
    {
        Gravity = 0.0;
    }

    partial void OnGravityChanged(double value)
    {
        OnGravityChangedAction?.Invoke(value);
    }
}
