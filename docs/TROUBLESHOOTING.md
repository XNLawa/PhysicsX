# 画板显示问题诊断指南

## 🔍 问题诊断步骤

### 1. 检查是否有默认场景

启动应用后，你应该看到：
- 1个绿色地面（底部）
- 2个蓝色小球（悬浮在空中）

**如果看不到**：
- ❌ OpenGL 初始化失败
- ❌ 渲染器未正确创建
- ❌ Shader 编译错误

**解决方案**：
```bash
# 查看控制台输出，寻找 OpenGL 错误
# 确保系统支持 OpenGL 3.3+
```

### 2. 测试添加对象

点击左侧 "🔵 小球" 按钮：

**预期行为**：
- 左侧列表出现 "小球 1"
- 画布中心出现一个蓝色圆圈
- 控制台输出：
  ```
  [PhysicsCanvas] Added: 小球 1 at <0, 0>
  [PhysicsCanvas] Total objects in engine: 1
  ```

**如果看不到对象但有日志**：
- ✅ 同步工作正常
- ❌ 渲染问题（可能是坐标、缩放或颜色）

**如果没有日志**：
- ❌ CollectionChanged 事件未触发
- ❌ MainWindow 连接失败

### 3. 检查相机视图

默认相机设置：
- 缩放：20 像素/单位
- 中心：(0, 0)
- 视野：约 40x30 单位

**可能的问题**：
- 对象位置超出视野范围
- 缩放过大或过小
- Y 轴方向错误

### 4. 调试检查清单

#### A. 检查 OpenGL 初始化
```csharp
// 在 PhysicsCanvas.OnOpenGlInit 添加：
System.Diagnostics.Debug.WriteLine("[PhysicsCanvas] OpenGL initialized");
System.Diagnostics.Debug.WriteLine($"[PhysicsCanvas] GL Version: {_gl?.GetStringS(StringName.Version)}");
```

#### B. 检查渲染调用
```csharp
// 在 OnOpenGlRender 开始添加：
System.Diagnostics.Debug.WriteLine($"[PhysicsCanvas] Rendering {_engine?.Objects.Count ?? 0} objects");
```

#### C. 检查对象位置
```csharp
// 在 RenderRigidBody 开始添加：
System.Diagnostics.Debug.WriteLine($"[PhysicsCanvas] Rendering {body.Name} at {body.Position}");
```

### 5. 常见问题和解决方案

#### 问题 1: 黑屏，没有任何内容

**可能原因**：
- OpenGL 上下文未创建
- Shader 编译失败
- 清屏颜色太暗

**解决**：
```csharp
// 改变清屏颜色以验证
_gl.ClearColor(1.0f, 0.0f, 0.0f, 1.0f); // 红色背景
```

#### 问题 2: 有默认场景，但新对象不显示

**可能原因**：
- SyncSceneObjects 清空了默认场景
- 对象位置在视野外

**解决**：
```csharp
// 检查对象位置
foreach (var obj in _engine.Objects)
{
    if (obj is RigidBody rb)
    {
        Debug.WriteLine($"Object: {rb.Name}, Pos: {rb.Position}, Shape: {rb.Shape?.GetType().Name}");
    }
}
```

#### 问题 3: CollectionChanged 不触发

**可能原因**：
- DataContext 未设置
- 事件连接失败

**验证**：
```csharp
// 在 AddCircle 命令中添加：
System.Diagnostics.Debug.WriteLine($"[SceneEditor] Added circle, total: {SceneObjects.Count}");
```

#### 问题 4: 对象位置都是 (0, 0)

**可能原因**：
- 默认位置没有设置
- 所有对象重叠在中心

**解决**：
```csharp
// 在添加对象时随机位置
Position = new Vector2(
    (float)(Random.Shared.NextDouble() * 10 - 5),
    (float)(Random.Shared.NextDouble() * 10 - 5)
)
```

### 6. 手动测试步骤

1. **启动应用**
   - 预期：看到灰色画布，绿色地面，两个蓝色球

2. **点击 "🔵 小球"**
   - 预期：列表增加 "小球 1"，画布中心出现蓝色圆

3. **修改位置**
   - 在右侧属性面板设置 X=5, Y=3
   - 预期：球移动到右上

4. **点击 "▶ 播放"**
   - 预期：球开始下落，碰到地面反弹

5. **修改重力**
   - 设置为 1.62（月球）
   - 预期：球下落变慢

### 7. 临时调试代码

在 `MainWindow.axaml.cs` 的 `OnWindowLoaded` 最后添加：

```csharp
// 强制添加一个测试对象
System.Diagnostics.Debug.WriteLine("[MainWindow] Adding test object");
viewModel.SceneEditor.AddCircleCommand.Execute(null);
System.Diagnostics.Debug.WriteLine($"[MainWindow] SceneObjects count: {viewModel.SceneEditor.SceneObjects.Count}");
```

### 8. 输出示例

**正常情况**：
```
[MainWindow] Adding test object
[SceneEditor] Added circle, total: 1
[PhysicsCanvas] Added: 小球 1 at <0, 0>
[PhysicsCanvas] Total objects in engine: 1
[PhysicsCanvas] Rendering 1 objects
[PhysicsCanvas] Rendering 小球 1 at <0, 0>
```

**异常情况**：
```
[MainWindow] Adding test object
[SceneEditor] Added circle, total: 1
// 没有 PhysicsCanvas 日志 → 事件未触发
```

---

## 🚀 如果还是不行

### 最后的诊断步骤

1. **重新构建项目**
   ```bash
   dotnet clean
   dotnet build
   ```

2. **检查依赖**
   ```bash
   dotnet restore
   ```

3. **检查 OpenGL 支持**
   - Windows: 确保显卡驱动最新
   - Linux: 安装 mesa-utils，运行 `glxinfo | grep "OpenGL version"`
   - macOS: 应该原生支持

4. **尝试其他渲染后端**
   - Avalonia 支持软件渲染
   - 在 Program.cs 中配置

5. **分享完整日志**
   - 启用详细日志
   - 分享控制台输出

---

## 📝 我需要的信息

如果问题仍然存在，请提供：

1. **操作系统**：Windows/Linux/macOS 版本
2. **显卡型号**：
3. **控制台输出**：（包含所有 Debug 日志）
4. **行为描述**：
   - [ ] 完全黑屏
   - [ ] 有默认场景但新对象不显示
   - [ ] 有列表但画布空白
   - [ ] 其他：___________

5. **测试结果**：
   - [ ] 点击小球按钮后列表有对象
   - [ ] 控制台有 "[PhysicsCanvas] Added" 日志
   - [ ] 控制台有 "[PhysicsCanvas] Rendering" 日志
