# PhysicsX

<div align="center">

![PhysicsX Logo](docs/assets/logo.png)

**现代化、轻量级跨平台物理仿真与解题演示软件**

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![Avalonia](https://img.shields.io/badge/Avalonia-11.2-orange.svg)](https://avaloniaui.net/)

[功能特性](#功能特性) • [快速开始](#快速开始) • [架构设计](#架构设计) • [贡献指南](#贡献指南)

</div>

---

## 🎯 产品定位

PhysicsX 是一款面向中学及大学物理教育的**物理题意理解与动态仿真工具**，具有：

- 🎨 **现代化 UI**：基于 Avalonia Fluent 2 设计语言，拥有圆角、半透明磨砂质感
- 🧲 **多物理场仿真**：支持力学、电学、电磁学的实时 2D/2.5D 模拟
- 🤖 **AI 智能识别**：拍照/截图物理题，AI 自动生成可交互仿真模型
- ✏️ **手绘建模**：草图转几何，随手画圆识别为小球，画斜线识别为斜面
- 🎬 **高性能渲染**：OpenGL GPU 加速，60 FPS 流畅体验
- 📹 **视频导出**：一键录制仿真过程为 MP4/GIF

## ✨ 功能特性

### 多物理场仿真引擎

#### 🔵 力学 (Mechanics)
- 刚体动力学（质量、惯性、旋转）
- 碰撞检测与响应（弹性/非弹性碰撞）
- 约束系统（弹簧、绳索、滑轮、斜面）
- 实时受力分析图（动态力向量渲染）

#### ⚡ 电学 (Electricity)
- 直流/交流电路元件（电源、电阻、电容、电感）
- 拖拽搭建电路，实时模拟电流流动动画
- 内置示波器波形显示

#### 🧲 电磁学 (Electromagnetics)
- 点电荷电场线分布可视化
- 匀强磁场中带电粒子偏转运动
- 电磁感应现象（洛伦兹力、安培力实时渲染）

### AI 驱动的智能建模

```
用户上传物理题截图
    ↓
GPT-4 Vision 解析文本
    ↓
提取物理实体（"2kg 小球，10m/s 初速度"）
    ↓
生成 JSON 描述文件
    ↓
自动在画布上构建仿真场景
```

### 交互式物理画布

- **草图识别**：手绘圆形 → 自动识别为小球
- **参数面板**：双击物体弹出编辑器，精准调节质量、摩擦力、电荷量等
- **时间轴控制**：暂停、慢放、单帧回溯、快进
- **网格对齐**：智能吸附，辅助精确布局

## 🚀 快速开始

### 环境要求

- **操作系统**：Windows 10/11 (x64/ARM64)
- **.NET SDK**：8.0 或更高版本
- **GPU**：支持 OpenGL 3.3+ 的显卡

### 安装

#### 从源码构建

```bash
# 克隆仓库
git clone https://github.com/yourusername/PhysicsX.git
cd PhysicsX

# 恢复 NuGet 包
dotnet restore

# 构建解决方案
dotnet build

# 运行应用
dotnet run --project src/PhysicsX.App
```

#### 下载预构建版本

前往 [Releases](https://github.com/yourusername/PhysicsX/releases) 页面下载最新版本。

### 基本使用

1. **创建物体**：点击工具栏的"添加"按钮或手绘形状
2. **调整参数**：双击物体打开属性面板
3. **开始模拟**：点击"播放"按钮，观察物理运动
4. **AI 识别**：点击"导入题目"，上传物理题图片
5. **导出视频**：点击"导出"按钮，选择格式和质量

## 🏗️ 架构设计

PhysicsX 采用分层架构，模块高度解耦：

```
┌─────────────────────────────────────────┐
│      Presentation Layer (Avalonia)      │  ← UI、控件、视图
├─────────────────────────────────────────┤
│   Application Layer (ViewModels+Svcs)   │  ← MVVM、业务逻辑
├─────────────────────────────────────────┤
│   Simulation Engine (Physics Core)      │  ← 物理计算引擎
├─────────────────────────────────────────┤
│   Rendering Layer (OpenGL via Silk.NET) │  ← GPU 渲染管线
├─────────────────────────────────────────┤
│        Data Layer (Scene Models)        │  ← 数据模型、序列化
└─────────────────────────────────────────┘
```

详细架构文档：[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)

### 项目结构

```
PhysicsX/
├── src/
│   ├── PhysicsX.Core/         # 核心物理引擎（无 UI 依赖）
│   ├── PhysicsX.Rendering/    # OpenGL 渲染库
│   ├── PhysicsX.AI/           # AI 集成模块
│   ├── PhysicsX.Export/       # 视频导出
│   └── PhysicsX.App/          # Avalonia 主应用
├── tests/                      # 单元测试与集成测试
├── samples/                    # 示例场景文件
└── docs/                       # 文档
```

## 🧪 运行测试

```bash
# 运行所有测试
dotnet test

# 运行物理引擎测试（验证精度）
dotnet test tests/PhysicsX.Core.Tests

# 生成测试覆盖率报告
dotnet test --collect:"XPlat Code Coverage"
```

## 🎯 路线图

### ✅ Alpha 阶段（当前）
- [x] 项目结构搭建
- [ ] OpenGL 渲染画板
- [ ] 基础力学仿真（刚体、碰撞）

### 🚧 Beta 阶段（未来 3 个月）
- [ ] 电学电路模拟
- [ ] AI 图像识别集成
- [ ] 视频导出功能
- [ ] 手绘形状识别

### 🎉 正式版 1.0（未来 6 个月）
- [ ] 电磁场可视化
- [ ] 完整的用户文档
- [ ] 社区题库分享平台

## 🤝 贡献指南

我们欢迎各种形式的贡献！

- 🐛 **报告 Bug**：[提交 Issue](https://github.com/yourusername/PhysicsX/issues/new?template=bug_report.md)
- ✨ **功能建议**：[提交 Issue](https://github.com/yourusername/PhysicsX/issues/new?template=feature_request.md)
- 🔧 **代码贡献**：Fork → 开发 → Pull Request

详细贡献流程请参阅 [CONTRIBUTING.md](CONTRIBUTING.md)

## 📜 开源协议

本项目采用 [GPL-3.0](LICENSE) 开源协议。

## 🌟 致谢

- [Avalonia UI](https://avaloniaui.net/) - 跨平台 UI 框架
- [Silk.NET](https://github.com/dotnet/Silk.NET) - .NET OpenGL 绑定
- [MathNet.Numerics](https://numerics.mathdotnet.com/) - 数学计算库
- [OpenAI](https://openai.com/) - AI 视觉识别服务

## 📧 联系方式

- **项目主页**：https://github.com/yourusername/PhysicsX
- **问题反馈**：https://github.com/yourusername/PhysicsX/issues
- **讨论区**：https://github.com/yourusername/PhysicsX/discussions

---

<div align="center">

**⭐ 如果这个项目对你有帮助，请给我们一个 Star！⭐**

Made with ❤️ by PhysicsX Team

</div>
