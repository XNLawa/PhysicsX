using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;
using PhysicsX.Core.Collision;

namespace PhysicsX.App.Models;

/// <summary>
/// 场景项目数据
/// </summary>
public class SceneProject
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "未命名项目";

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";

    [JsonPropertyName("gravity")]
    public double Gravity { get; set; } = 9.8;

    [JsonPropertyName("objects")]
    public List<SceneObject> Objects { get; set; } = new();

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [JsonPropertyName("modifiedAt")]
    public DateTime ModifiedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// 场景对象数据
/// </summary>
public class SceneObject
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("name")]
    public string Name { get; set; } = "Object";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "Circle"; // Circle, Box, Ground

    [JsonPropertyName("position")]
    public Vector2Data Position { get; set; } = new();

    [JsonPropertyName("velocity")]
    public Vector2Data Velocity { get; set; } = new();

    [JsonPropertyName("mass")]
    public double Mass { get; set; } = 1.0;

    [JsonPropertyName("restitution")]
    public double Restitution { get; set; } = 0.8;

    [JsonPropertyName("friction")]
    public double Friction { get; set; } = 0.3;

    [JsonPropertyName("isStatic")]
    public bool IsStatic { get; set; } = false;

    [JsonPropertyName("useGravity")]
    public bool UseGravity { get; set; } = true;

    [JsonPropertyName("radius")]
    public float? Radius { get; set; }

    [JsonPropertyName("width")]
    public float? Width { get; set; }

    [JsonPropertyName("height")]
    public float? Height { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; } = "#3B82F6";
}

/// <summary>
/// Vector2 序列化辅助类
/// </summary>
public class Vector2Data
{
    [JsonPropertyName("x")]
    public float X { get; set; }

    [JsonPropertyName("y")]
    public float Y { get; set; }

    public Vector2Data() { }

    public Vector2Data(float x, float y)
    {
        X = x;
        Y = y;
    }

    public Vector2Data(Vector2 v)
    {
        X = v.X;
        Y = v.Y;
    }

    public Vector2 ToVector2() => new Vector2(X, Y);
}
