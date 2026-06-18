# GitHub Actions 自动发布说明

## 功能

这个 GitHub Actions 工作流会自动：

1. ✅ **每次推送到 master** - 运行测试，确保代码质量
2. ✅ **编译跨平台二进制文件** - 支持 4 个平台：
   - Windows x64
   - Linux x64
   - macOS x64 (Intel)
   - macOS ARM64 (Apple Silicon)
3. ✅ **打包为 ZIP** - 每个平台一个独立压缩包
4. ✅ **自动创建 Release** - 当你推送 tag 时

## 如何发布新版本

### 方法 1: 通过 Git Tag（推荐）

```bash
# 1. 确保所有更改已提交
git add .
git commit -m "feat: 准备发布 v1.0.0"

# 2. 创建并推送 tag
git tag v1.0.0
git push origin master
git push origin v1.0.0
```

### 方法 2: 通过 GitHub 网页

1. 进入你的 GitHub 仓库
2. 点击 "Releases" → "Create a new release"
3. 点击 "Choose a tag" → 输入 `v1.0.0` → "Create new tag"
4. 填写 Release 标题和说明
5. 点击 "Publish release"

## 发布后的产物

GitHub Actions 会自动生成 4 个 ZIP 文件并附加到 Release：

```
PhysicsX-win-x64.zip       (Windows 版本)
PhysicsX-linux-x64.zip     (Linux 版本)
PhysicsX-osx-x64.zip       (macOS Intel 版本)
PhysicsX-osx-arm64.zip     (macOS Apple Silicon 版本)
```

每个 ZIP 包含：
- PhysicsX.App 可执行文件
- 所有必需的 .NET 运行时（自包含）
- 依赖的 DLL 文件

## 用户下载和使用

用户可以：

1. 进入你的 GitHub Releases 页面
2. 下载对应平台的 ZIP 文件
3. 解压缩
4. 直接运行 `PhysicsX.App` (或 `PhysicsX.App.exe` on Windows)

**无需安装 .NET 运行时！**

## 工作流程图

```
Push to master
    ↓
Run Tests (Ubuntu)
    ↓ (Pass)
Compile on 3 OS × 4 platforms
    ├─ Windows runner → win-x64.zip
    ├─ Ubuntu runner → linux-x64.zip
    └─ macOS runner → osx-x64.zip + osx-arm64.zip
    ↓
Upload as Artifacts (可在 Actions 页面下载)
    ↓ (If tag pushed)
Create GitHub Release
    └─ Attach all ZIP files
```

## 配置选项

在 `.github/workflows/release.yml` 中可调整：

| 选项 | 说明 | 默认值 |
|------|------|--------|
| `PublishSingleFile` | 单文件发布 | true |
| `PublishTrimmed` | 裁剪未使用代码 | true |
| `--self-contained` | 包含运行时 | true |

## 常见问题

### Q: 如何测试编译但不发布？

A: 直接推送到 master，不打 tag。编译产物会作为 Artifacts 保存 90 天。

### Q: 如何下载 Artifacts？

A: GitHub 仓库 → Actions → 点击任意 workflow run → 底部 "Artifacts" 区域

### Q: 编译失败怎么办？

A: 
1. 查看 Actions 页面的错误日志
2. 本地运行 `dotnet publish` 测试
3. 检查项目引用和依赖

### Q: 能自动发布到其他地方吗？

A: 可以！在 workflow 中添加：
- NuGet 包发布
- Docker 镜像构建
- Steam 发布
- 等等

## 示例：本地测试编译命令

```bash
# Windows
dotnet publish src/PhysicsX.App/PhysicsX.App.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true

# Linux
dotnet publish src/PhysicsX.App/PhysicsX.App.csproj -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true

# macOS Intel
dotnet publish src/PhysicsX.App/PhysicsX.App.csproj -c Release -r osx-x64 --self-contained -p:PublishSingleFile=true

# macOS ARM (M1/M2/M3)
dotnet publish src/PhysicsX.App/PhysicsX.App.csproj -c Release -r osx-arm64 --self-contained -p:PublishSingleFile=true
```

## 版本号规范

建议使用语义化版本（Semantic Versioning）：

- `v1.0.0` - 第一个稳定版本
- `v1.1.0` - 添加新功能
- `v1.1.1` - 修复 bug
- `v2.0.0` - 重大更新（破坏性变更）

## 注意事项

1. **首次运行**可能需要 20-30 分钟（编译 4 个平台）
2. **每次 push** 都会触发测试（快速，约 2-3 分钟）
3. **只有 tag** 才会创建 Release
4. **Artifacts 保留 90 天**（GitHub 免费版限制）
5. **Release 永久保存**

---

**准备好了吗？创建你的第一个 Release：**

```bash
git tag v0.1.0
git push origin master
git push origin v0.1.0
```

然后访问你的 GitHub Releases 页面查看结果！🚀
