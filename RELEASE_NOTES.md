# Rxbit.BrigadeApp 1.5.0 发布说明

**发布日期**: 2026-11-06  
**产品名称**: Rxbit.BrigadeApp（Backsense BS-9200X Test Tool）  
**版本**: V1.5.0  
**安装包**: `Rxbit.BrigadeApp-1.5-x86.msi`  
**架构**: x86（32 位），可在 32/64 位 Windows 上运行  
**运行环境**: .NET Framework 4.8 / Windows 10、Windows 11

---

## 一、新增功能

### 1. VIEW → Screen Recording（屏幕录制）

- 入口：主窗口 **View → Screen Recording**
- 录制范围：**整个屏幕**（主显示器）或 **本软件窗口**
- 帧率：5 / 10 / 15 fps（默认 10 fps）
- 输出：`<Recording Path>\Media\Screen_yyyyMMdd_HHmmss.avi`
- 格式：AVI 容器 + MJPEG 编码，内置编码器，无需安装编解码器或驱动
- 最大输出宽度 1280 像素（超出时等比缩小）
- 窗口内显示录制时长、已写帧数、状态提示，可一键打开输出目录

### 2. VIEW → USB Camera Recording（USB 摄像头录制）

- 入口：主窗口 **View → USB Camera Recording**
- 使用 Media Foundation 枚举与采集 USB 摄像头，UVC 免驱摄像头可直接使用
- 选中摄像头后自动预览；可手动 Refresh 重新枚举、Stop Preview 释放设备
- 帧率：5 / 10 / 15 fps
- 输出：`<Recording Path>\Media\Camera_<设备名>_yyyyMMdd_HHmmss.avi`
- 格式：AVI(MJPEG)，无音频
- 首帧看门狗：预览启动后 5 秒内未收到画面会自动停止并提示，避免采集线程无限阻塞
- **实机确认**：设备枚举 → 预览 → 开始/停止录制 → `Camera_*.avi` 文件保存整条链路已由用户实机确认可用（V1.5.0）
- **已知限制**：开发自测所用摄像头在驱动层不出帧，故保留上述 5 秒首帧超时保护；更换摄像头或排除占用/隐私权限后可正常使用

### 3. VIEW → Video Playback（视频回放）

- 入口：主窗口 **View → Video Playback**
- 打开软件录制的 AVI(MJPEG) 文件，显示首帧、分辨率、帧率与总帧数
- 支持播放、暂停、停止、进度拖动
- 默认打开 `<Recording Path>\Media`
- 内置 AVI(MJPEG) 读取器，离线可用，不需要 CAN 设备或传感器

> 三个新功能统一输出到 `RecondingPath\Media`（默认 `C:\Data\Media`）。Recording Path 可在 **Devices → Path Configuration → Recording Path** 修改。

---

## 二、问题修复

| 序号 | 修复项 | 说明 |
|:---:|--------|------|
| 1 | 主菜单与二级菜单未对齐 | 统一主菜单与下拉子菜单的左侧对齐、间距与分隔线样式 |
| 2 | 启动即退出 | 修复程序启动后立即退出的缺陷，主窗口可正常显示并进入 CAN 配置流程 |
| 3 | CAN Live Data 表格样式 | 由白色背景改为与主界面一致的深色主题，提升长时间观察的可读性 |
| 4 | Path Configuration 拼写 | 修正界面中的路径配置文字拼写 |
| 5 | View 菜单项被错误置灰 | View 下的数据窗口与录制/回放入口不再依赖传感器在线状态，启动后即可使用 |
| 6 | Asc to Brigade 窗口标题 | 修正 Asc to Brigade 工具窗口的标题显示 |
| 7 | 菜单结构优化 | 原 Configuration 菜单调整为 **Devices**，二级菜单与功能入口重新整理 |
| 8 | View 下拉菜单浅色块 | 子菜单分隔线改用透明模板（`Resources/Styles.xaml`、`MainWindow.xaml`），展开后不再出现白色矩形块 |

---

## 三、安装与卸载

### 安装

```powershell
msiexec /i "Rxbit.BrigadeApp-1.5-x86.msi" /qn /norestart
```

- 安装范围为当前用户，默认目录 `%LOCALAPPDATA%\Brigade\Rxbit.BrigadeApp`
- 创建开始菜单快捷方式与桌面快捷方式
- 安装包不包含任何需要重启的驱动或系统组件；CAN 设备驱动需按厂商说明单独安装

### 卸载

```powershell
msiexec /x {ProductCode} /qn /norestart
```

`ProductCode` 见安装包属性或本文件下方的“版本信息”表。也可通过 **设置 → 应用 → 已安装的应用 → Rxbit.BrigadeApp** 卸载。

### 安装包内容

- 主程序 `Rxbit.BrigadeApp.exe` 及全部 .NET 依赖 DLL
- CAN 厂商原生库：`gcan`、`gcanfd`、`peakcan`、`zlgcan`、`cxkjcan`
- 刷写资源：`RadarEcu`、`sub-exe`
- `BrigadeSdk.zip`（PCAN 驱动缺失时的备用 SDK 包）
- 不打包 `Logs`（程序启动时自动创建）、`docs`（当前 Help → Docs 打开关于页）
- `SQLite.Interop.dll` 未被当前主程序引用，且不在 Release 输出中，故未纳入安装包

### 用户数据

卸载不会删除以下数据：

- `%APPDATA%\Brigade\BrigadeSetting.config`
- `C:\Data\Media`（或自定义 Recording Path 下的录制文件）
- 安装目录下的 `Logs`（程序运行时生成，卸载时保留，便于排查问题；如需彻底清理可手动删除 `%LOCALAPPDATA%\Brigade\Rxbit.BrigadeApp`）

### 验证结果（2026-11-06，Windows 10/11 x64）

| 验证项 | 结论 |
|--------|------|
| 静默安装 `msiexec /i ... /qn /norestart` | 通过，退出码 0；安装 147 个文件，开始菜单与桌面快捷方式均创建 |
| 程序启动与主窗口 | 通过，主窗口 `Backsense BS-9200X Test Tool` 正常显示 |
| 启动时的 CAN Tools 模态窗 | 正常出现；关闭后主窗口保持运行，程序不退出 |
| VIEW 菜单入口 | 通过：Detection & Sensor Info、Network Summary、CAN Live Data、Screen Recording、USB Camera Recording、Video Playback 六项均存在且可用 |
| VIEW 菜单展开外观 | 通过：深色背景，分隔线为细线，无浅色矩形块 |
| 屏幕录制 | 通过：生成 `C:\Data\Media\Screen_<时间戳>.avi`（约 3.9 MB） |
| USB 摄像头录制 | 由用户实机确认：枚举、预览、开始/停止录制、`Camera_*.avi` 保存均可用（本机自测摄像头驱动不出帧，仅验证到 5 秒首帧超时提示） |
| 静默卸载 `msiexec /x ... /qn /norestart` | 通过，退出码 0；安装文件、快捷方式、ARP 条目全部移除 |
| 卸载后数据 | 通过：`%APPDATA%\Brigade\BrigadeSetting.config` 与 `C:\Data\Media` 未被删除；安装目录仅残留程序运行日志 `Logs` |

> ProductCode 在每次重新构建时会变化，卸载请使用当前安装包对应的 ProductCode 或通过“设置 → 应用”卸载。

---

## 四、已知限制

1. 屏幕录制仅支持主显示器；选择“本软件窗口”时按窗口区域抓屏，窗口被其他窗口遮挡会影响画面。
2. 视频回放仅支持 AVI(MJPEG)；其他编码（如 H.264）的 AVI 可能无法在软件内播放，可用 VLC 等播放器打开。
3. 屏幕录制与摄像头录制均不包含音频。
4. 同一 USB 摄像头通常只能被一个程序占用；被其他程序占用时会触发首帧超时提示。
5. 默认录制目录为 `C:\Data`；若当前用户无写权限，请在 Path Configuration 中改为可写目录。
6. 仓库中的 `Brigade软件使用文档.docx` 为历史版本，本次未更新；请以 `使用说明文档.md` 为准。

---

## 五、文档

| 文档 | 说明 |
|------|------|
| `使用说明文档.md` | 中文用户手册，已更新安装卸载、菜单、CAN/传感器配置、三大数据窗口、录制与回放 |
| `Backsense-BS-9200X-Test-Tool-User-Guide.md` | 英文核心流程指南，已同步安装包名称与卸载方式 |
| `README.md` | 开发与打包说明，已更新 x86 构建、MSI 打包与数据目录 |
| `Brigade软件使用文档.docx` | 历史版本，保留未改 |

---

## 六、版本信息

| 项目 | 值 |
|------|----|
| ProductName | Rxbit.BrigadeApp |
| ProductVersion | 1.5.0 |
| Manufacturer | Brigade |
| UpgradeCode | `{6F3D8E2A-9C41-4B7E-8D15-2A5F7C9E1B34}`（固定，用于升级检测） |
| ProductCode | `{47100472-2F92-4FAA-8B2E-E59E7CFA7215}`（本次构建；每次重新打包会变化） |
| 安装包 | `dist\Rxbit.BrigadeApp-1.5-x86.msi` |
| 安装范围 | per-user（当前用户） |
| 平台 | x86 |

> 打包工程源码：`installer/Product.wxs`、`installer/build-msi.ps1`（WiX 6.0.2，x86 MSI）。
