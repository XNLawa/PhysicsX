using System;
using System.Numerics;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using PhysicsX.Core.Collision;
using PhysicsX.Core.Engines;
using PhysicsX.Core.Models;
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

        _gl = GL.GetApi(gl.GetProcAddress);

        // 初始化渲染器
        InitializeRenderers();

        // 初始化物理引擎
        _engine = new MechanicsEngine { Gravity = 9.8 };

        // 添加示例场景
        CreateSampleScene();
    }

    private void InitializeRenderers()
    {
        if (_gl == null) return;

        // 加载 Shader
        string vertexShader = @"
#version 330 core
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

        string fragmentShader = @"
#version 330 core
in vec4 vColor;
out vec4 FragColor;

void main()
{
    FragColor = vColor;
}
";

        _shader = new ShaderProgram(_gl, vertexShader, fragmentShader);
        _circleRenderer = new CircleRenderer(_gl);
        _lineRenderer = new LineRenderer(_gl);
    }

    private void CreateSampleScene()
    {
        if (_engine == null) return;

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
            return;

        // 清屏
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
        foreach (var obj in _engine.Objects)
        {
            if (obj is RigidBody rb)
            {
                RenderRigidBody(rb);
            }
        }

        // 请求下一帧
        RequestNextFrameRendering();
    }

    private void RenderRigidBody(RigidBody body)
    {
        if (_gl == null || _shader == null || _circleRenderer == null || _lineRenderer == null)
            return;

        var modelMatrix = Matrix4x4.CreateTranslation(new Vector3(body.Position, 0));
        _shader.SetUniform("uModel", modelMatrix);

        if (body.Shape is CircleShape circle)
        {
            // 绘制圆形
            _circleRenderer.Draw(Vector2.Zero, circle.Radius, new Vector4(0.3f, 0.6f, 1.0f, 1.0f));

            // 绘制方向指示线（显示旋转）
            var direction = new Vector2(MathF.Cos(body.Rotation), MathF.Sin(body.Rotation)) * circle.Radius;
            _lineRenderer.DrawLine(Vector2.Zero, direction, new Vector4(1, 1, 0, 1), 2.0f);
        }
        else if (body.Shape is BoxShape box)
        {
            // 绘制矩形边框
            var color = body.IsStatic ? new Vector4(0.2f, 0.8f, 0.2f, 1.0f) : new Vector4(0.8f, 0.3f, 0.3f, 1.0f);
            _lineRenderer.DrawRectangle(Vector2.Zero, box.Width, box.Height, color, 3.0f);
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

    protected override void OnOpenGlDeinit(GlInterface gl)
    {
        base.OnOpenGlDeinit(gl);

        _timer?.Stop();
        _shader?.Dispose();
        _circleRenderer?.Dispose();
        _lineRenderer?.Dispose();
    }
}