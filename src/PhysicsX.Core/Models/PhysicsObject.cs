using System;
using System.Numerics;
using PhysicsX.Core.Interfaces;

namespace PhysicsX.Core.Models;

/// <summary>
/// 物理对象基类
/// </summary>
public abstract class PhysicsObject : IPhysicsObject
{
    public string Id { get; }
    public string Name { get; set; }
    public Vector2 Position { get; set; }
    public float Rotation { get; set; }
    public bool IsEnabled { get; set; }

    protected PhysicsObject(string? name = null)
    {
        Id = Guid.NewGuid().ToString();
        Name = name ?? GetType().Name;
        IsEnabled = true;
    }

    public abstract void Update(double deltaTime);
}
