using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using PhysicsX.Core.Interfaces;
using PhysicsX.Core.Integrators;
using PhysicsX.Core.Models;

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

        // 3. 碰撞检测（待实现）
        // DetectCollisions();

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
}
