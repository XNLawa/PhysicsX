using System.Numerics;

namespace PhysicsX.Core.Interfaces;

/// <summary>
/// 物理对象基础接口
/// </summary>
public interface IPhysicsObject
{
    /// <summary>
    /// 唯一标识符
    /// </summary>
    string Id { get; }

    /// <summary>
    /// 对象名称
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// 位置（世界坐标）
    /// </summary>
    Vector2 Position { get; set; }

    /// <summary>
    /// 旋转角度（弧度）
    /// </summary>
    float Rotation { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    bool IsEnabled { get; set; }

    /// <summary>
    /// 更新物理状态
    /// </summary>
    /// <param name="deltaTime">时间步长（秒）</param>
    void Update(double deltaTime);
}
