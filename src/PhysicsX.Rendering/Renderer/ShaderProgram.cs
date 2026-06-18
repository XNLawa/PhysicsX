using System;
using System.Numerics;
using Silk.NET.OpenGL;

namespace PhysicsX.Rendering.Renderer;

/// <summary>
/// Shader 程序管理器
/// </summary>
public class ShaderProgram : IDisposable
{
    private readonly GL _gl;
    private readonly uint _handle;
    private bool _disposed;

    public uint Handle => _handle;

    public ShaderProgram(GL gl, string vertexSource, string fragmentSource)
    {
        _gl = gl;

        // 编译顶点着色器
        uint vertexShader = CompileShader(ShaderType.VertexShader, vertexSource);

        // 编译片段着色器
        uint fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentSource);

        // 链接着色器程序
        _handle = _gl.CreateProgram();
        _gl.AttachShader(_handle, vertexShader);
        _gl.AttachShader(_handle, fragmentShader);
        _gl.LinkProgram(_handle);

        // 检查链接错误
        _gl.GetProgram(_handle, ProgramPropertyARB.LinkStatus, out int linkStatus);
        if (linkStatus == 0)
        {
            string infoLog = _gl.GetProgramInfoLog(_handle);
            throw new Exception($"Shader program linking failed: {infoLog}");
        }

        // 清理着色器对象
        _gl.DetachShader(_handle, vertexShader);
        _gl.DetachShader(_handle, fragmentShader);
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);
    }

    private uint CompileShader(ShaderType type, string source)
    {
        uint shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, source);
        _gl.CompileShader(shader);

        // 检查编译错误
        _gl.GetShader(shader, ShaderParameterName.CompileStatus, out int compileStatus);
        if (compileStatus == 0)
        {
            string infoLog = _gl.GetShaderInfoLog(shader);
            _gl.DeleteShader(shader);
            throw new Exception($"Shader compilation failed ({type}): {infoLog}");
        }

        return shader;
    }

    public void Use()
    {
        _gl.UseProgram(_handle);
    }

    public void SetUniform(string name, int value)
    {
        int location = _gl.GetUniformLocation(_handle, name);
        if (location >= 0)
            _gl.Uniform1(location, value);
    }

    public void SetUniform(string name, float value)
    {
        int location = _gl.GetUniformLocation(_handle, name);
        if (location >= 0)
            _gl.Uniform1(location, value);
    }

    public void SetUniform(string name, Vector2 value)
    {
        int location = _gl.GetUniformLocation(_handle, name);
        if (location >= 0)
            _gl.Uniform2(location, value.X, value.Y);
    }

    public void SetUniform(string name, Vector3 value)
    {
        int location = _gl.GetUniformLocation(_handle, name);
        if (location >= 0)
            _gl.Uniform3(location, value.X, value.Y, value.Z);
    }

    public void SetUniform(string name, Vector4 value)
    {
        int location = _gl.GetUniformLocation(_handle, name);
        if (location >= 0)
            _gl.Uniform4(location, value.X, value.Y, value.Z, value.W);
    }

    public unsafe void SetUniform(string name, Matrix4x4 value)
    {
        int location = _gl.GetUniformLocation(_handle, name);
        if (location >= 0)
        {
            _gl.UniformMatrix4(location, 1, false, (float*)&value);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _gl.DeleteProgram(_handle);
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~ShaderProgram()
    {
        if (!_disposed)
        {
            // 警告：在终结器中无法安全调用 OpenGL
            Console.WriteLine($"Warning: ShaderProgram {_handle} was not disposed properly");
        }
    }
}
