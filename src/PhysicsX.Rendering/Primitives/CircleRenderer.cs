using System;
using System.Numerics;
using Silk.NET.OpenGL;

namespace PhysicsX.Rendering.Primitives;

/// <summary>
/// 圆形渲染器
/// </summary>
public class CircleRenderer : IDisposable
{
    private readonly GL _gl;
    private uint _vao;
    private uint _vbo;
    private readonly int _segments;
    private bool _disposed;

    public CircleRenderer(GL gl, int segments = 32)
    {
        _gl = gl;
        _segments = segments;
        InitializeBuffers();
    }

    private unsafe void InitializeBuffers()
    {
        // 生成圆形顶点（使用三角扇）
        int vertexCount = _segments + 2; // 中心点 + 圆周点 + 闭合点
        float[] vertices = new float[vertexCount * 6]; // 每个顶点: x, y, r, g, b, a

        // 中心点
        vertices[0] = 0.0f; // x
        vertices[1] = 0.0f; // y
        vertices[2] = 1.0f; // r
        vertices[3] = 1.0f; // g
        vertices[4] = 1.0f; // b
        vertices[5] = 1.0f; // a

        // 圆周点
        for (int i = 0; i <= _segments; i++)
        {
            float angle = (float)(i * 2.0 * Math.PI / _segments);
            int offset = (i + 1) * 6;

            vertices[offset + 0] = MathF.Cos(angle);
            vertices[offset + 1] = MathF.Sin(angle);
            vertices[offset + 2] = 1.0f; // r
            vertices[offset + 3] = 1.0f; // g
            vertices[offset + 4] = 1.0f; // b
            vertices[offset + 5] = 1.0f; // a
        }

        // 创建 VAO 和 VBO
        _vao = _gl.GenVertexArray();
        _vbo = _gl.GenBuffer();

        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        fixed (float* v = vertices)
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);
        }

        // 位置属性 (location = 0)
        _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 6 * sizeof(float), (void*)0);
        _gl.EnableVertexAttribArray(0);

        // 颜色属性 (location = 1)
        _gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 6 * sizeof(float), (void*)(2 * sizeof(float)));
        _gl.EnableVertexAttribArray(1);

        _gl.BindVertexArray(0);
    }

    /// <summary>
    /// 渲染圆形
    /// </summary>
    /// <param name="position">位置（实际通过 uModel 矩阵传递）</param>
    /// <param name="radius">半径（实际通过 uModel 矩阵传递）</param>
    /// <param name="color">颜色 (RGBA)</param>
    public unsafe void Draw(Vector2 position, float radius, Vector4 color)
    {
        _gl.BindVertexArray(_vao);

        // 只更新颜色到 uniform（如果 shader 支持），或者更新 VBO
        // 这里简化为直接更新整个 VBO，但只改颜色
        int vertexCount = _segments + 2;
        float[] vertices = new float[vertexCount * 6];

        // 中心点
        vertices[0] = 0.0f;
        vertices[1] = 0.0f;
        vertices[2] = color.X;
        vertices[3] = color.Y;
        vertices[4] = color.Z;
        vertices[5] = color.W;

        // 圆周点
        for (int i = 0; i <= _segments; i++)
        {
            float angle = (float)(i * 2.0 * Math.PI / _segments);
            int offset = (i + 1) * 6;

            vertices[offset + 0] = MathF.Cos(angle);
            vertices[offset + 1] = MathF.Sin(angle);
            vertices[offset + 2] = color.X;
            vertices[offset + 3] = color.Y;
            vertices[offset + 4] = color.Z;
            vertices[offset + 5] = color.W;
        }

        // 更新 VBO
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        fixed (float* v = vertices)
        {
            _gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, (nuint)(vertices.Length * sizeof(float)), v);
        }

        _gl.DrawArrays(PrimitiveType.TriangleFan, 0, (uint)(_segments + 2));

        _gl.BindVertexArray(0);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _gl.DeleteBuffer(_vbo);
            _gl.DeleteVertexArray(_vao);
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~CircleRenderer()
    {
        if (!_disposed)
        {
            Console.WriteLine($"Warning: CircleRenderer was not disposed properly");
        }
    }
}
