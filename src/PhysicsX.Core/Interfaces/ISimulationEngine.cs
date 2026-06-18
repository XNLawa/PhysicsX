using System.Collections.Generic;

namespace PhysicsX.Core.Interfaces;

/// <summary>
/// 物理仿真引擎接口
/// </summary>
public interface ISimulationEngine
{
    /// <summary>
    /// 引擎名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 重力加速度 (m/s²)
    /// </summary>
    double Gravity { get; set; }

    /// <summary>
    /// 是否暂停
    /// </summary>
    bool IsPaused { get; set; }

    /// <summary>
    /// 当前仿真时间（秒）
    /// </summary>
    double SimulationTime { get; }

    /// <summary>
    /// 所有物理对象
    /// </summary>
    IReadOnlyList<IPhysicsObject> Objects { get; }

    /// <summary>
    /// 添加物理对象
    /// </summary>
    void AddObject(IPhysicsObject obj);

    /// <summary>
    /// 移除物理对象
    /// </summary>
    bool RemoveObject(string objectId);

    /// <summary>
    /// 清空所有对象
    /// </summary>
    void Clear();

    /// <summary>
    /// 执行一个仿真时间步
    /// </summary>
    /// <param name="deltaTime">时间步长（秒）</param>
    void Step(double deltaTime);

    /// <summary>
    /// 重置仿真状态
    /// </summary>
    void Reset();
}
