using System.Numerics;
using PhysicsX.Core.Interfaces;

namespace PhysicsX.Core.Integrators;

/// <summary>
/// 四阶龙格-库塔积分器（RK4）
/// 高精度数值积分，适合物理仿真
/// </summary>
public class RK4Integrator : IIntegrator
{
    public string Name => "Runge-Kutta 4th Order";

    public (Vector2 newPosition, Vector2 newVelocity) Integrate(
        Vector2 position,
        Vector2 velocity,
        Vector2 acceleration,
        double deltaTime)
    {
        float dt = (float)deltaTime;

        // RK4 方法
        // k1 = f(t, y)
        var k1Velocity = velocity;
        var k1Acceleration = acceleration;

        // k2 = f(t + dt/2, y + k1*dt/2)
        var k2Position = position + k1Velocity * (dt / 2f);
        var k2Velocity = velocity + k1Acceleration * (dt / 2f);

        // k3 = f(t + dt/2, y + k2*dt/2)
        var k3Position = position + k2Velocity * (dt / 2f);
        var k3Velocity = velocity + k1Acceleration * (dt / 2f);

        // k4 = f(t + dt, y + k3*dt)
        var k4Velocity = velocity + k1Acceleration * dt;

        // y(t + dt) = y(t) + dt/6 * (k1 + 2*k2 + 2*k3 + k4)
        var newPosition = position + (dt / 6f) * (k1Velocity + 2f * k2Velocity + 2f * k3Velocity + k4Velocity);
        var newVelocity = velocity + dt * acceleration;

        return (newPosition, newVelocity);
    }
}
