using System;
using System.Numerics;
using PhysicsX.Core.Models;

namespace PhysicsX.Core.Collision;

/// <summary>
/// 碰撞形状类型
/// </summary>
public enum ShapeType
{
    Circle,
    Box,
    Polygon,
    Spring,
    Rope,
    Ramp,
    Capsule
}

/// <summary>
/// 碰撞形状基类
/// </summary>
public abstract class CollisionShape
{
    public abstract ShapeType Type { get; }
}

/// <summary>
/// 圆形碰撞体
/// </summary>
public class CircleShape : CollisionShape
{
    public override ShapeType Type => ShapeType.Circle;

    /// <summary>
    /// 半径
    /// </summary>
    public float Radius { get; set; }

    public CircleShape(float radius)
    {
        Radius = radius;
    }
}

/// <summary>
/// 矩形碰撞体（AABB - 轴对齐包围盒）
/// </summary>
public class BoxShape : CollisionShape
{
    public override ShapeType Type => ShapeType.Box;

    /// <summary>
    /// 宽度
    /// </summary>
    public float Width { get; set; }

    /// <summary>
    /// 高度
    /// </summary>
    public float Height { get; set; }

    public BoxShape(float width, float height)
    {
        Width = width;
        Height = height;
    }
}

/// <summary>
/// 碰撞检测器
/// </summary>
public static class CollisionDetector
{
    /// <summary>
    /// 检测两个圆形的碰撞
    /// </summary>
    public static CollisionInfo CircleVsCircle(
        RigidBody a, CircleShape shapeA,
        RigidBody b, CircleShape shapeB)
    {
        var result = new CollisionInfo();

        // 计算圆心距离
        var delta = b.Position - a.Position;
        float distanceSquared = delta.LengthSquared();
        float radiusSum = shapeA.Radius + shapeB.Radius;

        // 判断是否碰撞
        if (distanceSquared >= radiusSum * radiusSum)
        {
            result.HasCollision = false;
            return result;
        }

        float distance = MathF.Sqrt(distanceSquared);

        result.HasCollision = true;
        result.Penetration = radiusSum - distance;

        // 处理两个圆心重合的情况
        if (distance < 0.0001f)
        {
            result.Normal = new Vector2(1, 0);
            result.Point = a.Position;
        }
        else
        {
            result.Normal = delta / distance;
            result.Point = a.Position + result.Normal * shapeA.Radius;
        }

        return result;
    }

    /// <summary>
    /// 检测圆形与矩形（AABB）的碰撞
    /// </summary>
    public static CollisionInfo CircleVsBox(
        RigidBody circle, CircleShape circleShape,
        RigidBody box, BoxShape boxShape)
    {
        var result = new CollisionInfo();

        // 计算矩形的边界
        float halfWidth = boxShape.Width / 2f;
        float halfHeight = boxShape.Height / 2f;
        Vector2 boxMin = box.Position - new Vector2(halfWidth, halfHeight);
        Vector2 boxMax = box.Position + new Vector2(halfWidth, halfHeight);

        // 找到矩形上距离圆心最近的点
        Vector2 closest = new Vector2(
            Math.Clamp(circle.Position.X, boxMin.X, boxMax.X),
            Math.Clamp(circle.Position.Y, boxMin.Y, boxMax.Y)
        );

        // 计算圆心到最近点的距离
        Vector2 delta = closest - circle.Position;
        float distanceSquared = delta.LengthSquared();
        float radius = circleShape.Radius;

        // 判断是否碰撞
        if (distanceSquared >= radius * radius)
        {
            result.HasCollision = false;
            return result;
        }

        float distance = MathF.Sqrt(distanceSquared);

        result.HasCollision = true;
        result.Penetration = radius - distance;

        // 处理圆心在矩形内部的情况
        if (distance < 0.0001f)
        {
            // 找到最近的边
            float distLeft = circle.Position.X - boxMin.X;
            float distRight = boxMax.X - circle.Position.X;
            float distTop = circle.Position.Y - boxMin.Y;
            float distBottom = boxMax.Y - circle.Position.Y;

            float minDist = Math.Min(Math.Min(distLeft, distRight), Math.Min(distTop, distBottom));

            if (minDist == distLeft)
                result.Normal = new Vector2(-1, 0);
            else if (minDist == distRight)
                result.Normal = new Vector2(1, 0);
            else if (minDist == distTop)
                result.Normal = new Vector2(0, -1);
            else
                result.Normal = new Vector2(0, 1);

            result.Point = closest;
        }
        else
        {
            result.Normal = -delta / distance; // 从圆指向矩形
            result.Point = closest;
        }

        return result;
    }

    /// <summary>
    /// 检测两个矩形（AABB）的碰撞
    /// </summary>
    public static CollisionInfo BoxVsBox(
        RigidBody a, BoxShape shapeA,
        RigidBody b, BoxShape shapeB)
    {
        var result = new CollisionInfo();

        // 计算两个矩形的边界
        float halfWidthA = shapeA.Width / 2f;
        float halfHeightA = shapeA.Height / 2f;
        float halfWidthB = shapeB.Width / 2f;
        float halfHeightB = shapeB.Height / 2f;

        Vector2 aMin = a.Position - new Vector2(halfWidthA, halfHeightA);
        Vector2 aMax = a.Position + new Vector2(halfWidthA, halfHeightA);
        Vector2 bMin = b.Position - new Vector2(halfWidthB, halfHeightB);
        Vector2 bMax = b.Position + new Vector2(halfWidthB, halfHeightB);

        // AABB 碰撞检测
        if (aMax.X < bMin.X || aMin.X > bMax.X ||
            aMax.Y < bMin.Y || aMin.Y > bMax.Y)
        {
            result.HasCollision = false;
            return result;
        }

        // 计算重叠区域
        float overlapX = Math.Min(aMax.X - bMin.X, bMax.X - aMin.X);
        float overlapY = Math.Min(aMax.Y - bMin.Y, bMax.Y - aMin.Y);

        result.HasCollision = true;

        // 选择穿透最小的轴作为碰撞法线
        if (overlapX < overlapY)
        {
            result.Penetration = overlapX;
            result.Normal = (a.Position.X < b.Position.X) ? new Vector2(-1, 0) : new Vector2(1, 0);
        }
        else
        {
            result.Penetration = overlapY;
            result.Normal = (a.Position.Y < b.Position.Y) ? new Vector2(0, -1) : new Vector2(0, 1);
        }

        result.Point = (a.Position + b.Position) / 2f;

        return result;
    }
}
