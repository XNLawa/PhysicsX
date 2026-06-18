using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using PhysicsX.Core.Interfaces;
using PhysicsX.Core.Integrators;
using PhysicsX.Core.Models;
using PhysicsX.Core.Collision;
using PhysicsX.Core.Physics;

namespace PhysicsX.Core.Engines;

/// <summary>
/// 力学仿真引擎
/// 支持刚体动力学、碰撞检测、约束求解
/// </summary>
public class MechanicsEngine : ISimulationEngine
{
    private readonly List<IPhysicsObject> _objects;
    private readonly IIntegrator _integrator;

    public string Name => "Mechanics Engine";
    public double Gravity { get; set; }
    public bool IsPaused { get; set; }
    public double SimulationTime { get; private set; }
    public IReadOnlyList<IPhysicsObject> Objects => _objects.AsReadOnly();

    public MechanicsEngine(IIntegrator? integrator = null)
    {
        _objects = new List<IPhysicsObject>();
        _integrator = integrator ?? new RK4Integrator();
        Gravity = 9.8; // 地球重力加速度
        IsPaused = false;
        SimulationTime = 0;
    }

    public void AddObject(IPhysicsObject obj)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        if (!_objects.Contains(obj))
        {
            _objects.Add(obj);
        }
    }

    public bool RemoveObject(string objectId)
    {
        var obj = _objects.FirstOrDefault(o => o.Id == objectId);
        if (obj != null)
        {
            _objects.Remove(obj);
            return true;
        }
        return false;
    }

    public void Clear()
    {
        _objects.Clear();
        SimulationTime = 0;
    }

    public void Step(double deltaTime)
    {
        if (IsPaused || deltaTime <= 0)
            return;

        // 1. 累积力（重力、用户自定义力等）
        AccumulateForces();

        // 2. 积分求解运动方程
        IntegrateMotion(deltaTime);

        // 3. 碰撞检测和响应
        DetectAndResolveCollisions();

        // 4. 约束求解（待实现）
        // SolveConstraints();

        // 5. 清除累积的力
        ClearForces();

        // 6. 更新仿真时间
        SimulationTime += deltaTime;
    }

    public void Reset()
    {
        SimulationTime = 0;
        foreach (var obj in _objects.OfType<RigidBody>())
        {
            obj.Velocity = Vector2.Zero;
            obj.Acceleration = Vector2.Zero;
            obj.AngularVelocity = 0;
            obj.ClearForces();
        }
    }

    /// <summary>
    /// 累积所有力
    /// </summary>
    private void AccumulateForces()
    {
        foreach (var obj in _objects.OfType<RigidBody>())
        {
            if (!obj.IsEnabled || obj.IsStatic)
                continue;

            // 施加重力
            if (obj.UseGravity && obj.Mass > 0)
            {
                var gravityForce = new Vector2(0, (float)(obj.Mass * Gravity));
                obj.ApplyForce(gravityForce);
            }
        }
    }

    /// <summary>
    /// 使用积分器更新运动状态
    /// </summary>
    private void IntegrateMotion(double deltaTime)
    {
        foreach (var obj in _objects.OfType<RigidBody>())
        {
            if (!obj.IsEnabled || obj.IsStatic)
                continue;

            // 计算加速度：F = ma => a = F/m
            if (obj.Mass > 0)
            {
                obj.Acceleration = obj.Force / (float)obj.Mass;
            }
            else
            {
                obj.Acceleration = Vector2.Zero;
            }

            // 使用积分器更新位置和速度
            var (newPosition, newVelocity) = _integrator.Integrate(
                obj.Position,
                obj.Velocity,
                obj.Acceleration,
                deltaTime
            );

            obj.Position = newPosition;
            obj.Velocity = newVelocity;

            // 更新旋转
            if (obj.MomentOfInertia > 0)
            {
                var angularAcceleration = obj.Torque / (float)obj.MomentOfInertia;
                obj.AngularVelocity += angularAcceleration * (float)deltaTime;
                obj.Rotation += obj.AngularVelocity * (float)deltaTime;
            }

            // 调用对象自己的更新逻辑
            obj.Update(deltaTime);
        }
    }

    /// <summary>
    /// 清除所有对象累积的力
    /// </summary>
    private void ClearForces()
    {
        foreach (var obj in _objects.OfType<RigidBody>())
        {
            obj.ClearForces();
        }
    }

    /// <summary>
    /// 检测并响应碰撞
    /// </summary>
    private void DetectAndResolveCollisions()
    {
        var rigidBodies = _objects.OfType<RigidBody>().Where(rb => rb.IsEnabled).ToList();

        // 遍历所有物体对
        for (int i = 0; i < rigidBodies.Count; i++)
        {
            for (int j = i + 1; j < rigidBodies.Count; j++)
            {
                var bodyA = rigidBodies[i];
                var bodyB = rigidBodies[j];

                // 至少有一个非静态对象才需要检测碰撞
                if (bodyA.IsStatic && bodyB.IsStatic)
                    continue;

                // 必须有碰撞形状
                if (bodyA.Shape == null || bodyB.Shape == null)
                    continue;

                // 根据形状类型选择碰撞检测方法
                CollisionInfo collision = DetectCollision(bodyA, bodyB);

                if (collision.HasCollision)
                {
                    ResolveCollision(bodyA, bodyB, collision);
                }
            }
        }
    }

    /// <summary>
    /// 检测两个物体的碰撞
    /// </summary>
    private CollisionInfo DetectCollision(RigidBody a, RigidBody b)
    {
        if (a.Shape is CircleShape circleA && b.Shape is CircleShape circleB)
        {
            return CollisionDetector.CircleVsCircle(a, circleA, b, circleB);
        }
        else if (a.Shape is CircleShape circleShape && b.Shape is BoxShape boxShape)
        {
            return CollisionDetector.CircleVsBox(a, circleShape, b, boxShape);
        }
        else if (a.Shape is BoxShape boxShapeA && b.Shape is CircleShape circleShapeB)
        {
            var collision = CollisionDetector.CircleVsBox(b, circleShapeB, a, boxShapeA);
            // 反转法线方向
            collision.Normal = -collision.Normal;
            return collision;
        }
        else if (a.Shape is BoxShape boxA && b.Shape is BoxShape boxB)
        {
            return CollisionDetector.BoxVsBox(a, boxA, b, boxB);
        }

        return new CollisionInfo { HasCollision = false };
    }

    /// <summary>
    /// 解决碰撞（冲量法）
    /// </summary>
    private void ResolveCollision(RigidBody a, RigidBody b, CollisionInfo collision)
    {
        // 分离物体（位置修正）
        PositionalCorrection(a, b, collision);

        // 计算相对速度
        Vector2 relativeVelocity = b.Velocity - a.Velocity;
        float velocityAlongNormal = Vector2.Dot(relativeVelocity, collision.Normal);

        // 物体正在分离，不需要施加冲量
        if (velocityAlongNormal > 0)
            return;

        // 计算恢复系数（取两者的最小值）
        float restitution = (float)Math.Min(a.Restitution, b.Restitution);

        // 计算冲量大小
        float invMassA = a.IsStatic ? 0 : 1.0f / (float)a.Mass;
        float invMassB = b.IsStatic ? 0 : 1.0f / (float)b.Mass;
        float invMassSum = invMassA + invMassB;

        if (invMassSum < 0.0001f) // 两个都是静态对象
            return;

        float impulseMagnitude = -(1 + restitution) * velocityAlongNormal / invMassSum;
        Vector2 impulse = impulseMagnitude * collision.Normal;

        // 施加冲量
        if (!a.IsStatic)
            a.Velocity -= impulse * invMassA;
        if (!b.IsStatic)
            b.Velocity += impulse * invMassB;

        // 摩擦力冲量
        ApplyFriction(a, b, collision, impulseMagnitude);
    }

    /// <summary>
    /// 位置修正（防止物体穿透）
    /// </summary>
    private void PositionalCorrection(RigidBody a, RigidBody b, CollisionInfo collision)
    {
        const float percent = 0.4f; // 穿透修正百分比
        const float slop = 0.01f;   // 允许的小穿透量

        float invMassA = a.IsStatic ? 0 : 1.0f / (float)a.Mass;
        float invMassB = b.IsStatic ? 0 : 1.0f / (float)b.Mass;
        float invMassSum = invMassA + invMassB;

        if (invMassSum < 0.0001f)
            return;

        Vector2 correction = (Math.Max(collision.Penetration - slop, 0.0f) / invMassSum) * percent * collision.Normal;

        if (!a.IsStatic)
            a.Position -= correction * invMassA;
        if (!b.IsStatic)
            b.Position += correction * invMassB;
    }

    /// <summary>
    /// 施加摩擦力冲量（并计算摩擦生热）
    /// </summary>
    private void ApplyFriction(RigidBody a, RigidBody b, CollisionInfo collision, float normalImpulse)
    {
        Vector2 relativeVelocity = b.Velocity - a.Velocity;

        // 计算切线方向（垂直于法线）
        Vector2 tangent = relativeVelocity - Vector2.Dot(relativeVelocity, collision.Normal) * collision.Normal;
        float tangentLength = tangent.Length();

        if (tangentLength < 0.0001f)
            return;

        tangent /= tangentLength; // 归一化切线

        // 计算切线冲量
        float invMassA = a.IsStatic ? 0 : 1.0f / (float)a.Mass;
        float invMassB = b.IsStatic ? 0 : 1.0f / (float)b.Mass;
        float invMassSum = invMassA + invMassB;

        float jt = -Vector2.Dot(relativeVelocity, tangent) / invMassSum;

        // 库仑摩擦定律
        float mu = (float)Math.Sqrt(a.Friction * b.Friction);

        Vector2 frictionImpulse;
        bool isKineticFriction = false;

        if (Math.Abs(jt) < normalImpulse * mu)
        {
            // 静摩擦
            frictionImpulse = jt * tangent;
        }
        else
        {
            // 动摩擦
            frictionImpulse = -normalImpulse * mu * tangent;
            isKineticFriction = true;
        }

        // 施加摩擦冲量
        if (!a.IsStatic)
            a.Velocity -= frictionImpulse * invMassA;
        if (!b.IsStatic)
            b.Velocity += frictionImpulse * invMassB;

        // 摩擦生热计算（仅动摩擦产生热量）
        if (isKineticFriction && (a.Thermal.EnableThermal || b.Thermal.EnableThermal))
        {
            // 计算摩擦力大小：F = μ * N
            float normalForce = normalImpulse / 0.016f; // 假设时间步长 0.016s (60 FPS)
            float frictionForce = mu * normalForce;

            // 相对滑动速度
            float relativeSpeed = tangentLength;

            // 假设接触时间（时间步长）
            double contactTime = 0.016;

            // 摩擦距离：d = v * t
            double frictionDistance = relativeSpeed * contactTime;

            // 摩擦生热：Q = F * d
            double heat = ThermalPhysics.CalculateFrictionHeat(mu, normalForce, frictionDistance);

            // 热量分配（按质量比例）
            double totalMass = a.Mass + b.Mass;
            double heatA = heat * (b.Mass / totalMass); // 轻物体获得更多热量（相对）
            double heatB = heat * (a.Mass / totalMass);

            // 累积热量
            if (a.Thermal.EnableThermal && !a.IsStatic)
            {
                a.Thermal.AccumulatedHeat += heatA;
                a.Thermal.Temperature = ThermalPhysics.CalculateTemperature(
                    20.0, // 初始环境温度
                    a.Thermal.AccumulatedHeat,
                    a.Mass,
                    a.Thermal.SpecificHeat
                );
            }

            if (b.Thermal.EnableThermal && !b.IsStatic)
            {
                b.Thermal.AccumulatedHeat += heatB;
                b.Thermal.Temperature = ThermalPhysics.CalculateTemperature(
                    20.0,
                    b.Thermal.AccumulatedHeat,
                    b.Mass,
                    b.Thermal.SpecificHeat
                );
            }
        }
    }
}
