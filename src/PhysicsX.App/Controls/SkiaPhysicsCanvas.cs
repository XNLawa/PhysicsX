using System;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
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
        context.Custom(new SkiaRenderOp(_engine, Bounds, _zoom, _cameraOffset));
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

    public void SyncSceneObjects(System.Collections.ObjectModel.ObservableCollection<ViewModels.SceneObjectViewModel> sceneObjects)
    {
        if (_engine == null) return;
        _engine.Clear();
        foreach (var sceneObj in sceneObjects)
        {
            var rigidBody = ConvertToRigidBody(sceneObj.ToSceneObject());
            if (rigidBody != null) _engine.AddObject(rigidBody);
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

        public SkiaRenderOp(MechanicsEngine engine, Rect bounds, float zoom, Vector2 cameraOffset)
        {
            _engine = engine;
            _bounds = bounds;
            _zoom = zoom;
            _cameraOffset = cameraOffset;
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

            canvas.Clear(new SKColor(26, 26, 38));
            canvas.Save();

            float width = (float)_bounds.Width;
            float height = (float)_bounds.Height;
            canvas.Translate(width / 2, height / 2);
            canvas.Scale(1, -1);
            canvas.Scale(_zoom, _zoom);
            canvas.Translate(_cameraOffset.X, _cameraOffset.Y);

            foreach (var obj in _engine.Objects)
            {
                if (obj is RigidBody rb) RenderBody(canvas, rb);
            }

            canvas.Restore();
        }

        private static void RenderBody(SKCanvas canvas, RigidBody body)
        {
            if (body.Shape == null) return;

            canvas.Save();
            canvas.Translate(body.Position.X, body.Position.Y);
            canvas.RotateDegrees(body.Rotation * 180f / MathF.PI);

            var color = body.IsStatic ? new SKColor(51, 204, 51) : new SKColor(77, 153, 255);
            using var paint = new SKPaint { Color = color, IsAntialias = true, Style = SKPaintStyle.Fill };
            using var stroke = new SKPaint { Color = color, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 0.1f };

            if (body.Shape is CircleShape circle)
            {
                canvas.DrawCircle(0, 0, circle.Radius, paint);
                stroke.Color = SKColors.Yellow;
                stroke.StrokeWidth = 0.05f;
                canvas.DrawLine(0, 0, circle.Radius, 0, stroke);
            }
            else if (body.Shape is BoxShape box)
            {
                var rect = new SKRect(-box.Width / 2, -box.Height / 2, box.Width / 2, box.Height / 2);
                canvas.DrawRect(rect, stroke);
            }

            canvas.Restore();
        }
    }
}
