using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Numerics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhysicsX.App.Models;
using PhysicsX.Core.Collision;
using PhysicsX.Core.Models;

namespace PhysicsX.App.ViewModels;

public partial class SceneEditorViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _projectName = "未命名项目";

    [ObservableProperty]
    private ObservableCollection<SceneObjectViewModel> _sceneObjects = new();

    [ObservableProperty]
    private SceneObjectViewModel? _selectedObject;

    private string _toolMode = "Select";
    public string ToolMode
    {
        get => _toolMode;
        set
        {
            if (_toolMode != value)
            {
                _toolMode = value;
                OnPropertyChanged(nameof(ToolMode));
            }
        }
    }

    // 回调：用于通知 View 更新场景
    public Action<SceneProject>? OnSceneLoaded { get; set; }
    public Action? OnSceneCleared { get; set; }
    public Action<string>? OnToolModeChanged { get; set; }

    [RelayCommand]
    private void AddCircle()
    {
        ToolMode = "Circle";
        OnToolModeChanged?.Invoke("Circle");
    }

    [RelayCommand]
    private void AddBox()
    {
        ToolMode = "Box";
        OnToolModeChanged?.Invoke("Box");
    }

    [RelayCommand]
    private void AddGround()
    {
        ToolMode = "Ground";
        OnToolModeChanged?.Invoke("Ground");
    }

    [RelayCommand]
    private void AddSpring()
    {
        ToolMode = "Spring";
        OnToolModeChanged?.Invoke("Spring");
    }

    [RelayCommand]
    private void AddRope()
    {
        ToolMode = "Rope";
        OnToolModeChanged?.Invoke("Rope");
    }

    [RelayCommand]
    private void AddRamp()
    {
        ToolMode = "Ramp";
        OnToolModeChanged?.Invoke("Ramp");
    }

    [RelayCommand]
    private void AddCapsule()
    {
        ToolMode = "Capsule";
        OnToolModeChanged?.Invoke("Capsule");
    }

    public void AddObjectFromPhysics(RigidBody rb)
    {
        var obj = new SceneObjectViewModel
        {
            Id = rb.Id,
            Name = rb.Name,
            Type = rb.Shape switch
            {
                CircleShape => "Circle",
                BoxShape => rb.IsStatic ? "Ground" : "Box",
                _ => "Unknown"
            },
            Position = rb.Position,
            Velocity = rb.Velocity,
            Mass = rb.Mass,
            Restitution = rb.Restitution,
            Friction = rb.Friction,
            IsStatic = rb.IsStatic,
            UseGravity = rb.UseGravity,
            Radius = (rb.Shape as CircleShape)?.Radius,
            Width = (rb.Shape as BoxShape)?.Width,
            Height = (rb.Shape as BoxShape)?.Height,
            Color = "#DCDCDC" // 默认灰色
        };
        SceneObjects.Add(obj);
        ToolMode = "Select"; // 创建后回到选择模式
    }

    [RelayCommand]
    private void DeleteSelected()
    {
        if (SelectedObject != null)
        {
            SceneObjects.Remove(SelectedObject);
            SelectedObject = null;
        }
    }

    [RelayCommand]
    private void DuplicateSelected()
    {
        if (SelectedObject != null)
        {
            var clone = SelectedObject.Clone();
            clone.Id = Guid.NewGuid().ToString();
            clone.Name = $"{SelectedObject.Name} (副本)";
            clone.Position = new Vector2(clone.Position.X + 1, clone.Position.Y + 1);
            SceneObjects.Add(clone);
            SelectedObject = clone;
        }
    }

    [RelayCommand]
    private void ClearScene()
    {
        SceneObjects.Clear();
        SelectedObject = null;
        OnSceneCleared?.Invoke();
    }

    [RelayCommand]
    private async Task SaveProject()
    {
        var project = new SceneProject
        {
            Name = ProjectName,
            Objects = SceneObjects.Select(o => o.ToSceneObject()).ToList(),
            ModifiedAt = DateTime.Now
        };

        var json = JsonSerializer.Serialize(project, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        // TODO: 实现文件保存对话框
        var fileName = $"{ProjectName.Replace(" ", "_")}.physicsx";
        await File.WriteAllTextAsync(fileName, json);
    }

    [RelayCommand]
    private async Task LoadProject(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            return;

        var json = await File.ReadAllTextAsync(filePath);
        var project = JsonSerializer.Deserialize<SceneProject>(json);

        if (project == null)
            return;

        ProjectName = project.Name;
        SceneObjects.Clear();

        foreach (var obj in project.Objects)
        {
            SceneObjects.Add(SceneObjectViewModel.FromSceneObject(obj));
        }

        OnSceneLoaded?.Invoke(project);
    }

    public SceneProject GetCurrentProject()
    {
        return new SceneProject
        {
            Name = ProjectName,
            Objects = SceneObjects.Select(o => o.ToSceneObject()).ToList()
        };
    }
}

/// <summary>
/// 场景对象 ViewModel
/// </summary>
public partial class SceneObjectViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string _name = "Object";

    [ObservableProperty]
    private string _type = "Circle";

    [ObservableProperty]
    private Vector2 _position = Vector2.Zero;

    public float PositionX
    {
        get => Position.X;
        set
        {
            Position = new Vector2(value, Position.Y);
            OnPropertyChanged(nameof(Position));
        }
    }

    public float PositionY
    {
        get => Position.Y;
        set
        {
            Position = new Vector2(Position.X, value);
            OnPropertyChanged(nameof(Position));
        }
    }

    [ObservableProperty]
    private Vector2 _velocity = Vector2.Zero;

    [ObservableProperty]
    private double _mass = 1.0;

    [ObservableProperty]
    private double _restitution = 0.8;

    [ObservableProperty]
    private double _friction = 0.3;

    [ObservableProperty]
    private bool _isStatic = false;

    [ObservableProperty]
    private bool _useGravity = true;

    [ObservableProperty]
    private float? _radius;

    [ObservableProperty]
    private float? _width;

    [ObservableProperty]
    private float? _height;

    [ObservableProperty]
    private string _color = "#DCDCDC"; // 默认灰色填充

    [ObservableProperty]
    private string _strokeColor = "#000000"; // 默认黑色边框

    // 热力学属性
    [ObservableProperty]
    private double _temperature = 20.0;

    [ObservableProperty]
    private bool _enableThermal = false;

    [ObservableProperty]
    private string _material = "Aluminum"; // Aluminum, Iron, Copper, Wood, Rubber, Ice

    // 扩展形状属性
    [ObservableProperty]
    private float? _angle; // 斜面角度

    [ObservableProperty]
    private float? _stiffness; // 弹簧刚度

    [ObservableProperty]
    private float? _damping; // 阻尼

    [ObservableProperty]
    private float? _maxLength; // 绳索最大长度

    [ObservableProperty]
    private float? _thickness; // 厚度

    public SceneObject ToSceneObject()
    {
        return new SceneObject
        {
            Id = Id,
            Name = Name,
            Type = Type,
            Position = new Vector2Data(Position),
            Velocity = new Vector2Data(Velocity),
            Mass = Mass,
            Restitution = Restitution,
            Friction = Friction,
            IsStatic = IsStatic,
            UseGravity = UseGravity,
            Radius = Radius,
            Width = Width,
            Height = Height,
            Color = Color,
            StrokeColor = StrokeColor,
            Temperature = Temperature,
            EnableThermal = EnableThermal,
            Material = Material,
            Angle = Angle,
            Stiffness = Stiffness,
            Damping = Damping,
            MaxLength = MaxLength,
            Thickness = Thickness
        };
    }

    public static SceneObjectViewModel FromSceneObject(SceneObject obj)
    {
        return new SceneObjectViewModel
        {
            Id = obj.Id,
            Name = obj.Name,
            Type = obj.Type,
            Position = obj.Position.ToVector2(),
            Velocity = obj.Velocity.ToVector2(),
            Mass = obj.Mass,
            Restitution = obj.Restitution,
            Friction = obj.Friction,
            IsStatic = obj.IsStatic,
            UseGravity = obj.UseGravity,
            Radius = obj.Radius,
            Width = obj.Width,
            Height = obj.Height,
            Color = obj.Color ?? "#DCDCDC",
            StrokeColor = obj.StrokeColor ?? "#000000",
            Temperature = obj.Temperature ?? 20.0,
            EnableThermal = obj.EnableThermal ?? false,
            Material = obj.Material ?? "Aluminum",
            Angle = obj.Angle,
            Stiffness = obj.Stiffness,
            Damping = obj.Damping,
            MaxLength = obj.MaxLength,
            Thickness = obj.Thickness
        };
    }

    public SceneObjectViewModel Clone()
    {
        return new SceneObjectViewModel
        {
            Name = Name,
            Type = Type,
            Position = Position,
            Velocity = Velocity,
            Mass = Mass,
            Restitution = Restitution,
            Friction = Friction,
            IsStatic = IsStatic,
            UseGravity = UseGravity,
            Radius = Radius,
            Width = Width,
            Height = Height,
            Color = Color,
            StrokeColor = StrokeColor,
            Temperature = Temperature,
            EnableThermal = EnableThermal,
            Material = Material,
            Angle = Angle,
            Stiffness = Stiffness,
            Damping = Damping,
            MaxLength = MaxLength,
            Thickness = Thickness
        };
    }
}
