using System.Numerics;

namespace PhysicsX.Core.Collision;

/// <summary>
/// 弹簧连接
/// </summary>
public class SpringShape : CollisionShape
{
    public override ShapeType Type => ShapeType.Spring;

    public float RestLength { get; set; }
    public float Stiffness { get; set; }  // 刚度系数 (N/m)
    public float Damping { get; set; }    // 阻尼系数

    public string BodyAId { get; set; }   // 连接的物体A
    public string BodyBId { get; set; }   // 连接的物体B

    public SpringShape(float restLength, float stiffness, float damping = 0.1f)
    {
        RestLength = restLength;
        Stiffness = stiffness;
        Damping = damping;
        BodyAId = "";
        BodyBId = "";
    }
}

/// <summary>
/// 绳索约束
/// </summary>
public class RopeShape : CollisionShape
{
    public override ShapeType Type => ShapeType.Rope;

    public float MaxLength { get; set; }
    public float Thickness { get; set; }

    public string BodyAId { get; set; }
    public string BodyBId { get; set; }

    public RopeShape(float maxLength, float thickness = 0.1f)
    {
        MaxLength = maxLength;
        Thickness = thickness;
        BodyAId = "";
        BodyBId = "";
    }
}

/// <summary>
/// 斜面形状（静态）
/// </summary>
public class RampShape : CollisionShape
{
    public override ShapeType Type => ShapeType.Ramp;

    public float Width { get; set; }
    public float Height { get; set; }
    public float Angle { get; set; }  // 角度（度）

    public RampShape(float width, float height, float angle)
    {
        Width = width;
        Height = height;
        Angle = angle;
    }
}

/// <summary>
/// 多边形形状
/// </summary>
public class PolygonShape : CollisionShape
{
    public override ShapeType Type => ShapeType.Polygon;

    public Vector2[] Vertices { get; set; }

    public PolygonShape(Vector2[] vertices)
    {
        if (vertices.Length < 3)
            throw new System.ArgumentException("Polygon must have at least 3 vertices");
        Vertices = vertices;
    }
}

/// <summary>
/// 胶囊形状（圆角矩形）
/// </summary>
public class CapsuleShape : CollisionShape
{
    public override ShapeType Type => ShapeType.Capsule;

    public float Length { get; set; }
    public float Radius { get; set; }

    public CapsuleShape(float length, float radius)
    {
        Length = length;
        Radius = radius;
    }
}
