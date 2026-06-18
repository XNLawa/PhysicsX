using System;
using System.Numerics;
using Silk.NET.OpenGL;

namespace PhysicsX.Rendering.Primitives;

/// <summary>
/// 线条渲染器（用于绘制力向量、速度向量等）
/// </summary>
public class LineRenderer : IDisposable
{
    private readonly GL _gl;
    private uint _vao;
    private uint _vbo;
    private bool _disposed;

    public LineRenderer(GL gl)
    {
        _gl = gl;
        InitializeBuffers();
    }

    private unsafe void InitializeBuffers()
    {
        _vao = _gl.GenVertexArray();
        _vbo = _gl.GenBuffer();

        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        // 预分配缓冲区（动态更新）
        _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(2 * 6 * sizeof(float)), null, BufferUsageARB.DynamicDraw);

        // 位置属性
        _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 6 * sizeof(float), (void*)0);
        _gl.EnableVertexAttribArray(0);

        // 颜色属性
        _gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 6 * sizeof(float), (void*)(2 * sizeof(float)));
        _gl.EnableVertexAttribArray(1);

        _gl.BindVertexArray(0);
    }

    /// <summary>
    /// 绘制单条线段
    /// </summary>
    public unsafe void DrawLine(Vector2 start, Vector2 end, Vector4 color, float lineWidth = 2.0f)
    {
        float[] vertices = new float[]
        {
            start.X, start.Y, color.X, color.Y, color.Z, color.W,
            end.X,   end.Y,   color.X, color.Y, color.Z, color.W
        };

        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        fixed (float* v = vertices)
        {
            _gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, (nuint)(vertices.Length * sizeof(float)), v);
        }

        _gl.LineWidth(lineWidth);
        _gl.DrawArrays(PrimitiveType.Lines, 0, 2);

        _gl.BindVertexArray(0);
    }

    /// <summary>
    /// 绘制箭头（向量表示）
    /// </summary>
    public void DrawArrow(Vector2 start, Vector2 end, Vector4 color, float lineWidth = 2.0f, float arrowHeadSize = 0.2f)
    {
        // 绘制主线
        DrawLine(start, end, color, lineWidth);

        // 计算箭头方向
        Vector2 direction = Vector2.Normalize(end - start);
        Vector2 perpendicular = new Vector2(-direction.Y, direction.X);

        // 箭头头部的两个点
        Vector2 arrowPoint1 = end - direction * arrowHeadSize + perpendicular * arrowHeadSize * 0.5f;
        Vector2 arrowPoint2 = end - direction * arrowHeadSize - perpendicular * arrowHeadSize * 0.5f;

        // 绘制箭头头部
        DrawLine(end, arrowPoint1, color, lineWidth);
        DrawLine(end, arrowPoint2, color, lineWidth);
    }

    /// <summary>
    /// 绘制矩形边框
    /// </summary>
    public void DrawRectangle(Vector2 center, float width, float height, Vector4 color, float lineWidth = 1.0f)
    {
        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        Vector2 topLeft = center + new Vector2(-halfWidth, -halfHeight);
        Vector2 topRight = center + new Vector2(halfWidth, -halfHeight);
        Vector2 bottomRight = center + new Vector2(halfWidth, halfHeight);
        Vector2 bottomLeft = center + new Vector2(-halfWidth, halfHeight);

        DrawLine(topLeft, topRight, color, lineWidth);
        DrawLine(topRight, bottomRight, color, lineWidth);
        DrawLine(bottomRight, bottomLeft, color, lineWidth);
        DrawLine(bottomLeft, topLeft, color, lineWidth);
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

    ~LineRenderer()
    {
        if (!_disposed)
        {
            Console.WriteLine($"Warning: LineRenderer was not disposed properly");
        }
    }
}
