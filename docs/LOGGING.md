# 日志系统使用说明

## 📝 概述

PhysicsX 内置了完整的日志系统，支持控制台和文件双输出，帮助诊断问题和追踪应用行为。

## 🎯 日志级别

| 级别 | 用途 | 控制台颜色 |
|------|------|------------|
| **Debug** | 详细调试信息 | 灰色 |
| **Info** | 一般信息 | 白色 |
| **Warning** | 警告（不影响运行） | 黄色 |
| **Error** | 错误（功能异常） | 红色 |
| **Fatal** | 致命错误（崩溃） | 深红色 |

## 📂 日志文件位置

### 自动生成路径

```
应用程序根目录/logs/physicsx_YYYYMMDD_HHMMSS.log
```

**示例**:
- Windows: `C:\Projects\PhysicsX\logs\physicsx_20260618_203045.log`
- Linux/Mac: `~/PhysicsX/logs/physicsx_20260618_203045.log`
- Termux: `/data/data/com.termux/files/home/PhysicsX/logs/physicsx_20260618_203045.log`

### 查找日志文件

启动应用后，控制台会显示：
```
[2026-06-18 20:30:45.123] INFO    [App] Log file: /path/to/logs/physicsx_20260618_203045.log
```

## 📊 日志格式

```
[时间戳] 级别     [分类] 消息内容
```

**示例**:
```
[2026-06-18 20:30:45.123] INFO    [App] PhysicsX Application Starting
[2026-06-18 20:30:45.456] DEBUG   [PhysicsCanvas] OpenGL Version: 3.3.0
[2026-06-18 20:30:46.789] WARNING [PhysicsCanvas] Engine is null, cannot sync objects
[2026-06-18 20:30:47.012] ERROR   [PhysicsCanvas] Failed to initialize renderers
```

## 🔍 关键日志分类

### 1. 应用启动 (`[App]`)

```
[App] PhysicsX Application Starting
[App] Log file: /path/to/log
[App] XAML loaded
[App] MainWindow created
[App] Framework initialization completed
```

**诊断**:
- 如果看不到这些日志 → 应用启动失败
- 如果停在某一步 → 记下最后一条日志

### 2. 窗口初始化 (`[MainWindow]`)

```
[MainWindow] MainWindow constructor called
[MainWindow] MainWindow initialized
[MainWindow] MainWindow loaded event triggered
[MainWindow] DataContext is MainWindowViewModel
[MainWindow] PhysicsCanvas control found
[MainWindow] MainWindow event bindings complete
```

**诊断**:
- `DataContext is NOT MainWindowViewModel!` → ViewModel 绑定失败
- `PhysicsCanvas control NOT found!` → XAML 名称错误

### 3. OpenGL 初始化 (`[PhysicsCanvas]`)

```
[PhysicsCanvas] OpenGL initialization started
[PhysicsCanvas] OpenGL Version: 3.3.0
[PhysicsCanvas] OpenGL Vendor: NVIDIA Corporation
[PhysicsCanvas] OpenGL Renderer: GeForce GTX 1060/PCIe/SSE2
[PhysicsCanvas] Physics engine initialized with gravity = 9.8
[PhysicsCanvas] OpenGL initialization complete. Scene has 3 objects
```

**诊断**:
- 没有这些日志 → OpenGL 未初始化
- 版本低于 3.3 → 显卡不支持
- 初始化失败 → 查看错误日志

### 4. 场景对象同步 (`[PhysicsCanvas]`)

```
[PhysicsCanvas] Syncing 1 scene objects
[PhysicsCanvas] Added: 小球 1 (CircleShape) at (0.00, 0.00)
[PhysicsCanvas] Sync complete: 1/1 objects added. Total in engine: 1
```

**诊断**:
- `Syncing 0 scene objects` → 编辑器没有对象
- `0/1 objects added` → 对象转换失败
- `Failed to convert scene object` → 查看警告/错误

### 5. 场景加载 (`[PhysicsCanvas]`)

```
[PhysicsCanvas] Loading scene: 我的场景
[PhysicsCanvas] Loaded object: 小球 1 at (0.00, 0.00)
[PhysicsCanvas] Scene loaded: 3 objects, gravity = 9.8
```

### 6. 渲染警告 (`[PhysicsCanvas]`)

```
[PhysicsCanvas] Render skipped: components not initialized
[PhysicsCanvas] No objects rendered, but engine has 3 objects
```

**诊断**:
- `components not initialized` → OpenGL 初始化失败
- `No objects rendered` → 渲染问题或位置问题

## 🐛 故障排除工作流

### 场景 1: 画板完全黑屏

**步骤**:
1. 打开日志文件
2. 搜索 `[PhysicsCanvas] OpenGL`
3. 检查是否有 OpenGL 版本信息

**如果有**:
- OpenGL 正常 → 检查场景对象
- 搜索 `Scene has X objects`
- 如果 X = 0 → 没有对象

**如果没有**:
- OpenGL 初始化失败
- 搜索 `ERROR` 查看错误原因

### 场景 2: 默认场景显示，新对象不显示

**步骤**:
1. 在应用中点击 "🔵 小球"
2. 查看控制台或日志文件
3. 搜索 `SceneObjects collection changed`

**预期日志**:
```
[MainWindow] SceneObjects collection changed: Add
[PhysicsCanvas] Syncing 1 scene objects
[PhysicsCanvas] Added: 小球 1 (CircleShape) at (0.00, 0.00)
[PhysicsCanvas] Sync complete: 1/1 objects added. Total in engine: 1
```

**如果没有第1条**:
- CollectionChanged 事件未触发
- 检查 MainWindow 是否有 `event bindings complete`

**如果没有第2-4条**:
- SyncSceneObjects 未被调用
- 或者对象转换失败

### 场景 3: 对象添加但不显示

**步骤**:
1. 确认日志显示对象已添加
2. 检查对象位置
3. 搜索 `Added:` 查看坐标

**示例**:
```
[PhysicsCanvas] Added: 小球 1 (CircleShape) at (0.00, 0.00)
```

**诊断**:
- 位置 (0, 0) 在视野中心 → 应该可见
- 位置超出范围（如 (100, 100)）→ 在视野外

**默认视野**: 约 -20 到 +20 单位

## 📋 常用日志搜索命令

### Windows (PowerShell)
```powershell
# 查找错误
Select-String -Path "logs\*.log" -Pattern "ERROR"

# 查找特定分类
Select-String -Path "logs\*.log" -Pattern "\[PhysicsCanvas\]"

# 最后100行
Get-Content "logs\physicsx_*.log" -Tail 100
```

### Linux/Mac
```bash
# 查找错误
grep "ERROR" logs/*.log

# 查找特定分类
grep "\[PhysicsCanvas\]" logs/*.log

# 最后100行
tail -100 logs/physicsx_*.log

# 实时查看（新日志追加）
tail -f logs/physicsx_*.log
```

## 🛠️ 自定义日志配置

### 修改日志级别

在 `App.axaml.cs` 中：

```csharp
// 只显示 Info 及以上级别
_logger.Configure(
    minLevel: LogLevel.Info,  // 改为 Info
    writeToConsole: true,
    writeToFile: true
);
```

### 禁用文件输出

```csharp
_logger.Configure(
    minLevel: LogLevel.Debug,
    writeToConsole: true,
    writeToFile: false  // 禁用文件
);
```

### 自定义日志路径

```csharp
_logger.Configure(
    minLevel: LogLevel.Debug,
    writeToConsole: true,
    writeToFile: true,
    customLogPath: @"C:\MyLogs\physics.log"  // 自定义路径
);
```

## 📈 日志示例（完整启动流程）

```
[2026-06-18 20:30:45.000] INFO    [App] =================================================
[2026-06-18 20:30:45.001] INFO    [App] PhysicsX Application Starting
[2026-06-18 20:30:45.002] INFO    [App] Log file: C:\Projects\PhysicsX\logs\physicsx_20260618_203045.log
[2026-06-18 20:30:45.003] INFO    [App] =================================================
[2026-06-18 20:30:45.123] INFO    [App] XAML loaded
[2026-06-18 20:30:45.234] INFO    [App] Initializing desktop application
[2026-06-18 20:30:45.345] INFO    [MainWindow] MainWindow constructor called
[2026-06-18 20:30:45.456] INFO    [MainWindow] MainWindow initialized
[2026-06-18 20:30:45.567] INFO    [App] MainWindow created
[2026-06-18 20:30:45.678] INFO    [App] Framework initialization completed
[2026-06-18 20:30:45.789] INFO    [MainWindow] MainWindow loaded event triggered
[2026-06-18 20:30:45.890] INFO    [MainWindow] DataContext is MainWindowViewModel
[2026-06-18 20:30:45.901] INFO    [MainWindow] PhysicsCanvas control found
[2026-06-18 20:30:46.012] INFO    [PhysicsCanvas] PhysicsCanvas OpenGL initialization started
[2026-06-18 20:30:46.123] INFO    [PhysicsCanvas] OpenGL Version: 3.3.0
[2026-06-18 20:30:46.234] INFO    [PhysicsCanvas] OpenGL Vendor: NVIDIA Corporation
[2026-06-18 20:30:46.345] INFO    [PhysicsCanvas] OpenGL Renderer: GeForce GTX 1060/PCIe/SSE2
[2026-06-18 20:30:46.456] DEBUG   [PhysicsCanvas] Initializing renderers...
[2026-06-18 20:30:46.567] INFO    [PhysicsCanvas] Renderers initialized successfully
[2026-06-18 20:30:46.678] INFO    [PhysicsCanvas] Physics engine initialized with gravity = 9.8
[2026-06-18 20:30:46.789] INFO    [PhysicsCanvas] OpenGL initialization complete. Scene has 3 objects
[2026-06-18 20:30:46.890] INFO    [MainWindow] No initial objects to sync
[2026-06-18 20:30:46.901] INFO    [MainWindow] MainWindow event bindings complete
```

**点击添加小球后**:
```
[2026-06-18 20:30:50.123] INFO    [MainWindow] SceneObjects collection changed: Add
[2026-06-18 20:30:50.234] INFO    [PhysicsCanvas] Syncing 1 scene objects
[2026-06-18 20:30:50.345] DEBUG   [PhysicsCanvas] Added: 小球 1 (CircleShape) at (0.00, 0.00)
[2026-06-18 20:30:50.456] INFO    [PhysicsCanvas] Sync complete: 1/1 objects added. Total in engine: 1
```

## 💡 提示

1. **保留日志**: 日志自动保留7天，之后自动删除
2. **性能**: Debug 级别会产生大量日志，生产环境建议使用 Info
3. **分享日志**: 遇到问题时，可以直接分享日志文件
4. **实时查看**: 使用 `tail -f` (Linux/Mac) 或 PowerShell 实时查看日志
5. **搜索技巧**: 善用 `grep` 或 `Select-String` 快速定位问题

---

有了完整的日志系统，现在可以精确追踪应用的每一步操作！🎉
