# Rxbit.BrigadeApp (Backsense BS-9200X Test Tool)

基于 .NET Framework 4.8 的雷达传感器测试工具，用于雷达目标检测、CAN 总线通信和 ECU 刷写等功能。

## 项目结构

```
Rxbit.BrigadeApp/
├── Rxbit.BrigadeApp/              # 主应用程序 (WPF WinExe)
│   ├── Controls/                  # 自定义控件
│   ├── Converters/                # 数据转换器
│   ├── Models/                    # 应用模型
│   ├── Resources/                 # 资源文件 (样式、图标等)
│   ├── Services/                  # 应用服务
│   │   └── Media/                 # 录制/回放（GDI 抓屏、Media Foundation 摄像头、AVI MJPEG）
│   ├── ViewModels/                # 视图模型
│   └── Views/                     # 视图
├── BrigadeTool.CanDevices/        # CAN 设备通信库 (Library)
│   ├── Config/                    # 配置类
│   ├── IServices/                 # 服务接口
│   ├── Parsers/                   # CAN 消息解析器
│   ├── Services/                  # 通道服务实现
│   └── Structs/                   # 数据结构
├── BrigadeTool.Models/            # 数据模型和工具库 (Library)
│   ├── Attributes/                # 自定义特性
│   ├── Commands/                  # 命令模式实现
│   ├── EnumEntitys/               # 枚举定义
│   ├── Helper/                    # 辅助工具类
│   └── Models/                    # 领域模型
├── BrigadeTool.Services/          # 服务层库 (Library)
│   └── Services/                  # 数据中介服务
├── BrigadeTool.EcuServices/       # ECU 服务库 (Library)
│   ├── Models/                    # ECU 模型
│   ├── Services/                  # ECU 服务
│   ├── Utils/                     # ECU 工具
│   └── ZflashItems/               # 刷写项定义
├── installer/                     # MSI 打包工程与脚本
├── 使用说明文档.md                 # 中文用户手册
└── RELEASE_NOTES.md               # 版本发布说明
```

## 项目依赖关系

```
Rxbit.BrigadeApp (主程序)
    ├── BrigadeTool.CanDevices     # CAN 通信
    ├── BrigadeTool.Models         # 数据模型
    ├── BrigadeTool.Services       # 服务层
    └── BrigadeTool.EcuServices    # ECU 服务

BrigadeTool.EcuServices
    └── BrigadeTool.CanDevices     # CAN 通信依赖
```

## 功能特性

### 1. CAN 设备管理
- 支持多种 CAN 设备：ZLG USBCAN、PEAK CAN、GCAN、创芯科技 CAN
- CAN/CAN FD 模式切换
- 多通道配置和管理
- CAN 消息实时收发

### 2. 雷达目标显示
- 雷达扫描角度显示
- 目标实时跟踪
- 目标类型分类（车辆、行人、骑行者、障碍物）
- 目标运动方向指示

### 3. 传感器配置
- 传感器类型选择 (Gen1/Gen2)
- CAN ID 配置
- 传感器状态监控

### 4. ECU 刷写服务
- UDS 协议支持
- Flash 数据解析和下载
- CRC 校验计算
- 动态密钥生成

### 5. 鸟瞰视图
- 多传感器融合显示
- 目标位置可视化

### 6. 网络状态监控
- 节点状态监控
- CAN 消息实时数据流
- CAN Live Data 深色主题表格

### 7. 录制与回放（V1.5 新增）
- 屏幕录制（整个屏幕 / 本软件窗口）
- USB 摄像头录制（Media Foundation，UVC 免驱）
- 视频回放（内置 AVI MJPEG 读取器）
- 输出位置 `<Recording Path>\Media`，默认 `C:\Data\Media`

## 技术栈

- **框架**: .NET Framework 4.8
- **UI**: WPF (Windows Presentation Foundation)
- **MVVM**: CommunityToolkit.Mvvm 8.2.0
- **DI**: Microsoft.Extensions.DependencyInjection 7.0.0
- **JSON**: Newtonsoft.Json 13.0.4
- **构建**: MSBuild

## 支持的 CAN 设备

| 设备类型 | 驱动 | 说明 |
|---------|------|------|
| ZLG USBCAN | zlgcan.dll | 周立功 USBCAN I/II/FD 系列 |
| PEAK CAN | PCANBasic.dll | PEAK-System CAN 设备 |
| GCAN | ECanVci.dll | 广成科技 CAN 设备 |
| 创芯科技 | ControlCAN.dll | 创芯科技 CAN/CAN FD 设备 |

## 前置条件

1. **开发环境**: Visual Studio 2022 (支持 .NET Framework 4.8)
2. **系统要求**: Windows 10/11（32/64 位；程序与安装包均为 x86）
3. **CAN 驱动**: 根据使用的 CAN 设备安装对应驱动
4. **打包工具**: .NET SDK（用于按需安装 WiX 6 本地工具）

## 构建和运行

### 构建项目

```powershell
# 构建主程序（x86）
& "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" `
    "Rxbit.BrigadeApp\Rxbit.BrigadeApp.csproj" `
    /p:Configuration=Debug `
    /p:Platform=x86 `
    /t:Build

# 一条命令构建 Release 并生成 MSI（自动定位 VS / 按需安装 WiX）
powershell -ExecutionPolicy Bypass -File .\installer\build-msi.ps1
```

### 运行程序

1. 构建成功后，在 `Rxbit.BrigadeApp\bin\x86\Debug\` 目录下找到 `Rxbit.BrigadeApp.exe`
2. 运行程序，首先会弹出 CAN 设备配置窗口
3. 选择 CAN 设备类型、通道参数，点击连接
4. 连接成功后进入主界面

## 使用说明

### CAN 设备连接

1. 启动程序后，首先进入 CAN 设备配置界面
2. 选择 CAN 设备类型（如 PEAK CAN）
3. 设置通道参数（波特率、通道号等）
4. 点击"打开"按钮连接设备

### 雷达目标监控

1. 在主界面选择传感器类型
2. 连接 CAN 设备后，雷达数据会自动接收和解析
3. 在雷达显示窗口查看目标信息

### 传感器配置

1. 通过主菜单进入传感器配置界面
2. 配置传感器类型和 CAN ID
3. 保存配置到本地

## 配置文件

配置文件存储路径：
- **全局配置**: `%APPDATA%\Brigade\BrigadeSetting.config`
- **日志目录**: `程序运行目录\Logs\`
- **数据记录目录**: 默认 `C:\Data\`，可在 Devices → Path Configuration 中修改
- **录制视频**: `<数据记录目录>\Media\`（默认 `C:\Data\Media`）

## 项目配置

### NuGet 包管理

项目使用 packages.config 管理 NuGet 依赖，主要依赖包括：

- `CommunityToolkit.Mvvm` - MVVM 框架
- `Microsoft.Extensions.DependencyInjection` - 依赖注入
- `Newtonsoft.Json` - JSON 序列化
- `SkiaSharp` - 图形渲染
- `SharpDX` - DirectX 绑定

### 编译选项

- **目标框架**: .NET Framework 4.8
- **语言版本**: C# 8.0
- **平台目标**: x86（Release 输出目录 `Rxbit.BrigadeApp\bin\x86\Release`）

## MSI 打包

`installer/build-msi.ps1` 会自动完成：

1. 使用 MSBuild 构建 `Release|x86`
2. 将 Release 输出与运行时补充资源（`zlgcan`、`BrigadeSdk.zip`）汇总到 `installer/stage`
3. 调用 WiX 6（首次运行自动安装到 `.tools`）生成 x86 MSI
4. 产物：`dist\Rxbit.BrigadeApp-1.5-x86.msi`

```powershell
powershell -ExecutionPolicy Bypass -File .\installer\build-msi.ps1
```

安装/卸载验证命令见 [使用说明文档.md](使用说明文档.md) 第 0 章，版本变更见 [RELEASE_NOTES.md](RELEASE_NOTES.md)。

## 许可证

Copyright © HP 2026
