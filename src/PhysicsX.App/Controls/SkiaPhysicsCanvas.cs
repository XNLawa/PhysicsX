using System;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using PhysicsX.Core.Collision;
using PhysicsX.Core.Engines;
using PhysicsX.Core.Models;
using PhysicsX.Core.Utils;
using SkiaSharp;

namespace PhysicsX.App.Controls;

public class SkiaPhysicsCanvas : Control
{
    private MechanicsEngine? _engine;
    private DispatcherTimer? _timer;
    private bool _isRunning;
    private readonly Logger _logger = Logger.Instance;
    private float _zoom = 20f;
    private Vector2 _cameraOffset = Vector2.Zero;

    // 交互绘图模式
    private string _drawMode = "Select"; // Select, Circle, Box, Ground
    private Vector2? _drawStartPos;
    private Vector2? _drawCurrentPos;

    public Action<RigidBody>? OnObjectCreated { get; set; }

    public SkiaPhysicsCanvas()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(1000.0 / 60.0)
        };
        _timer.Tick += OnTimerTick;

        _engine = new MechanicsEngine { Gravity = 9.8 };
        _logger.Info("Physics engine initialized with gravity = 9.8", "SkiaPhysicsCanvas");

        CreateSampleScene();
        _logger.Info($"Skia canvas initialized. Scene has {_engine.Objects.Count} objects", "SkiaPhysicsCanvas");
    }

    public void SetDrawMode(string mode)
    {
        _drawMode = mode;
        _drawStartPos = null;
        _drawCurrentPos = null;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var point = e.GetPosition(this);
        var worldPos = ScreenToWorld(point);

        if (_drawMode == "Circle")
        {
            // 点击生成小球
            CreateCircleAtPosition(worldPos);
            _drawStartPos = null;
            _drawCurrentPos = null;
        }
        else if (_drawMode == "Box" || _drawMode == "Ground")
        {
            // 开始绘制矩形/地面
            _drawStartPos = worldPos;
            _drawCurrentPos = worldPos;
        }

        InvalidateVisual();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_drawStartPos.HasValue && (_drawMode == "Box" || _drawMode == "Ground"))
        {
            var point = e.GetPosition(this);
            _drawCurrentPos = ScreenToWorld(point);
            InvalidateVisual();
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_drawStartPos.HasValue && _drawCurrentPos.HasValue)
        {
            if (_drawMode == "Box")
            {
                CreateBoxFromDrag(_drawStartPos.Value, _drawCurrentPos.Value);
            }
            else if (_drawMode == "Ground")
            {
                CreateGroundFromDrag(_drawStartPos.Value, _drawCurrentPos.Value);
            }

            _drawStartPos = null;
            _drawCurrentPos = null;
            InvalidateVisual();
        }
    }

    private Vector2 ScreenToWorld(Point screenPoint)
    {
        float width = (float)Bounds.Width;
        float height = (float)Bounds.Height;

        // 屏幕坐标转世界坐标
        float x = ((float)screenPoint.X - width / 2) / _zoom - _cameraOffset.X;
        float y = -((float)screenPoint.Y - height / 2) / _zoom - _cameraOffset.Y;

        return new Vector2(x, y);
    }

    private void CreateCircleAtPosition(Vector2 pos)
    {
        var ball = new RigidBody($"Ball {_engine?.Objects.Count + 1}")
        {
            Mass = 1.0,
            Position = pos,
            Restitution = 0.8,
            Friction = 0.3,
            UseGravity = true,
            Shape = new CircleShape(0.5f)
        };

        _engine?.AddObject(ball);
        OnObjectCreated?.Invoke(ball);
        _logger.Info($"Created ball at {pos}", "SkiaPhysicsCanvas");
    }

    private void CreateBoxFromDrag(Vector2 start, Vector2 end)
    {
        var center = (start + end) / 2;
        var width = Math.Abs(end.X - start.X);
        var height = Math.Abs(end.Y - start.Y);

        if (width < 0.1f || height < 0.1f) return; // 太小不创建

        var box = new RigidBody($"Box {_engine?.Objects.Count + 1}")
        {
            Mass = 2.0,
            Position = center,
            Restitution = 0.5,
            Friction = 0.4,
            UseGravity = true,
            Shape = new BoxShape(width, height)
        };

        _engine?.AddObject(box);
        OnObjectCreated?.Invoke(box);
        _logger.Info($"Created box at {center}, size {width}x{height}", "SkiaPhysicsCanvas");
    }

    private void CreateGroundFromDrag(Vector2 start, Vector2 end)
    {
        // 地面始终是水平直线
        var centerX = (start.X + end.X) / 2;
        var y = (start.Y + end.Y) / 2;
        var width = Math.Abs(end.X - start.X);

        if (width < 0.5f) width = 10.0f; // 最小宽度

        var ground = new RigidBody($"Ground {_engine?.Objects.Count + 1}")
        {
            Position = new Vector2(centerX, y),
            IsStatic = true,
            Shape = new BoxShape(width, 0.2f) // 固定高度
        };

        _engine?.AddObject(ground);
        OnObjectCreated?.Invoke(ground);
        _logger.Info($"Created ground at ({centerX}, {y}), width {width}", "SkiaPhysicsCanvas");
    }

    private void CreateSampleScene()
    {
        if (_engine == null || _engine.Objects.Count > 0) return;

        var ground = new RigidBody("Ground")
        {
            Position = new Vector2(0, 8),
            IsStatic = true,
            Shape = new BoxShape(30, 1)
        };
        _engine.AddObject(ground);

        var ball1 = new RigidBody("Ball 1")
        {
            Mass = 1.0,
            Position = new Vector2(-3, 0),
            Restitution = 0.8,
            Friction = 0.3,
            UseGravity = true,
            Shape = new CircleShape(0.5f)
        };
        _engine.AddObject(ball1);

        var ball2 = new RigidBody("Ball 2")
        {
            Mass = 1.5,
            Position = new Vector2(3, 2),
            Restitution = 0.7,
            Friction = 0.4,
            UseGravity = true,
            Shape = new CircleShape(0.6f)
        };
        _engine.AddObject(ball2);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (_engine == null) return;
        context.Custom(new SkiaRenderOp(_engine, Bounds, _zoom, _cameraOffset, _drawStartPos, _drawCurrentPos, _drawMode, _objectColors));
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (_isRunning && _engine != null)
        {
            _engine.Step(1.0 / 60.0);
            InvalidateVisual();
        }
    }

    public void Start()
    {
        _isRunning = true;
        _timer?.Start();
    }

    public void Pause()
    {
        _isRunning = false;
    }

    public void Reset()
    {
        _engine?.Reset();
        foreach (var obj in _engine?.Objects ?? Array.Empty<PhysicsX.Core.Interfaces.IPhysicsObject>())
        {
            if (obj is RigidBody rb)
            {
                rb.Position = rb.Name switch
                {
                    "Ball 1" => new Vector2(-3, 0),
                    "Ball 2" => new Vector2(3, 2),
                    _ => rb.Position
                };
            }
        }
        InvalidateVisual();
    }

    public void SetGravity(double gravity)
    {
        if (_engine != null) _engine.Gravity = gravity;
    }

    public double GetGravity() => _engine?.Gravity ?? 9.8;

    public void LoadScene(Models.SceneProject project)
    {
        if (_engine == null) return;
        _logger.Info($"Loading scene: {project.Name}", "SkiaPhysicsCanvas");
        _engine.Clear();
        foreach (var sceneObj in project.Objects)
        {
            var rigidBody = ConvertToRigidBody(sceneObj);
            if (rigidBody != null) _engine.AddObject(rigidBody);
        }
        _engine.Gravity = project.Gravity;
        InvalidateVisual();
    }

    public void ClearScene()
    {
        _engine?.Clear();
        InvalidateVisual();
    }

    // 用于存储物体的渲染颜色信息
    private Dictionary<string, (string fillColor, string strokeColor)> _objectColors = new();

    public void SyncSceneObjects(System.Collections.ObjectModel.ObservableCollection<ViewModels.SceneObjectViewModel> sceneObjects)
    {
        if (_engine == null) return;
        _engine.Clear();
        _objectColors.Clear();

        foreach (var sceneObj in sceneObjects)
        {
            var rigidBody = ConvertToRigidBody(sceneObj.ToSceneObject());
            if (rigidBody != null)
            {
                _engine.AddObject(rigidBody);
                _objectColors[rigidBody.Id] = (sceneObj.Color ?? "#DCDCDC", sceneObj.StrokeColor ?? "#000000");
            }
        }
        InvalidateVisual();
    }

    private RigidBody? ConvertToRigidBody(Models.SceneObject sceneObj)
    {
        var rb = new RigidBody(sceneObj.Name)
        {
            Position = sceneObj.Position.ToVector2(),
            Velocity = sceneObj.Velocity.ToVector2(),
            Mass = sceneObj.Mass,
            Restitution = sceneObj.Restitution,
            Friction = sceneObj.Friction,
            IsStatic = sceneObj.IsStatic,
            UseGravity = sceneObj.UseGravity
        };

        rb.Shape = sceneObj.Type switch
        {
            "Circle" when sceneObj.Radius.HasValue => new CircleShape(sceneObj.Radius.Value),
            "Box" when sceneObj.Width.HasValue && sceneObj.Height.HasValue => new BoxShape(sceneObj.Width.Value, sceneObj.Height.Value),
            _ => null
        };

        return rb;
    }

    private class SkiaRenderOp : ICustomDrawOperation
    {
        private readonly MechanicsEngine _engine;
        private readonly Rect _bounds;
        private readonly float _zoom;
        private readonly Vector2 _cameraOffset;
        private readonly Vector2? _drawStartPos;
        private readonly Vector2? _drawCurrentPos;
        private readonly string _drawMode;
        private readonly Dictionary<string, (string fillColor, string strokeColor)> _objectColors;

        public SkiaRenderOp(MechanicsEngine engine, Rect bounds, float zoom, Vector2 cameraOffset,
            Vector2? drawStartPos, Vector2? drawCurrentPos, string drawMode,
            Dictionary<string, (string fillColor, string strokeColor)> objectColors)
        {
            _engine = engine;
            _bounds = bounds;
            _zoom = zoom;
            _cameraOffset = cameraOffset;
            _drawStartPos = drawStartPos;
            _drawCurrentPos = drawCurrentPos;
            _drawMode = drawMode;
            _objectColors = objectColors;
        }

        public void Dispose() { }
        public Rect Bounds => _bounds;
        public bool HitTest(Point p) => false;
        public bool Equals(ICustomDrawOperation? other) => false;

        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature == null) return;

            using var lease = leaseFeature.Lease();
            var canvas = lease.SkCanvas;

            canvas.Clear(new SKColor(248, 248, 248)); // 浅灰色背景，类似教材纸张
            canvas.Save();

            float width = (float)_bounds.Width;
            float height = (float)_bounds.Height;
            canvas.Translate(width / 2, height / 2);
            canvas.Scale(1, -1);
            canvas.Scale(_zoom, _zoom);
            canvas.Translate(_cameraOffset.X, _cameraOffset.Y);

            foreach (var obj in _engine.Objects)
            {
                if (obj is RigidBody rb)
                {
                    // 获取自定义颜色，如果没有则使用默认
                    var colors = _objectColors.TryGetValue(rb.Id, out var c) ? c : ("#DCDCDC", "#000000");
                    RenderBody(canvas, rb, colors.Item1, colors.Item2);
                }
            }

            // 绘制预览（正在拖拽绘制中）
            if (_drawStartPos.HasValue && _drawCurrentPos.HasValue)
            {
                RenderDrawPreview(canvas, _drawStartPos.Value, _drawCurrentPos.Value, _drawMode);
            }

            canvas.Restore();
        }

        private static void RenderDrawPreview(SKCanvas canvas, Vector2 start, Vector2 current, string mode)
        {
            using var previewPaint = new SKPaint
            {
                Color = new SKColor(100, 100, 100, 128), // 半透明灰色
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            using var previewStroke = new SKPaint
            {
                Color = new SKColor(0, 0, 0, 200), // 黑色边框
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 0.05f,
                PathEffect = SKPathEffect.CreateDash(new float[] { 0.2f, 0.1f }, 0) // 虚线
            };

            if (mode == "Ground")
            {
                // 地面：始终水平
                var centerX = (start.X + current.X) / 2;
                var y = (start.Y + current.Y) / 2;
                var width = Math.Abs(current.X - start.X);
                if (width < 0.5f) width = 10.0f;

                var rect = new SKRect(centerX - width / 2, y - 0.1f, centerX + width / 2, y + 0.1f);
                canvas.DrawRect(rect, previewPaint);
                canvas.DrawRect(rect, previewStroke);
            }
            else if (mode == "Box")
            {
                var center = (start + current) / 2;
                var width = Math.Abs(current.X - start.X);
                var height = Math.Abs(current.Y - start.Y);

                canvas.Save();
                canvas.Translate(center.X, center.Y);
                var rect = new SKRect(-width / 2, -height / 2, width / 2, height / 2);
                canvas.DrawRect(rect, previewPaint);
                canvas.DrawRect(rect, previewStroke);
                canvas.Restore();
            }
        }

        private static void RenderBody(SKCanvas canvas, RigidBody body, string fillColorHex, string strokeColorHex)
        {
            if (body.Shape == null) return;

            canvas.Save();
            canvas.Translate(body.Position.X, body.Position.Y);
            canvas.RotateDegrees(body.Rotation * 180f / MathF.PI);

            // 解析颜色
            var fillColor = ParseColor(fillColorHex);
            var strokeColor = ParseColor(strokeColorHex);

            using var fillPaint = new SKPaint
            {
                Color = fillColor,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            using var strokePaint = new SKPaint
            {
                Color = strokeColor,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 0.05f  // 细线条，类似教材插图
            };

            if (body.Shape is CircleShape circle)
            {
                // 绘制填充
                canvas.DrawCircle(0, 0, circle.Radius, fillPaint);
                // 绘制边框
                canvas.DrawCircle(0, 0, circle.Radius, strokePaint);

                // 绘制方向指示线（从圆心到边缘）
                using var directionPaint = new SKPaint
                {
                    Color = strokeColor,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 0.02f
                };
                canvas.DrawLine(0, 0, circle.Radius, 0, directionPaint);
            }
            else if (body.Shape is BoxShape box)
            {
                var rect = new SKRect(-box.Width / 2, -box.Height / 2, box.Width / 2, box.Height / 2);
                // 绘制填充
                canvas.DrawRect(rect, fillPaint);
                // 绘制边框
                canvas.DrawRect(rect, strokePaint);
            }

            canvas.Restore();
        }

        private static SKColor ParseColor(string hexColor)
        {
            try
            {
                if (string.IsNullOrEmpty(hexColor)) return new SKColor(220, 220, 220);

                hexColor = hexColor.TrimStart('#');
                if (hexColor.Length == 6)
                {
                    byte r = Convert.ToByte(hexColor.Substring(0, 2), 16);
                    byte g = Convert.ToByte(hexColor.Substring(2, 2), 16);
                    byte b = Convert.ToByte(hexColor.Substring(4, 2), 16);
                    return new SKColor(r, g, b);
                }
            }
            catch
            {
                // 解析失败返回默认灰色
            }
            return new SKColor(220, 220, 220);
        }
    }
}
