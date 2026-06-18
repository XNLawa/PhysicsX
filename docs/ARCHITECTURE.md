# PhysicsX 系统架构设计

## 1. 系统概述

PhysicsX 是一款基于 .NET Avalonia UI 的跨平台物理仿真与解题演示软件，采用分层架构设计，支持多物理场仿真、AI 智能识别和高性能实时渲染。

## 2. 架构层次图

```
┌─────────────────────────────────────────────────────────────────┐
│                        Presentation Layer                        │
│                      (Avalonia UI + Fluent 2)                   │
├─────────────────────────────────────────────────────────────────┤
│  MainWindow          Canvas View         Properties Panel       │
│  - Toolbar           - OpenGL Render     - Parameter Editor     │
│  - Menu              - Gesture Handler   - Object Inspector     │
│  - Status Bar        - Grid System       - Animation Timeline   │
└────────────┬────────────────────────────────────────────────────┘
             │
             ├─ MVVM Data Binding
             ↓
┌─────────────────────────────────────────────────────────────────┐
│                      Application Layer                           │
│                  (ViewModels + Services)                         │
├─────────────────────────────────────────────────────────────────┤
│  MainViewModel       │  CanvasViewModel    │  AIServiceAdapter  │
│  - Command Routing   │  - Scene Management │  - Vision API      │
│  - State Management  │  - Selection Logic  │  - Text Parsing    │
│                      │  - Undo/Redo Stack  │  - JSON Generator  │
├──────────────────────┼─────────────────────┼────────────────────┤
│  ExportService       │  ProjectService     │  GestureRecognizer │
│  - Video Encoding    │  - Save/Load        │  - Shape Detection │
│  - GIF Generation    │  - Format Handling  │  - Auto-snap Grid  │
└────────────┬────────────────────────────────────────────────────┘
             │
             ├─ Business Logic Interfaces
             ↓
┌─────────────────────────────────────────────────────────────────┐
│                      Simulation Engine Layer                     │
│                  (Physics Calculation Core)                      │
├─────────────────────────────────────────────────────────────────┤
│  ISimulationEngine (Abstract)                                   │
│  ├─ MechanicsEngine      ├─ ElectricityEngine                  │
│  │  - Rigid Body         │  - Circuit Solver (Kirchhoff)       │
│  │  - Collision Detect   │  - AC/DC Analysis                   │
│  │  - Constraint Solver  │  - Capacitor/Inductor ODE           │
│  │  - Force Accumulator  │                                      │
│  │                       │                                      │
│  └─ ElectromagneticsEngine                                      │
│     - Electric Field (Coulomb's Law)                            │
│     - Magnetic Field (Biot-Savart)                              │
│     - Lorentz Force Calculator                                  │
│     - Ampere's Law / Faraday's Law                              │
├─────────────────────────────────────────────────────────────────┤
│  Integrator:  Runge-Kutta 4 (RK4) / Verlet / Symplectic Euler │
│  Time Step:   Adaptive Δt with error control                   │
└────────────┬────────────────────────────────────────────────────┘
             │
             ├─ Scene Graph + Render Commands
             ↓
┌─────────────────────────────────────────────────────────────────┐
│                      Rendering Layer                             │
│              (OpenGL via Silk.NET + Avalonia)                   │
├─────────────────────────────────────────────────────────────────┤
│  PhysicsCanvas (OpenGlControlBase)                              │
│  ├─ Renderer Pipeline                                           │
│  │  - Batch Renderer (instanced draw calls)                    │
│  │  - Shader Manager (GLSL 3.3+)                               │
│  │  - Framebuffer for offscreen rendering                      │
│  │                                                              │
│  ├─ Visual Components                                           │
│  │  - CircleRenderer (particles, balls)                        │
│  │  - LineRenderer (ropes, force vectors, field lines)         │
│  │  - PolygonRenderer (rigid bodies, surfaces)                 │
│  │  - TextRenderer (labels, measurements)                      │
│  │                                                              │
│  └─ Effects & Post-Processing                                   │
│     - Motion Blur (velocity-based)                             │
│     - Glow (electric charges, current flow)                    │
│     - Trail Effect (trajectory visualization)                  │
└────────────┬────────────────────────────────────────────────────┘
             │
             ↓
┌─────────────────────────────────────────────────────────────────┐
│                      Data Layer                                  │
├─────────────────────────────────────────────────────────────────┤
│  Scene Model:                                                    │
│  - PhysicsObject (base class)                                   │
│    ├─ RigidBody (mass, velocity, rotation)                     │
│    ├─ ElectricComponent (voltage, current, resistance)         │
│    └─ FieldSource (charge, magnetic moment)                    │
│                                                                  │
│  Serialization:  JSON (System.Text.Json)                        │
│  Format Version: 1.0 (extensible schema)                        │
└─────────────────────────────────────────────────────────────────┘
```

## 3. 核心交互流程

### 3.1 用户手绘物理场景流程

```
User Draw Gesture
       ↓
GestureRecognizer (识别形状: 圆形→球体, 直线→斜面)
       ↓
CanvasViewModel.AddObject(PhysicsObject)
       ↓
SimulationEngine.RegisterEntity(entity)
       ↓
PhysicsCanvas.InvalidateVisual() → Render Update
```

### 3.2 AI 识别物理题流程

```
User Upload Image
       ↓
AIServiceAdapter.AnalyzeImage(image)
       ↓
GPT-4 Vision API (返回结构化 JSON)
       ↓
JSON Parser → SceneDescriptor
       ↓
SceneBuilder.BuildFromDescriptor(descriptor)
       ↓
SimulationEngine.LoadScene(scene)
       ↓
PhysicsCanvas.Render(scene)
```

### 3.3 物理仿真更新循环 (Game Loop)

```
60 FPS Timer Tick
       ↓
SimulationEngine.Step(deltaTime)
  ├─ AccumulateForces()
  ├─ IntegrateVelocity(RK4)
  ├─ DetectCollisions()
  ├─ ResolveConstraints()
  └─ UpdateFieldLines()
       ↓
CanvasViewModel.UpdateVisuals()
       ↓
PhysicsCanvas.OnRender(GL context)
  ├─ Clear Framebuffer
  ├─ Draw Background Grid
  ├─ Batch Render All Objects
  └─ Draw Debug Overlays (force vectors, velocity)
```

### 3.4 视频导出流程

```
User Click "Export Video"
       ↓
ExportService.StartRecording(fps=60, codec=H264)
       ↓
For each frame:
  ├─ SimulationEngine.Step(1/60)
  ├─ PhysicsCanvas.RenderToTexture(offscreen FBO)
  ├─ FFmpeg.WriteFrame(texture_data)
       ↓
FFmpeg.Finalize() → MP4 File Saved
```

## 4. 技术栈明细

| 层级           | 技术选型                          | 说明                                  |
|----------------|-----------------------------------|---------------------------------------|
| UI 框架        | Avalonia 11.2+ (FluentTheme)     | 跨平台 XAML UI，支持 Fluent 2 风格   |
| 渲染           | Silk.NET.OpenGL 2.21+            | 现代 OpenGL 绑定，GPU 加速            |
| 物理引擎       | 自定义 C# 实现                    | 深度可控，支持多物理场耦合            |
| 数学库         | System.Numerics + MathNet.Numerics| 向量、矩阵、ODE 求解                 |
| AI 接口        | OpenAI SDK / Anthropic SDK       | 多模态图像理解                        |
| 视频编码       | FFmpeg.AutoGen 6.0+              | 内存级帧编码，无临时文件              |
| 序列化         | System.Text.Json                 | 高性能 JSON 处理                      |
| 单元测试       | xUnit + FluentAssertions         | 物理计算精度验证                      |

## 5. 模块职责分离

### 5.1 PhysicsX.Core (核心库 - .NET Standard 2.1)
- `ISimulationEngine` 接口定义
- 物理计算实现（不依赖 UI）
- 数学工具类
- 场景数据模型

### 5.2 PhysicsX.Rendering (渲染库)
- OpenGL 渲染管线
- Shader 管理
- 批处理优化
- 后期特效

### 5.3 PhysicsX.AI (AI 集成)
- Vision API 封装
- 提示词工程（Prompt Engineering）
- JSON Schema 定义
- 物理实体映射

### 5.4 PhysicsX.App (主应用)
- Avalonia UI 视图
- MVVM ViewModels
- 依赖注入容器
- 应用生命周期管理

### 5.5 PhysicsX.Export (导出模块)
- FFmpeg 集成
- GIF 优化
- 帧缓冲管理

## 6. 性能优化策略

### 6.1 渲染优化
- **批处理绘制**：相同材质的物体合并为单次 draw call
- **视锥剔除**：仅渲染可见区域内的物体
- **LOD 系统**：远距离物体使用简化模型
- **实例化渲染**：大量相似粒子使用 GPU Instancing

### 6.2 物理计算优化
- **空间哈希**：碰撞检测使用网格划分（O(n²) → O(n)）
- **休眠机制**：静止物体暂停模拟
- **多线程**：力计算并行化（`Parallel.For`）
- **可变时间步长**：高速碰撞时自动细分 Δt

### 6.3 内存管理
- **对象池**：频繁创建的物体（粒子、碰撞点）复用
- **AOT 编译**：Native AOT 发布，减少启动时间

## 7. 扩展性设计

### 7.1 插件系统（未来规划）
```csharp
public interface IPhysicsModule
{
    string Name { get; }
    void Initialize(ISimulationEngine engine);
    void Update(double deltaTime);
}
```

### 7.2 自定义 Shader（高级用户）
用户可编写 GLSL 片段着色器实现自定义视觉效果：
```glsl
// 示例：霓虹光晕效果
uniform vec3 glowColor;
uniform float intensity;

void main() {
    float dist = length(fragPos - objectCenter);
    float glow = exp(-dist * intensity);
    fragColor = vec4(glowColor * glow, 1.0);
}
```

## 8. 安全性与隐私

- **AI 数据处理**：用户上传的图片仅用于题目解析，不保存到服务器
- **本地优先**：所有仿真计算在本地执行，无需网络
- **开源透明**：核心物理引擎代码完全开放，可审计

## 9. 测试策略

### 9.1 物理精度测试
- 自由落体：验证 `h = 1/2 g t²`
- 弹性碰撞：验证动量守恒和能量守恒
- 简谐振动：验证周期 `T = 2π√(m/k)`

### 9.2 性能基准测试
- 1000 粒子同时模拟需保持 60 FPS
- 启动时间 < 2 秒（冷启动）
- 内存占用 < 200MB（空场景）

### 9.3 跨平台兼容性测试
- Windows 10/11 (x64/ARM64)
- macOS 12+ (Intel/Apple Silicon)
- Linux (Ubuntu 22.04, Fedora 38)

## 10. 发布计划

### Alpha 阶段（当前）
- ✅ 项目结构搭建
- ✅ OpenGL 渲染画板
- ⏳ 基础力学仿真（刚体、碰撞）

### Beta 阶段（3个月后）
- 电学电路模拟
- AI 图像识别集成
- 视频导出功能

### 正式版（6个月后）
- 电磁场可视化
- 完整的用户文档
- 社区题库分享

---

**架构设计原则**：
1. **职责分离**：UI、业务逻辑、物理计算严格解耦
2. **可测试性**：核心物理引擎无 UI 依赖，便于单元测试
3. **性能优先**：GPU 加速 + 多线程 + 批处理
4. **开放扩展**：模块化设计，便于社区贡献新的物理模型
