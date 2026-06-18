using System.Numerics;

namespace PhysicsX.Core.Interfaces;

/// <summary>
/// 数值积分器接口（用于求解微分方程）
/// </summary>
public interface IIntegrator
{
    /// <summary>
    /// 积分器名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 积分一个时间步
    /// </summary>
    /// <param name="position">当前位置</param>
    /// <param name="velocity">当前速度</param>
    /// <param name="acceleration">当前加速度</param>
    /// <param name="deltaTime">时间步长</param>
    /// <returns>新的位置和速度</returns>
    (Vector2 newPosition, Vector2 newVelocity) Integrate(
        Vector2 position,
        Vector2 velocity,
        Vector2 acceleration,
        double deltaTime);
}
