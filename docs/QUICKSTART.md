# PhysicsX 快速开始指南

## 🚀 5分钟上手

### 第一步：克隆并构建

```bash
# 克隆项目
git clone <your-repo-url>
cd PhysicsX

# 构建项目
dotnet build

# 运行测试（可选）
dotnet test
```

### 第二步：运行应用

```bash
dotnet run --project src/PhysicsX.App
```

**注意**: 需要在支持 GUI 的环境中运行（Windows/Linux Desktop/macOS）

### 第三步：体验功能

1. **播放仿真**
   - 点击 "▶ 播放" 按钮
   - 观察两个小球自由落体和弹性碰撞

2. **调整重力**
   - 点击 🌙 月球按钮
   - 观察重力变为 1.62 m/s²
   - 小球下落变慢！

3. **自定义重力**
   - 在输入框中输入 `50.0`
   - 观察极强的重力效果
   - 尝试 `0.0` 看太空环境

4. **重置场景**
   - 点击 "⟲ 重置" 按钮
   - 一切恢复初始状态

---

## 🎮 UI 界面说明

```
┌─────────────────────────────────────────────────────────┐
│  [▶ 播放]  [⟲ 重置]  │  重力: [9.8] 🌍🌙🔴🪐🚀  │  时间: 0.00s  │
├─────────────────────────────────────────────────────────┤
│                                                         │
│                   🔵 小球 1                              │
│                                                         │
│                                     🔵 小球 2            │
│                                                         │
│                                                         │
│  ═════════════════════════════════════════════════      │
│                     绿色地面                            │
└─────────────────────────────────────────────────────────┘
```

### 控制按钮

| 按钮 | 功能 |
|------|------|
| ▶ 播放 / ⏸ 暂停 | 开始或暂停物理仿真 |
| ⟲ 重置 | 恢复初始状态 |

### 重力预设

| 图标 | 星球 | 重力 (m/s²) |
|------|------|-------------|
| 🌍   | 地球 | 9.8         |
| 🌙   | 月球 | 1.62        |
| 🔴   | 火星 | 3.71        |
| 🪐   | 木星 | 24.79       |
| 🚀   | 太空 | 0.0         |

---

## 🧪 实验建议

### 实验 1：自由落体对比

**目标**: 验证不同星球的重力差异

1. 点击 "⟲ 重置"
2. 点击 🌍 地球，播放 → 记录小球下落速度
3. 重置，点击 🌙 月球，播放 → 对比速度
4. 重置，点击 🪐 木星，播放 → 观察极快下落

**预期结果**: 木星 > 地球 > 火星 > 月球 > 太空

### 实验 2：弹性碰撞观察

**目标**: 观察能量损失

1. 重置到地球重力
2. 播放并观察小球与地面碰撞
3. 注意每次弹起高度逐渐降低（能量损失）

### 实验 3：极端重力测试

**目标**: 测试引擎稳定性

1. 在输入框中输入 `100`（极强重力）
2. 播放 → 观察瞬间下落
3. 输入 `0.01`（微重力）
4. 播放 → 观察缓慢飘落

---

## 📝 代码示例

### 创建自定义场景

```csharp
// 在 PhysicsCanvas.cs 的 CreateSampleScene() 中修改

private void CreateSampleScene()
{
    if (_engine == null) return;

    // 创建一个大球
    var bigBall = new RigidBody("Big Ball")
    {
        Mass = 5.0,                    // 5kg
        Position = new Vector2(0, 0),
        Restitution = 0.9,             // 90% 弹性
        Friction = 0.1,                // 低摩擦
        UseGravity = true,
        Shape = new CircleShape(1.0f)  // 半径 1m
    };
    _engine.AddObject(bigBall);

    // 地面
    var ground = new RigidBody("Ground")
    {
        Position = new Vector2(0, 10),
        IsStatic = true,
        Shape = new BoxShape(30, 1)
    };
    _engine.AddObject(ground);
}
```

### 添加新的重力预设

在 `MainWindowViewModel.cs` 中添加：

```csharp
[RelayCommand]
private void SetCustomGravity()
{
    Gravity = 15.0;  // 自定义值
}
```

在 `MainWindow.axaml` 中添加按钮：

```xml
<Button Command="{Binding SetCustomGravityCommand}"
        Content="⚡ 自定义" 
        ToolTip.Tip="15.0 m/s²" 
        Height="28"/>
```

---

## 🐛 常见问题

### Q1: 应用无法启动

**原因**: 缺少 .NET 8.0 SDK 或 OpenGL 驱动

**解决**:
```bash
# 检查 .NET 版本
dotnet --version

# 应该显示 8.0.x
```

### Q2: 画面是黑色的

**原因**: OpenGL 初始化失败

**解决**:
- 更新显卡驱动
- 检查是否支持 OpenGL 3.3+

### Q3: 在 Termux 中无法运行

**原因**: Termux 不支持 GUI 应用

**解决**:
- 在 Windows/Linux 桌面环境运行
- 或使用 VNC/X11 转发

---

## 📚 进阶学习

### 推荐阅读顺序

1. **架构文档**: `docs/ARCHITECTURE.md`
2. **碰撞系统**: `docs/UPDATE_COLLISION_RENDERING.md`
3. **完整报告**: `docs/FINAL_REPORT.md`

### 代码结构

```
重点阅读:
- src/PhysicsX.Core/Engines/MechanicsEngine.cs  # 物理引擎核心
- src/PhysicsX.Core/Collision/CollisionDetector.cs  # 碰撞检测
- src/PhysicsX.App/Controls/PhysicsCanvas.cs  # 渲染逻辑
```

### 单元测试

```bash
# 运行特定测试
dotnet test --filter "ClassName=CollisionTests"

# 查看详细输出
dotnet test --verbosity detailed
```

---

## 💡 提示与技巧

1. **调整相机缩放**
   - 在 `PhysicsCanvas.cs` 中修改 `_zoom` 值
   - 默认 20 像素/米

2. **修改时间步长**
   - 在 `PhysicsCanvas.cs` 中修改定时器间隔
   - 默认 60 FPS (16.67ms)

3. **添加更多物体**
   - 修改 `CreateSampleScene()` 方法
   - 使用 `_engine.AddObject()`

---

## 🎯 快速命令参考

```bash
# 构建
dotnet build

# 测试
dotnet test

# 运行
dotnet run --project src/PhysicsX.App

# 清理
dotnet clean

# 查看帮助
dotnet --help
```

---

**准备好了吗？现在就运行应用，体验物理的魅力！** 🚀

```bash
dotnet run --project src/PhysicsX.App
```
