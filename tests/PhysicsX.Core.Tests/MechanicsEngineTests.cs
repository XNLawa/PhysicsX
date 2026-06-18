using System.Numerics;
using FluentAssertions;
using PhysicsX.Core.Engines;
using PhysicsX.Core.Models;
using Xunit;

namespace PhysicsX.Core.Tests;

/// <summary>
/// 力学引擎测试
/// </summary>
public class MechanicsEngineTests
{
    [Fact]
    public void FreeFall_After1Second_ShouldFall4_9Meters()
    {
        // Arrange: 创建一个1kg的小球，初始位置在原点
        var engine = new MechanicsEngine { Gravity = 9.8 };
        var ball = new RigidBody("Ball")
        {
            Mass = 1.0,
            Position = Vector2.Zero,
            UseGravity = true
        };
        engine.AddObject(ball);

        // Act: 模拟1秒（60帧）
        const double timeStep = 1.0 / 60.0;
        for (int i = 0; i < 60; i++)
        {
            engine.Step(timeStep);
        }

        // Assert: 验证自由落体公式 h = 1/2 * g * t²
        // h = 0.5 * 9.8 * 1² = 4.9 米（向下为正）
        ball.Position.Y.Should().BeApproximately(4.9f, 0.15f);
    }

    [Fact]
    public void StaticObject_ShouldNotMove()
    {
        // Arrange
        var engine = new MechanicsEngine();
        var staticBox = new RigidBody("StaticBox")
        {
            Mass = 100.0,
            IsStatic = true,
            Position = new Vector2(0, 0)
        };
        engine.AddObject(staticBox);

        // Act: 模拟1秒
        for (int i = 0; i < 60; i++)
        {
            engine.Step(1.0 / 60.0);
        }

        // Assert: 静态对象不应该移动
        staticBox.Position.Should().Be(Vector2.Zero);
        staticBox.Velocity.Should().Be(Vector2.Zero);
    }

    [Fact]
    public void ApplyImpulse_ShouldChangeVelocity()
    {
        // Arrange
        var engine = new MechanicsEngine();
        var ball = new RigidBody("Ball")
        {
            Mass = 2.0,
            UseGravity = false
        };
        engine.AddObject(ball);

        // Act: 施加水平冲量 10 N·s
        ball.ApplyImpulse(new Vector2(10f, 0));

        // Assert: 速度变化 Δv = J/m = 10/2 = 5 m/s
        ball.Velocity.X.Should().BeApproximately(5f, 0.01f);
    }

    [Fact]
    public void Pause_ShouldStopSimulation()
    {
        // Arrange
        var engine = new MechanicsEngine();
        var ball = new RigidBody("Ball")
        {
            Position = Vector2.Zero,
            Velocity = new Vector2(10, 0)
        };
        engine.AddObject(ball);

        // Act: 暂停并尝试步进
        engine.IsPaused = true;
        engine.Step(1.0 / 60.0);

        // Assert: 位置不应该改变
        ball.Position.Should().Be(Vector2.Zero);
    }

    [Fact]
    public void Reset_ShouldClearVelocity()
    {
        // Arrange
        var engine = new MechanicsEngine();
        var ball = new RigidBody("Ball")
        {
            Velocity = new Vector2(10, 5),
            AngularVelocity = 2.0f
        };
        engine.AddObject(ball);

        // Act
        engine.Reset();

        // Assert
        ball.Velocity.Should().Be(Vector2.Zero);
        ball.AngularVelocity.Should().Be(0);
        engine.SimulationTime.Should().Be(0);
    }
}
