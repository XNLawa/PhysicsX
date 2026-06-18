using System;
using System.Numerics;

namespace PhysicsX.Core.Physics;

/// <summary>
/// 热力学计算（摩擦生热）
/// </summary>
public class ThermalPhysics
{
    /// <summary>
    /// 摩擦产生的热量 (焦耳)
    /// Q = μ * N * d
    /// </summary>
    /// <param name="frictionCoefficient">摩擦系数</param>
    /// <param name="normalForce">法向力 (N)</param>
    /// <param name="distance">摩擦距离 (m)</param>
    /// <returns>热量 (J)</returns>
    public static double CalculateFrictionHeat(double frictionCoefficient, double normalForce, double distance)
    {
        return frictionCoefficient * normalForce * distance;
    }

    /// <summary>
    /// 根据热量计算温度变化
    /// ΔT = Q / (m * c)
    /// </summary>
    /// <param name="heat">热量 (J)</param>
    /// <param name="mass">质量 (kg)</param>
    /// <param name="specificHeat">比热容 (J/(kg·K))，默认为铝 897</param>
    /// <returns>温度变化 (K 或 °C)</returns>
    public static double CalculateTemperatureChange(double heat, double mass, double specificHeat = 897.0)
    {
        if (mass <= 0) return 0;
        return heat / (mass * specificHeat);
    }

    /// <summary>
    /// 计算物体温度（基于累积热量）
    /// </summary>
    public static double CalculateTemperature(double initialTemp, double accumulatedHeat, double mass, double specificHeat = 897.0)
    {
        double deltaT = CalculateTemperatureChange(accumulatedHeat, mass, specificHeat);
        return initialTemp + deltaT;
    }

    /// <summary>
    /// 热传导损失（简化模型）
    /// 物体向环境散热
    /// </summary>
    /// <param name="temperature">当前温度 (K)</param>
    /// <param name="ambientTemp">环境温度 (K)</param>
    /// <param name="surfaceArea">表面积 (m²)</param>
    /// <param name="deltaTime">时间步长 (s)</param>
    /// <param name="heatTransferCoeff">传热系数，默认 10</param>
    /// <returns>散失的热量 (J)</returns>
    public static double CalculateHeatLoss(double temperature, double ambientTemp, double surfaceArea,
                                          double deltaTime, double heatTransferCoeff = 10.0)
    {
        // 牛顿冷却定律简化版：Q = h * A * ΔT * Δt
        double tempDiff = temperature - ambientTemp;
        if (tempDiff <= 0) return 0;

        return heatTransferCoeff * surfaceArea * tempDiff * deltaTime;
    }

    /// <summary>
    /// 计算圆形物体的表面积
    /// </summary>
    public static double CalculateSphereSurfaceArea(double radius)
    {
        return 4.0 * Math.PI * radius * radius;
    }

    /// <summary>
    /// 计算矩形物体的表面积（简化为 2D）
    /// </summary>
    public static double CalculateBoxSurfaceArea(double width, double height)
    {
        // 2D 简化：周长
        return 2.0 * (width + height);
    }
}

/// <summary>
/// 热力学属性扩展（为 RigidBody 添加）
/// </summary>
public class ThermalProperties
{
    /// <summary>
    /// 温度 (摄氏度)
    /// </summary>
    public double Temperature { get; set; } = 20.0;

    /// <summary>
    /// 累积热量 (焦耳)
    /// </summary>
    public double AccumulatedHeat { get; set; } = 0.0;

    /// <summary>
    /// 比热容 (J/(kg·K))
    /// </summary>
    public double SpecificHeat { get; set; } = 897.0; // 默认铝

    /// <summary>
    /// 是否启用热力学计算
    /// </summary>
    public bool EnableThermal { get; set; } = false;

    /// <summary>
    /// 材料预设
    /// </summary>
    public enum Material
    {
        Aluminum,   // 铝 897 J/(kg·K)
        Iron,       // 铁 450 J/(kg·K)
        Copper,     // 铜 385 J/(kg·K)
        Wood,       // 木材 1700 J/(kg·K)
        Rubber,     // 橡胶 2000 J/(kg·K)
        Ice         // 冰 2100 J/(kg·K)
    }

    public void SetMaterial(Material material)
    {
        SpecificHeat = material switch
        {
            Material.Aluminum => 897.0,
            Material.Iron => 450.0,
            Material.Copper => 385.0,
            Material.Wood => 1700.0,
            Material.Rubber => 2000.0,
            Material.Ice => 2100.0,
            _ => 897.0
        };
    }

    /// <summary>
    /// 获取颜色（基于温度，用于可视化）
    /// </summary>
    public Vector4 GetTemperatureColor()
    {
        // 20°C = 蓝色，100°C = 红色
        float t = Math.Clamp((float)(Temperature - 20.0) / 80.0f, 0f, 1f);

        float r = t;                    // 红色随温度增加
        float g = 1.0f - t;             // 绿色随温度减少
        float b = 1.0f - t;             // 蓝色随温度减少

        return new Vector4(r, g, b, 1.0f);
    }
}
