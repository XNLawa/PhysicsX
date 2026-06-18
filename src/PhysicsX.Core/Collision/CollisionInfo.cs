using System.Numerics;

namespace PhysicsX.Core.Collision;

/// <summary>
/// 碰撞信息
/// </summary>
public struct CollisionInfo
{
    /// <summary>
    /// 碰撞点（世界坐标）
    /// </summary>
    public Vector2 Point { get; set; }

    /// <summary>
    /// 碰撞法线（从 A 指向 B）
    /// </summary>
    public Vector2 Normal { get; set; }

    /// <summary>
    /// 穿透深度
    /// </summary>
    public float Penetration { get; set; }

    /// <summary>
    /// 是否发生碰撞
    /// </summary>
    public bool HasCollision { get; set; }
}
