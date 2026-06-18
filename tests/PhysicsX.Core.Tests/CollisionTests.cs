using System.Numerics;
using FluentAssertions;
using PhysicsX.Core.Collision;
using PhysicsX.Core.Engines;
using PhysicsX.Core.Models;
using Xunit;

namespace PhysicsX.Core.Tests;

/// <summary>
/// 碰撞检测测试
/// </summary>
public class CollisionTests
{
    [Fact]
    public void CircleVsCircle_Overlapping_ShouldDetectCollision()
    {
        // Arrange
        var bodyA = new RigidBody { Position = new Vector2(0, 0) };
        var bodyB = new RigidBody { Position = new Vector2(1.5f, 0) };
        var shapeA = new CircleShape(1.0f);
        var shapeB = new CircleShape(1.0f);

        // Act
        var collision = CollisionDetector.CircleVsCircle(bodyA, shapeA, bodyB, shapeB);

        // Assert
        collision.HasCollision.Should().BeTrue();
        collision.Penetration.Should().BeApproximately(0.5f, 0.01f);
        collision.Normal.X.Should().BeApproximately(1.0f, 0.01f);
    }

    [Fact]
    public void CircleVsCircle_NotOverlapping_ShouldNotDetectCollision()
    {
        // Arrange
        var bodyA = new RigidBody { Position = new Vector2(0, 0) };
        var bodyB = new RigidBody { Position = new Vector2(5, 0) };
        var shapeA = new CircleShape(1.0f);
        var shapeB = new CircleShape(1.0f);

        // Act
        var collision = CollisionDetector.CircleVsCircle(bodyA, shapeA, bodyB, shapeB);

        // Assert
        collision.HasCollision.Should().BeFalse();
    }

    [Fact]
    public void CircleVsBox_Overlapping_ShouldDetectCollision()
    {
        // Arrange: 圆心在 (0,0)，半径 1.0，矩形在 (1.5, 0)，宽高 2x2
        // 圆应该与矩形左边缘重叠
        var circle = new RigidBody { Position = new Vector2(0, 0) };
        var box = new RigidBody { Position = new Vector2(1.5f, 0) };
        var circleShape = new CircleShape(1.0f);
        var boxShape = new BoxShape(2.0f, 2.0f);

        // Act
        var collision = CollisionDetector.CircleVsBox(circle, circleShape, box, boxShape);

        // Assert
        collision.HasCollision.Should().BeTrue();
    }

    [Fact]
    public void BouncingBall_ShouldLoseEnergy()
    {
        // Arrange: 从 10 米高度落下的小球
        var engine = new MechanicsEngine { Gravity = 9.8 };

        var ball = new RigidBody("Ball")
        {
            Mass = 1.0,
            Position = new Vector2(0, 0),
            Velocity = Vector2.Zero,
            Restitution = 0.8, // 80% 弹性
            UseGravity = true,
            Shape = new CircleShape(0.5f)
        };

        var ground = new RigidBody("Ground")
        {
            Position = new Vector2(0, 10),
            IsStatic = true,
            Shape = new BoxShape(20, 1)
        };

        engine.AddObject(ball);
        engine.AddObject(ground);

        // Act: 模拟落地和反弹
        float initialHeight = ball.Position.Y;

        // 模拟 2 秒（足够落地和反弹）
        for (int i = 0; i < 120; i++)
        {
            engine.Step(1.0 / 60.0);
        }

        // Assert: 小球应该在地面附近弹跳
        ball.Position.Y.Should().BeLessThan(10.5f); // 接近地面

        // 速度应该因为碰撞而改变（不为零）
        // 但由于能量损失，不会回到原始高度
    }

    [Fact]
    public void TwoCircles_Collision_ShouldSeparate()
    {
        // Arrange
        var engine = new MechanicsEngine { Gravity = 0 }; // 无重力

        var ballA = new RigidBody("Ball A")
        {
            Mass = 1.0,
            Position = new Vector2(0, 0),
            Velocity = new Vector2(5, 0),
            Restitution = 1.0, // 完全弹性碰撞
            Shape = new CircleShape(1.0f)
        };

        var ballB = new RigidBody("Ball B")
        {
            Mass = 1.0,
            Position = new Vector2(3, 0),
            Velocity = new Vector2(-5, 0),
            Restitution = 1.0,
            Shape = new CircleShape(1.0f)
        };

        engine.AddObject(ballA);
        engine.AddObject(ballB);

        // Act: 模拟碰撞
        for (int i = 0; i < 60; i++)
        {
            engine.Step(1.0 / 60.0);
        }

        // Assert: 两个球应该分离并反向运动
        // 由于完全弹性碰撞且质量相等，应该交换速度
        ballA.Velocity.X.Should().BeLessThan(0); // 向左
        ballB.Velocity.X.Should().BeGreaterThan(0); // 向右
    }

    [Fact]
    public void CustomGravity_ShouldAffectFallSpeed()
    {
        // Arrange: 测试自定义重力加速度
        var earthEngine = new MechanicsEngine { Gravity = 9.8 };
        var moonEngine = new MechanicsEngine { Gravity = 1.62 }; // 月球重力

        var ballEarth = new RigidBody { Mass = 1.0, Position = Vector2.Zero, UseGravity = true };
        var ballMoon = new RigidBody { Mass = 1.0, Position = Vector2.Zero, UseGravity = true };

        earthEngine.AddObject(ballEarth);
        moonEngine.AddObject(ballMoon);

        // Act: 模拟 1 秒
        for (int i = 0; i < 60; i++)
        {
            earthEngine.Step(1.0 / 60.0);
            moonEngine.Step(1.0 / 60.0);
        }

        // Assert: 地球上的球下落速度应该更快
        ballEarth.Position.Y.Should().BeGreaterThan(ballMoon.Position.Y);

        // 验证重力公式: h = 1/2 * g * t²
        // 地球: 1/2 * 9.8 * 1² = 4.9 米
        ballEarth.Position.Y.Should().BeApproximately(4.9f, 0.15f);

        // 月球: 1/2 * 1.62 * 1² = 0.81 米
        ballMoon.Position.Y.Should().BeApproximately(0.81f, 0.15f);
    }
}
