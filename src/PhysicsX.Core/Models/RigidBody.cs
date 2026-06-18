using System.Numerics;
using PhysicsX.Core.Collision;

namespace PhysicsX.Core.Models;

/// <summary>
/// 刚体物理对象
/// </summary>
public class RigidBody : PhysicsObject
{
    /// <summary>
    /// 质量 (kg)
    /// </summary>
    public double Mass { get; set; }

    /// <summary>
    /// 速度 (m/s)
    /// </summary>
    public Vector2 Velocity { get; set; }

    /// <summary>
    /// 加速度 (m/s²)
    /// </summary>
    public Vector2 Acceleration { get; set; }

    /// <summary>
    /// 角速度 (rad/s)
    /// </summary>
    public float AngularVelocity { get; set; }

    /// <summary>
    /// 转动惯量 (kg·m²)
    /// </summary>
    public double MomentOfInertia { get; set; }

    /// <summary>
    /// 累积受力 (N)
    /// </summary>
    public Vector2 Force { get; set; }

    /// <summary>
    /// 累积力矩 (N·m)
    /// </summary>
    public float Torque { get; set; }

    /// <summary>
    /// 恢复系数（弹性碰撞系数，0=完全非弹性，1=完全弹性）
    /// </summary>
    public double Restitution { get; set; }

    /// <summary>
    /// 摩擦系数
    /// </summary>
    public double Friction { get; set; }

    /// <summary>
    /// 是否受重力影响
    /// </summary>
    public bool UseGravity { get; set; }

    /// <summary>
    /// 是否为静态对象（无限质量，不移动）
    /// </summary>
    public bool IsStatic { get; set; }

    /// <summary>
    /// 碰撞形状
    /// </summary>
    public CollisionShape? Shape { get; set; }

    public RigidBody(string? name = null) : base(name)
    {
        Mass = 1.0;
        Restitution = 0.8;
        Friction = 0.3;
        UseGravity = true;
        IsStatic = false;
    }

    /// <summary>
    /// 施加力
    /// </summary>
    public void ApplyForce(Vector2 force)
    {
        Force += force;
    }

    /// <summary>
    /// 施加冲量
    /// </summary>
    public void ApplyImpulse(Vector2 impulse)
    {
        if (!IsStatic && Mass > 0)
        {
            Velocity += impulse / (float)Mass;
        }
    }

    /// <summary>
    /// 清除累积的力和力矩
    /// </summary>
    public void ClearForces()
    {
        Force = Vector2.Zero;
        Torque = 0f;
    }

    public override void Update(double deltaTime)
    {
        if (!IsEnabled || IsStatic)
            return;

        // 基础运动更新（在引擎中由积分器处理）
    }
}
