# 渲染问题调试指南

## 已修复的问题

### 1. 删除了测试绘制代码
- **位置**: `PhysicsCanvas.cs:198`
- **问题**: 红色对角线测试代码在错误的位置（投影矩阵设置之前）
- **修复**: 已删除

### 2. 修复了 CircleRenderer 性能问题
- **问题**: 每次 `Draw()` 都调用 `BufferData` 重新分配内存
- **修复**: 改用 `BufferSubData` 更新现有缓冲区

### 3. 修复了模型矩阵设置顺序
- **问题**: 在 `RenderRigidBody` 中，部分形状没有正确设置模型矩阵
- **修复**: 确保所有形状绘制前都设置正确的 `uModel` 矩阵

## 如果仍然是黑屏，检查以下项：

### 检查 OpenGL 初始化
运行应用后，应该看到类似输出：
```
[PhysicsCanvas] OpenGL initialization started
[PhysicsCanvas] OpenGL Version: 3.x.x
[PhysicsCanvas] Physics engine initialized with gravity = 9.8
[PhysicsCanvas] OpenGL initialization complete. Scene has 3 objects
```

如果没有这些输出，OpenGL 初始化失败。

### 检查场景对象
默认场景应该包含：
- **Ground**: 位置 (0, 8)，静态矩形
- **Ball 1**: 位置 (-3, 0)，半径 0.5
- **Ball 2**: 位置 (3, 2)，半径 0.6

### 检查视野范围
- **缩放**: `_zoom = 20f`（每单位 20 像素）
- **视野宽度**: `Bounds.Width / 20`（假设 800px 宽 = 40 单位）
- **视野高度**: `Bounds.Height / 20`（假设 600px 高 = 30 单位）
- **默认视野**: 约 (-20, -15) 到 (20, 15)

物体位置应该在这个范围内可见。

### 调试步骤

1. **确认 OpenGL 版本支持**
   - 需要 OpenGL 3.0+ 或 OpenGL ES 3.0+
   - Termux 通过 VNC 或 Xwayland 可能有限制

2. **检查 Shader 编译**
   - 如果 Shader 编译失败，会抛出异常
   - 查看是否有异常输出

3. **验证顶点数据**
   - CircleRenderer 使用 TriangleFan 绘制 32 段圆
   - 确保 VAO/VBO 正确绑定

4. **检查清屏是否工作**
   - 应该看到深灰色背景 (0.1, 0.1, 0.15)
   - 如果是黑色 (0, 0, 0)，可能是 Clear 调用失败

5. **矩阵变换检查**
   ```
   投影矩阵: 正交投影，左右翻转 Y 轴
   视图矩阵: 相机偏移（默认 0,0）
   模型矩阵: 每个物体的位置和缩放
   ```

## 临时诊断代码

如果需要进一步诊断，可以在 `OnOpenGlRender` 开头添加：

```csharp
// 测试：绘制屏幕空间的 X
_shader.Use();
_shader.SetUniform("uProjection", Matrix4x4.Identity);
_shader.SetUniform("uView", Matrix4x4.Identity);
_shader.SetUniform("uModel", Matrix4x4.Identity);

_lineRenderer.DrawLine(
    new Vector2(-0.8f, -0.8f), 
    new Vector2(0.8f, 0.8f), 
    new Vector4(1, 0, 0, 1), 5.0f);
_lineRenderer.DrawLine(
    new Vector2(-0.8f, 0.8f), 
    new Vector2(0.8f, -0.8f), 
    new Vector4(0, 1, 0, 1), 5.0f);
```

如果看到红绿交叉线，说明 OpenGL 渲染工作正常，问题在于坐标系统或场景对象。

## Termux 特殊注意事项

在 Termux 上运行 Avalonia OpenGL 应用需要：

1. **X11 服务器**（如 VNC 或 Termux-X11）
2. **OpenGL 驱动**
   ```bash
   pkg install mesa
   export GALLIUM_DRIVER=llvmpipe  # 软件渲染
   ```

3. **可能需要设置 DISPLAY**
   ```bash
   export DISPLAY=:1
   ```

如果 Termux 不支持 OpenGL，考虑：
- 使用远程桌面在真实 Linux 环境运行
- 或者在 PC 上交叉编译测试
