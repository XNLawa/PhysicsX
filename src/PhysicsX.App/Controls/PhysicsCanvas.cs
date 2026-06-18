using System;
using System.Numerics;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using PhysicsX.Core.Collision;
using PhysicsX.Core.Engines;
using PhysicsX.Core.Models;
using PhysicsX.Core.Utils;
using PhysicsX.Rendering.Primitives;
using PhysicsX.Rendering.Renderer;
using Silk.NET.OpenGL;

namespace PhysicsX.App.Controls;

/// <summary>
/// OpenGL 物理画布控件
/// </summary>
public class PhysicsCanvas : OpenGlControlBase
{
    private GL? _gl;
    private ShaderProgram? _shader;
    private CircleRenderer? _circleRenderer;
    private LineRenderer? _lineRenderer;
    private MechanicsEngine? _engine;
    private DispatcherTimer? _timer;
    private bool _isRunning;
    private readonly Logger _logger = Logger.Instance;

    // 视图变换
    private Matrix4x4 _projectionMatrix;
    private Matrix4x4 _viewMatrix;
    private float _zoom = 20f; // 每单位多少像素
    private Vector2 _cameraOffset = Vector2.Zero;

    public PhysicsCanvas()
    {
        // 初始化定时器（60 FPS）
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(1000.0 / 60.0)
        };
        _timer.Tick += OnTimerTick;
    }

    protected override void OnOpenGlInit(GlInterface gl)
    {
        base.OnOpenGlInit(gl);

        _logger.Info("PhysicsCanvas OpenGL initialization started", "PhysicsCanvas");

        _gl = GL.GetApi(gl.GetProcAddress);

        // 记录 OpenGL 信息
        var glVersion = _gl.GetStringS(StringName.Version);
        var glVendor = _gl.GetStringS(StringName.Vendor);
        var glRenderer = _gl.GetStringS(StringName.Renderer);

        _logger.Info($"OpenGL Version: {glVersion}", "PhysicsCanvas");
        _logger.Info($"OpenGL Vendor: {glVendor}", "PhysicsCanvas");
        _logger.Info($"OpenGL Renderer: {glRenderer}", "PhysicsCanvas");

        // 初始化渲染器
        InitializeRenderers();

        // 初始化物理引擎
        _engine = new MechanicsEngine { Gravity = 9.8 };
        _logger.Info("Physics engine initialized with gravity = 9.8", "PhysicsCanvas");

        // 添加示例场景
        CreateSampleScene();

        _logger.Info($"OpenGL initialization complete. Scene has {_engine.Objects.Count} objects", "PhysicsCanvas");
    }

    private void InitializeRenderers()
    {
        if (_gl == null) return;

        _logger.Debug("Initializing renderers...", "PhysicsCanvas");

        // 加载 Shader (OpenGL ES 3.0 兼容版本)
        string vertexShader = @"#version 300 es
precision highp float;

layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec4 aColor;

uniform mat4 uProjection;
uniform mat4 uView;
uniform mat4 uModel;

out vec4 vColor;

void main()
{
    gl_Position = uProjection * uView * uModel * vec4(aPosition, 0.0, 1.0);
    vColor = aColor;
}
";

        string fragmentShader = @"#version 300 es
precision highp float;

in vec4 vColor;
out vec4 FragColor;

void main()
{
    FragColor = vColor;
}
";

        try
        {
            _shader = new ShaderProgram(_gl, vertexShader, fragmentShader);
            _circleRenderer = new CircleRenderer(_gl);
            _lineRenderer = new LineRenderer(_gl);
            _logger.Info("Renderers initialized successfully", "PhysicsCanvas");
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to initialize renderers", ex, "PhysicsCanvas");
            throw;
        }
    }

    private void CreateSampleScene()
    {
        if (_engine == null) return;

        // 只在引擎为空时创建示例场景
        if (_engine.Objects.Count > 0) return;

        // 地面
        var ground = new RigidBody("Ground")
        {
            Position = new Vector2(0, 8),
            IsStatic = true,
            Shape = new BoxShape(30, 1)
        };
        _engine.AddObject(ground);

        // 第一个小球
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

        // 第二个小球
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

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        if (_gl == null || _shader == null || _circleRenderer == null || _lineRenderer == null || _engine == null)
        {
            _logger.Warning("Render skipped: components not initialized", "PhysicsCanvas");
            return;
        }

        // 清屏 - 深灰色背景
        _gl.ClearColor(0.1f, 0.1f, 0.15f, 1.0f);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        // 设置投影矩阵（正交投影）
        float aspect = (float)Bounds.Width / (float)Bounds.Height;
        float viewWidth = (float)Bounds.Width / _zoom;
        float viewHeight = (float)Bounds.Height / _zoom;

        _projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(
            -viewWidth / 2f, viewWidth / 2f,
            viewHeight / 2f, -viewHeight / 2f,
            -1f, 1f
        );

        _viewMatrix = Matrix4x4.CreateTranslation(new Vector3(_cameraOffset, 0));

        _shader.Use();
        _shader.SetUniform("uProjection", _projectionMatrix);
        _shader.SetUniform("uView", _viewMatrix);

        // 渲染所有物体
        int renderedCount = 0;
        foreach (var obj in _engine.Objects)
        {
            if (obj is RigidBody rb)
            {
                try
                {
                    RenderRigidBody(rb);
                    renderedCount++;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error rendering {rb.Name}", ex, "PhysicsCanvas");
                }
            }
        }

        if (renderedCount == 0 && _engine.Objects.Count > 0)
        {
            _logger.Warning($"No objects rendered, but engine has {_engine.Objects.Count} objects", "PhysicsCanvas");
        }

        // 请求下一帧
        RequestNextFrameRendering();
    }

    private void RenderRigidBody(RigidBody body)
    {
        if (_gl == null || _shader == null || _circleRenderer == null || _lineRenderer == null)
            return;

        if (body.Shape == null)
        {
            _logger.Warning($"Body {body.Name} has no shape", "PhysicsCanvas");
            return;
        }

        var modelMatrix = Matrix4x4.CreateTranslation(new Vector3(body.Position, 0));
        _shader.SetUniform("uModel", modelMatrix);

        // 根据热力学温度改变颜色
        Vector4 color;
        if (body.Thermal.EnableThermal)
        {
            color = body.Thermal.GetTemperatureColor();
        }
        else
        {
            // 默认颜色
            color = body.IsStatic
                ? new Vector4(0.2f, 0.8f, 0.2f, 1.0f)
                : new Vector4(0.3f, 0.6f, 1.0f, 1.0f);
        }

        // 只在初次渲染时记录日志
        // _logger.Debug($"Rendering {body.Name} ({body.Shape.GetType().Name}) at ({body.Position.X:F2}, {body.Position.Y:F2}) color=({color.X:F2},{color.Y:F2},{color.Z:F2})", "PhysicsCanvas");

        if (body.Shape is CircleShape circle)
        {
            // 绘制圆形 - 创建包含缩放的模型矩阵
            var scaleMatrix = Matrix4x4.CreateScale(circle.Radius, circle.Radius, 1.0f);
            var translationMatrix = Matrix4x4.CreateTranslation(new Vector3(body.Position, 0));
            var modelMatrix2 = scaleMatrix * translationMatrix;
            _shader.SetUniform("uModel", modelMatrix2);

            _circleRenderer.Draw(Vector2.Zero, circle.Radius, color);

            // 绘制方向指示线（显示旋转）
            var direction = new Vector2(MathF.Cos(body.Rotation), MathF.Sin(body.Rotation)) * circle.Radius;
            _shader.SetUniform("uModel", Matrix4x4.Identity);
            _lineRenderer.DrawLine(body.Position, body.Position + direction, new Vector4(1, 1, 0, 1), 2.0f);
        }
        else if (body.Shape is BoxShape box)
        {
            // 绘制矩形边框
            _lineRenderer.DrawRectangle(Vector2.Zero, box.Width, box.Height, color, 3.0f);
        }
        else if (body.Shape is SpringShape spring)
        {
            // 绘制弹簧（波浪线）
            var springColor = new Vector4(1.0f, 0.6f, 0.0f, 1.0f);
            _lineRenderer.DrawLine(new Vector2(-spring.RestLength/2, 0), new Vector2(spring.RestLength/2, 0), springColor, 3.0f);
        }
        else if (body.Shape is RopeShape rope)
        {
            // 绘制绳索（直线）
            var ropeColor = new Vector4(0.55f, 0.27f, 0.07f, 1.0f);
            _lineRenderer.DrawLine(new Vector2(-rope.MaxLength/2, 0), new Vector2(rope.MaxLength/2, 0), ropeColor, 2.0f);
        }
        else if (body.Shape is RampShape ramp)
        {
            // 绘制斜面（三角形）
            var rampColor = new Vector4(0.6f, 0.6f, 0.6f, 1.0f);
            var angleRad = ramp.Angle * MathF.PI / 180f;
            var p1 = new Vector2(-ramp.Width/2, ramp.Height/2);
            var p2 = new Vector2(ramp.Width/2, ramp.Height/2);
            var p3 = new Vector2(ramp.Width/2, -ramp.Height/2);

            _lineRenderer.DrawLine(p1, p2, rampColor, 3.0f);
            _lineRenderer.DrawLine(p2, p3, rampColor, 3.0f);
            _lineRenderer.DrawLine(p3, p1, rampColor, 3.0f);
        }
        else if (body.Shape is CapsuleShape capsule)
        {
            // 绘制胶囊（圆角矩形）
            var capsuleColor = new Vector4(0.65f, 0.55f, 0.95f, 1.0f);

            // 绘制两个圆
            _circleRenderer.Draw(new Vector2(-capsule.Length/2, 0), capsule.Radius, capsuleColor);
            _circleRenderer.Draw(new Vector2(capsule.Length/2, 0), capsule.Radius, capsuleColor);

            // 绘制连接线
            _lineRenderer.DrawLine(
                new Vector2(-capsule.Length/2, capsule.Radius),
                new Vector2(capsule.Length/2, capsule.Radius),
                capsuleColor, 2.0f);
            _lineRenderer.DrawLine(
                new Vector2(-capsule.Length/2, -capsule.Radius),
                new Vector2(capsule.Length/2, -capsule.Radius),
                capsuleColor, 2.0f);
        }

        // 绘制速度向量（如果物体在运动）
        if (body.Velocity.LengthSquared() > 0.01f && !body.IsStatic)
        {
            var velocityScale = 0.3f;
            var velocityEnd = body.Velocity * velocityScale;

            // 重置模型矩阵以在世界空间绘制
            _shader.SetUniform("uModel", Matrix4x4.Identity);
            _lineRenderer.DrawArrow(
                body.Position,
                body.Position + velocityEnd,
                new Vector4(1, 0.5f, 0, 1),
                2.0f,
                0.2f
            );
        }
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (_isRunning && _engine != null)
        {
            _engine.Step(1.0 / 60.0);
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
    }

    public void SetGravity(double gravity)
    {
        if (_engine != null)
        {
            _engine.Gravity = gravity;
        }
    }

    public double GetGravity()
    {
        return _engine?.Gravity ?? 9.8;
    }

    public void LoadScene(Models.SceneProject project)
    {
        if (_engine == null) return;

        _logger.Info($"Loading scene: {project.Name}", "PhysicsCanvas");

        // 清空当前场景
        _engine.Clear();

        // 加载项目中的对象
        foreach (var sceneObj in project.Objects)
        {
            var rigidBody = ConvertToRigidBody(sceneObj);
            if (rigidBody != null)
            {
                _engine.AddObject(rigidBody);
                _logger.Debug($"Loaded object: {rigidBody.Name} at ({rigidBody.Position.X:F2}, {rigidBody.Position.Y:F2})", "PhysicsCanvas");
            }
        }

        // 设置重力
        _engine.Gravity = project.Gravity;

        _logger.Info($"Scene loaded: {_engine.Objects.Count} objects, gravity = {project.Gravity}", "PhysicsCanvas");
    }

    public void ClearScene()
    {
        if (_engine == null) return;
        _engine.Clear();
        _logger.Info("Scene cleared", "PhysicsCanvas");
    }

    public void SyncSceneObjects(System.Collections.ObjectModel.ObservableCollection<ViewModels.SceneObjectViewModel> sceneObjects)
    {
        if (_engine == null)
        {
            _logger.Warning("Engine is null, cannot sync objects", "PhysicsCanvas");
            return;
        }

        _logger.Info($"Syncing {sceneObjects.Count} scene objects", "PhysicsCanvas");

        // 清空当前引擎对象
        _engine.Clear();

        // 添加场景编辑器中的所有对象
        int successCount = 0;
        foreach (var sceneObj in sceneObjects)
        {
            try
            {
                var rigidBody = ConvertToRigidBody(sceneObj.ToSceneObject());
                if (rigidBody != null)
                {
                    _engine.AddObject(rigidBody);
                    _logger.Debug($"Added: {rigidBody.Name} ({rigidBody.Shape?.GetType().Name}) at ({rigidBody.Position.X:F2}, {rigidBody.Position.Y:F2})", "PhysicsCanvas");
                    successCount++;
                }
                else
                {
                    _logger.Warning($"Failed to convert scene object: {sceneObj.Name}", "PhysicsCanvas");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error adding object {sceneObj.Name}", ex, "PhysicsCanvas");
            }
        }

        _logger.Info($"Sync complete: {successCount}/{sceneObjects.Count} objects added. Total in engine: {_engine.Objects.Count}", "PhysicsCanvas");

        // 强制重绘
        RequestNextFrameRendering();
    }

    private RigidBody? ConvertToRigidBody(Models.SceneObject sceneObj)
    {
        var rigidBody = new RigidBody(sceneObj.Name)
        {
            Position = sceneObj.Position.ToVector2(),
            Velocity = sceneObj.Velocity.ToVector2(),
            Mass = sceneObj.Mass,
            Restitution = sceneObj.Restitution,
            Friction = sceneObj.Friction,
            IsStatic = sceneObj.IsStatic,
            UseGravity = sceneObj.UseGravity
        };

        // 设置形状
        switch (sceneObj.Type)
        {
            case "Circle":
                if (sceneObj.Radius.HasValue)
                {
                    rigidBody.Shape = new CircleShape(sceneObj.Radius.Value);
                }
                break;

            case "Box":
                if (sceneObj.Width.HasValue && sceneObj.Height.HasValue)
                {
                    rigidBody.Shape = new BoxShape(sceneObj.Width.Value, sceneObj.Height.Value);
                }
                break;

            case "Spring":
                if (sceneObj.Width.HasValue && sceneObj.Stiffness.HasValue && sceneObj.Damping.HasValue)
                {
                    rigidBody.Shape = new SpringShape(
                        sceneObj.Width.Value,
                        sceneObj.Stiffness.Value,
                        sceneObj.Damping.Value
                    );
                }
                break;

            case "Rope":
                if (sceneObj.MaxLength.HasValue && sceneObj.Thickness.HasValue)
                {
                    rigidBody.Shape = new RopeShape(
                        sceneObj.MaxLength.Value,
                        sceneObj.Thickness.Value
                    );
                }
                break;

            case "Ramp":
                if (sceneObj.Width.HasValue && sceneObj.Height.HasValue && sceneObj.Angle.HasValue)
                {
                    rigidBody.Shape = new RampShape(
                        sceneObj.Width.Value,
                        sceneObj.Height.Value,
                        sceneObj.Angle.Value
                    );
                }
                break;

            case "Capsule":
                if (sceneObj.Width.HasValue && sceneObj.Radius.HasValue)
                {
                    rigidBody.Shape = new CapsuleShape(
                        sceneObj.Width.Value,
                        sceneObj.Radius.Value
                    );
                }
                break;
        }

        // 设置热力学属性
        if (sceneObj.EnableThermal.HasValue)
        {
            rigidBody.Thermal.EnableThermal = sceneObj.EnableThermal.Value;
        }
        if (sceneObj.Temperature.HasValue)
        {
            rigidBody.Thermal.Temperature = sceneObj.Temperature.Value;
        }
        if (!string.IsNullOrEmpty(sceneObj.Material))
        {
            if (Enum.TryParse<PhysicsX.Core.Physics.ThermalProperties.Material>(sceneObj.Material, out var material))
            {
                rigidBody.Thermal.SetMaterial(material);
            }
        }

        return rigidBody;
    }

    protected override void OnOpenGlDeinit(GlInterface gl)
    {
        base.OnOpenGlDeinit(gl);

        _timer?.Stop();
        _shader?.Dispose();
        _circleRenderer?.Dispose();
        _lineRenderer?.Dispose();
    }
}