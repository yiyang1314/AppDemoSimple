# Wpf.CustomControl

Wpf.CustomControl 是一个基于 .NET Framework 4.8 的 WPF 自定义控件类库，提供 MVVM 基础设施、自定义对话框、矢量图标和绘图控件等功能。

## 模块结构

```
Wpf.CustomControl/
├── Mvvm/              # MVVM 基础设施
│   ├── ObservableObject.cs    # 属性通知基类
│   ├── RelayCommand.cs        # 同步命令
│   └── AsyncCommand.cs        # 异步命令
├── Dialogs/           # 自定义对话框
│   ├── DialogResources.xaml   # 对话框共享资源
│   ├── ConfirmDialog.xaml(.cs)# 确认对话框
│   ├── InputDialog.xaml(.cs)  # 输入对话框
│   ├── LoadingDialog.xaml(.cs)# 加载对话框
│   ├── ProgressDialog.xaml(.cs)# 进度对话框
│   ├── TipDialog.xaml(.cs)    # 提示对话框
│   └── WarningDialog.xaml(.cs)# 警告对话框
├── Drawing/           # 绘图控件
│   ├── DrawingElement.cs      # 绘制元素基类
│   ├── DrawingCanvas.cs       # 绘制容器
│   ├── RectangleElement.cs    # 矩形元素
│   ├── ArrowLineElement.cs    # 箭头直线元素
│   ├── PolygonElement.cs      # 多边形元素
│   ├── EllipseElement.cs      # 椭圆元素
│   ├── ArcElement.cs          # 圆弧元素
│   └── TextElement.cs         # 文本元素
├── Icons/             # 矢量图标
│   ├── IconKind.cs            # 图标类型枚举
│   ├── IconPaths.cs           # 图标路径数据
│   ├── IconResources.xaml     # 图标资源字典
│   └── IconElement.cs         # 图标显示控件
├── Converters/        # 值转换器
│   ├── NullToDashConverter.cs       # null 转 '-'
│   ├── ZeroToBoolConverter.cs       # 0 转 bool
│   ├── BoolToVisibilityConverter.cs # bool 转 Visibility
│   ├── NullToVisibilityConverter.cs # null 转 Visibility
│   └── EnumToColorConverter.cs      # 枚举状态转颜色
├── Behaviors/         # 附加行为
│   └── DataGridAutoScrollBehavior.cs # DataGrid 自动滚动
├── Utilities/         # 工具类
│   ├── XmlHelper.cs     # XML 序列化/反序列化
│   ├── FileHelper.cs    # 文件操作工具
│   ├── JsonHelper.cs    # JSON 序列化/反序列化
│   ├── ConfigHelper.cs  # INI 风格配置文件
│   ├── ZipHelper.cs     # ZIP 压缩/解压
│   ├── ImageHelper.cs   # 图像处理工具
│   ├── HttpRestApiHelper.cs   # HTTP REST API 调用
│   └── FileDownloadHelper.cs # 文件下载（支持断点续传）
└── Themes/
    └── Generic.xaml           # 控件默认样式
```

## 使用前准备

### 1. 添加项目引用

在 WPF 项目中添加对 `Wpf.CustomControl` 的引用。

### 2. 添加资源字典

在 `App.xaml` 中添加必要的资源字典：

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <!-- 对话框资源 -->
            <ResourceDictionary Source="pack://application:,,,/Wpf.CustomControl;component/Dialogs/DialogResources.xaml" />
            <!-- 图标资源 -->
            <ResourceDictionary Source="pack://application:,,,/Wpf.CustomControl;component/Icons/IconResources.xaml" />
            <!-- 主题样式 -->
            <ResourceDictionary Source="pack://application:,,,/Wpf.CustomControl;component/Themes/Generic.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### 3. 添加命名空间

```xml
xmlns:mvvm="clr-namespace:Wpf.CustomControl.Mvvm;assembly=Wpf.CustomControl"
xmlns:dialogs="clr-namespace:Wpf.CustomControl.Dialogs;assembly=Wpf.CustomControl"
xmlns:drawing="clr-namespace:Wpf.CustomControl.Drawing;assembly=Wpf.CustomControl"
xmlns:icons="clr-namespace:Wpf.CustomControl.Icons;assembly=Wpf.CustomControl"
xmlns:converters="clr-namespace:Wpf.CustomControl.Converters;assembly=Wpf.CustomControl"
xmlns:behaviors="clr-namespace:Wpf.CustomControl.Behaviors;assembly=Wpf.CustomControl"
```

> **Utilities 模块** 是纯 C# 静态类/普通类，无需 XAML 命名空间，在 C# 代码中通过 `using Wpf.CustomControl.Utilities;` 引入即可。

### 4. 新增程序集引用

Utilities 模块需要以下程序集引用（项目文件已配置）：
- `System.IO.Compression` - ZIP 压缩
- `System.IO.Compression.FileSystem` - ZIP 文件系统
- `System.Runtime.Serialization` - JSON 序列化
- `System.Drawing` - 图像处理（Bitmap、Icon）
- `System.Net.Http` - HTTP REST API 调用

---

## 1. MVVM 模块

### 1.1 ObservableObject

属性通知基类，实现 `INotifyPropertyChanged` 接口。

**使用示例：**

```csharp
public class MainViewModel : ObservableObject
{
    private string _title;
    public string Title
    {
        get { return _title; }
        set { SetProperty(ref _title, value); }
    }
}
```

**方法：**

| 方法 | 说明 |
|------|------|
| `OnPropertyChanged(string)` | 触发属性变更事件 |
| `SetProperty<T>(ref T, T)` | 设置属性值并触发变更通知 |
| `SetProperty<T>(ref T, T, string[])` | 设置属性值并触发关联属性变更通知 |

### 1.2 RelayCommand

同步命令实现，支持 `CanExecute` 判断。

**使用示例：**

```csharp
public class MainViewModel : ObservableObject
{
    private ICommand _saveCommand;
    public ICommand SaveCommand
    {
        get
        {
            return _saveCommand ?? (_saveCommand = new RelayCommand(
                () => { /* 执行保存逻辑 */ },
                () => { return !string.IsNullOrEmpty(Content); }
            ));
        }
    }
}
```

**泛型版本（带参数）：**

```csharp
var command = new RelayCommand<string>(
    param => { /* 使用参数 */ },
    param => { return param != null; }
);
```

### 1.3 AsyncCommand

异步命令实现，基于 `Task`，支持 `IsExecuting` 状态绑定和异常捕获。

**使用示例：**

```csharp
public class MainViewModel : ObservableObject
{
    private AsyncCommand _loadCommand;
    public AsyncCommand LoadCommand
    {
        get
        {
            return _loadCommand ?? (_loadCommand = new AsyncCommand(
                async () =>
                {
                    await Task.Delay(1000);
                    // 异步加载数据
                },
                () => { return true; }
            ));
        }
    }
}
```

**属性：**

| 属性 | 类型 | 说明 |
|------|------|------|
| `IsExecuting` | `bool` | 是否正在执行 |
| `Exception` | `Exception` | 执行过程中捕获的异常 |

**事件：**

| 事件 | 说明 |
|------|------|
| `ErrorOccurred` | 执行异常时触发 |

---

## 2. Dialogs 模块

提供多种自定义对话框，支持静态方法快速调用。

### 2.1 ConfirmDialog（确认对话框）

显示标题、内容和确认/取消按钮。

**使用示例：**

```csharp
bool? result = ConfirmDialog.Show("确认删除", "确定要删除这条记录吗？");
if (result == true)
{
    // 用户确认
}
```

**方法：**

| 方法 | 参数 | 说明 |
|------|------|------|
| `Show(string content)` | content: 内容 | 标题默认为"确认" |
| `Show(string title, string content)` | title: 标题, content: 内容 | 按钮文字默认为"确定"/"取消" |
| `Show(string title, string content, string confirmText, string cancelText)` | 自定义按钮文字 | 完整参数 |

### 2.2 InputDialog（输入对话框）

获取用户输入文本。

**使用示例：**

```csharp
string result = InputDialog.Show("请输入名称", "名称：", "默认值");
if (!string.IsNullOrEmpty(result))
{
    // 使用输入值
}
```

### 2.3 LoadingDialog（加载对话框）

显示加载状态，支持文字和动画。

**使用示例：**

```csharp
LoadingDialog.ShowLoading("正在加载...");
// 执行耗时操作
LoadingDialog.CloseLoading();
```

### 2.4 ProgressDialog（进度对话框）

显示进度条，支持进度更新。

**使用示例：**

```csharp
var dialog = ProgressDialog.Show("处理中", "请稍候...", 0, 100);
for (int i = 0; i <= 100; i++)
{
    dialog.UpdateProgress(i);
    await Task.Delay(50);
}
dialog.Close();
```

### 2.5 TipDialog（提示对话框）

显示提示信息，自动关闭。

**使用示例：**

```csharp
TipDialog.Show("操作成功！");
TipDialog.Show("操作成功！", 2000); // 2秒后自动关闭
```

### 2.6 WarningDialog（警告对话框）

显示警告信息。

**使用示例：**

```csharp
WarningDialog.Show("警告", "数据格式不正确");
```

---

## 3. Drawing 模块

基于 `DrawingVisual` 的高效绘图控件，支持多种图形元素。

### 3.1 DrawingCanvas

绘制容器，继承 `Canvas`，用于管理 `DrawingElement` 子元素。

**使用示例：**

```xml
<drawing:DrawingCanvas x:Name="canvas" Width="400" Height="300" Background="White" />
```

```csharp
var rect = new RectangleElement
{
    X = 50,
    Y = 50,
    RectWidth = 100,
    RectHeight = 60,
    Fill = Brushes.Blue,
    Stroke = Brushes.Black,
    StrokeThickness = 2
};
canvas.AddElement(rect);
```

**方法：**

| 方法 | 说明 |
|------|------|
| `AddElement(DrawingElement)` | 添加绘制元素 |
| `RemoveElement(DrawingElement)` | 移除绘制元素 |
| `ClearElements()` | 清除所有元素 |

### 3.2 DrawingElement（基类）

绘制元素抽象基类，提供通用依赖属性。

**依赖属性：**

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Stroke` | `Brush` | Black | 描边画刷 |
| `StrokeThickness` | `double` | 1.0 | 描边粗细 |
| `Fill` | `Brush` | null | 填充画刷 |

### 3.3 图形元素

#### RectangleElement（矩形）

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `X` | `double` | 0 | 左上角 X 坐标 |
| `Y` | `double` | 0 | 左上角 Y 坐标 |
| `RectWidth` | `double` | 50 | 宽度 |
| `RectHeight` | `double` | 50 | 高度 |
| `RadiusX` | `double` | 0 | 圆角 X 半径 |
| `RadiusY` | `double` | 0 | 圆角 Y 半径 |

#### ArrowLineElement（箭头直线）

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `StartX` | `double` | 0 | 起点 X 坐标 |
| `StartY` | `double` | 0 | 起点 Y 坐标 |
| `EndX` | `double` | 100 | 终点 X 坐标 |
| `EndY` | `double` | 100 | 终点 Y 坐标 |
| `ArrowSize` | `double` | 8 | 箭头大小 |

#### EllipseElement（椭圆）

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `X` | `double` | 0 | 圆心 X 坐标 |
| `Y` | `double` | 0 | 圆心 Y 坐标 |
| `RadiusX` | `double` | 50 | X 半径 |
| `RadiusY` | `double` | 50 | Y 半径 |

#### ArcElement（圆弧）

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `X` | `double` | 0 | 圆心 X 坐标 |
| `Y` | `double` | 0 | 圆心 Y 坐标 |
| `Radius` | `double` | 50 | 半径 |
| `StartAngle` | `double` | 0 | 起始角度（度） |
| `EndAngle` | `double` | 90 | 终止角度（度） |

#### PolygonElement（多边形）

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `Points` | `PointCollection` | null | 顶点集合 |

#### TextElement（文本）

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `X` | `double` | 0 | 文本位置 X |
| `Y` | `double` | 0 | 文本位置 Y |
| `Text` | `string` | null | 文本内容 |
| `FontSize` | `double` | 12 | 字体大小 |
| `FontFamily` | `FontFamily` | Segoe UI | 字体 |

---

## 4. Icons 模块

内置 30 个矢量图标，支持通过 `IconElement` 控件或 `IconResources.xaml` 资源使用。

### 4.1 IconElement 控件

矢量图标显示控件，通过 `IconKind` 选择内置图标。

**使用示例：**

```xml
<icons:IconElement IconKind="Save" Width="24" Height="24" Foreground="Blue" />
<icons:IconElement IconKind="Search" Width="32" Height="32" Foreground="Gray" Stretch="Uniform" />
```

**依赖属性：**

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `IconKind` | `IconKind` | Play | 图标类型 |
| `Stretch` | `Stretch` | Uniform | 拉伸方式 |

### 4.2 IconKind 枚举

| 枚举值 | 说明 | 资源 Key |
|--------|------|----------|
| `Pause` | 暂停 | PauseIcon |
| `Record` | 录制（红色圆点） | RecordIcon |
| `StopRecord` | 停止录制 | StopRecordIcon |
| `Receive` | 接收（下载箭头） | ReceiveIcon |
| `StopReceive` | 停止接收 | StopReceiveIcon |
| `Device` | 设备（USB） | DeviceIcon |
| `Play` | 播放 | PlayIcon |
| `Stop` | 停止 | StopIcon |
| `Connect` | 连接（插头） | ConnectIcon |
| `Disconnect` | 断开 | DisconnectIcon |
| `Settings` | 设置（齿轮） | SettingsIcon |
| `Refresh` | 刷新 | RefreshIcon |
| `Power` | 电源 | PowerIcon |
| `Send` | 发送（上传箭头） | SendIcon |
| `StopSend` | 停止发送 | StopSendIcon |
| `Picture` | 图片 | PictureIcon |
| `Photo` | 照片 | PhotoIcon |
| `Camera` | 相机 | CameraIcon |
| `Gallery` | 图库/相册 | GalleryIcon |
| `ImagePlus` | 添加图片 | ImagePlusIcon |
| `ImageEdit` | 编辑图片 | ImageEditIcon |
| `ImageView` | 查看图片 | ImageViewIcon |
| `Save` | 保存 | SaveIcon |
| `SaveAs` | 另存为 | SaveAsIcon |
| `Recording` | 录制中 | RecordingIcon |
| `Copy` | 复制 | CopyIcon |
| `Paste` | 粘贴 | PasteIcon |
| `Search` | 搜索（放大镜） | SearchIcon |
| `Config` | 配置（齿轮） | ConfigIcon |

### 4.3 程序化使用图标

```csharp
// 获取图标的 Geometry
Geometry geometry = IconPaths.GetGeometry(IconKind.Save);

// 获取图标的路径字符串
string path = IconPaths.GetPathData(IconKind.Search);
```

### 4.4 通过 Image 控件使用

```xml
<Image Source="{StaticResource SaveIcon}" Width="24" Height="24" />
<Image Source="{StaticResource CameraIcon}" Width="24" Height="24" />
```

---

## 5. Converters 模块

提供常用值转换器，简化 XAML 数据绑定时的值转换。

### 5.1 NullToDashConverter（null 转 '-'）

当值为 null、空字符串或 DBNull 时返回 "-"，否则返回原值字符串。

**使用示例：**

```xml
<!-- 资源定义 -->
<converters:NullToDashConverter x:Key="NullToDashConverter" />

<!-- 基本用法：null 显示为 '-' -->
<TextBlock Text="{Binding Description, Converter={StaticResource NullToDashConverter}}" />

<!-- 自定义空值显示文本（通过 ConverterParameter） -->
<TextBlock Text="{Binding Value, Converter={StaticResource NullToDashConverter}, ConverterParameter=N/A}" />
```

### 5.2 ZeroToBoolConverter（0 转 bool）

数值为 0 时返回 `false`，非 0 时返回 `true`，支持反向转换。可通过 `ConverterParameter=Invert` 反转结果。

**使用示例：**

```xml
<converters:ZeroToBoolConverter x:Key="ZeroToBoolConverter" />

<!-- 数量为 0 时按钮禁用 -->
<Button IsEnabled="{Binding Count, Converter={StaticResource ZeroToBoolConverter}}" />

<!-- 反转：数量为 0 时按钮启用 -->
<Button IsEnabled="{Binding Count, Converter={StaticResource ZeroToBoolConverter}, ConverterParameter=Invert}" />
```

### 5.3 BoolToVisibilityConverter（bool 转 Visibility）

`true` 显示，`false` 隐藏（默认 `Collapsed`）。

**ConverterParameter 可选值：**
- `Hidden`：隐藏方式使用 `Visibility.Hidden`（保留布局空间）
- `Collapse`（默认）：隐藏方式使用 `Visibility.Collapsed`
- `Invert`：反转结果（`true` 隐藏，`false` 显示）

可组合使用，如 `Hidden,Invert`。

**使用示例：**

```xml
<converters:BoolToVisibilityConverter x:Key="BoolToVisConverter" />

<!-- IsLoading 为 true 时显示 -->
<Grid Visibility="{Binding IsLoading, Converter={StaticResource BoolToVisConverter}}" />

<!-- Invert：IsLoading 为 false 时显示 -->
<Grid Visibility="{Binding IsLoading, Converter={StaticResource BoolToVisConverter}, ConverterParameter=Invert}" />

<!-- Hidden：隐藏时保留布局空间 -->
<Grid Visibility="{Binding IsVisible, Converter={StaticResource BoolToVisConverter}, ConverterParameter=Hidden}" />
```

### 5.4 NullToVisibilityConverter（null 转 Visibility）

非 null 显示，null 隐藏（默认 `Collapsed`）。支持 `Invert`、`Hidden`、`EmptyString` 参数。

**使用示例：**

```xml
<converters:NullToVisibilityConverter x:Key="NullToVisConverter" />

<!-- 数据非空时显示 -->
<Grid Visibility="{Binding Description, Converter={StaticResource NullToVisConverter}}" />

<!-- Invert：数据为 null 时显示占位提示 -->
<TextBlock Text="暂无数据" 
           Visibility="{Binding Description, Converter={StaticResource NullToVisConverter}, ConverterParameter=Invert}" />

<!-- EmptyString：空字符串也视为 null -->
<Grid Visibility="{Binding Name, Converter={StaticResource NullToVisConverter}, ConverterParameter=EmptyString}" />
```

### 5.5 EnumToColorConverter（枚举状态转颜色）

根据枚举值（或其字符串形式）返回对应的颜色画刷，常用于状态指示器。

**两种使用方式：**

**方式 1：使用内置默认映射**

内置常见状态的默认颜色映射：
| 状态值 | 颜色 |
|--------|------|
| Running / Active / Online / Connected / Start / Success / Ok | Green |
| Stopped / Stop / Idle / Offline / Disconnected / Disabled / None | Gray |
| Error / Failed / Fault / Fail | Red |
| Warning / Pending / Waiting / Pause / Paused | Orange |
| Unknown | DarkGray |
| Default / 其他 | Black |

```xml
<converters:EnumToColorConverter x:Key="EnumToColorConverter" />

<!-- 根据设备状态自动着色 -->
<TextBlock Text="{Binding Status}" 
           Foreground="{Binding Status, Converter={StaticResource EnumToColorConverter}}" />
```

**方式 2：通过 ConverterParameter 自定义映射**

格式：`Key1=Color1|Key2=Color2|Default=ColorN`，颜色支持命名颜色（如 `Green`）或十六进制（如 `#FF0000`）。

```xml
<!-- 自定义映射 -->
<TextBlock Text="{Binding Status}"
           Foreground="{Binding Status, Converter={StaticResource EnumToColorConverter},
                       ConverterParameter=Running=#22C55E|Stopped=#9CA3AF|Error=#EF4444|Default=#000000}" />

<!-- 枚举值（枚举会自动转字符串） -->
public enum DeviceStatus
{
    Online,
    Offline,
    Fault
}
// Online -> Green, Offline -> Gray, Fault -> Red
```

**ViewModel 示例：**

```csharp
public class DeviceViewModel : ObservableObject
{
    private DeviceStatus _status;
    public DeviceStatus Status
    {
        get { return _status; }
        set { SetProperty(ref _status, value); }
    }
}
```

---

## 6. Behaviors 模块

提供附加行为，方便在 XAML 中直接使用而无需修改代码逻辑。

### 6.1 DataGridAutoScrollBehavior（DataGrid 自动滚动）

当 `ItemsSource` 新增项时自动滚动到末尾（或开头），常用于日志列表、实时数据列表。

**使用示例：**

```xml
<!-- 默认：自动滚动到末尾 -->
<DataGrid ItemsSource="{Binding LogList}"
          behaviors:DataGridAutoScrollBehavior.IsEnabled="True" />

<!-- 滚动到开头 -->
<DataGrid ItemsSource="{Binding LogList}"
          behaviors:DataGridAutoScrollBehavior.IsEnabled="True"
          behaviors:DataGridAutoScrollBehavior.ScrollDirection="First" />
```

**附加属性：**

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `IsEnabled` | `bool` | false | 是否启用自动滚动 |
| `ScrollDirection` | `ScrollDirectionKind` | Last | 滚动方向（Last/First） |

**ScrollDirectionKind 枚举：**

| 值 | 说明 |
|------|------|
| `Last` | 滚动到末尾（默认） |
| `First` | 滚动到开头 |

**ViewModel 示例：**

```csharp
public class LogViewModel : ObservableObject
{
    private ObservableCollection<string> _logList;
    public ObservableCollection<string> LogList
    {
        get { return _logList; }
        set { SetProperty(ref _logList, value); }
    }

    public LogViewModel()
    {
        LogList = new ObservableCollection<string>();
    }

    public void AddLog(string message)
    {
        // 添加新日志后 DataGrid 自动滚动到末尾
        LogList.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
    }
}
```

**说明：**
- 使用 `WeakReference` 保存 DataGrid 引用，避免内存泄漏
- 自动监听 `ItemsSource` 变更，切换数据源时自动重新订阅
- 使用 `Dispatcher.Background` 优先级滚动，确保布局更新完成后再滚动
- 集合必须实现 `INotifyCollectionChanged`（如 `ObservableCollection<T>`）

---

## 7. Utilities 模块

提供文件、配置、序列化、压缩等常用工具类。

### 7.1 XmlHelper（XML 序列化工具）

基于 `System.Xml.Serialization.XmlSerializer`，支持泛型类型的 XML 序列化和反序列化。

**API 一览：**

| 方法 | 说明 |
|------|------|
| `Serialize<T>(T obj, bool omitXmlDeclaration, bool indent)` | 序列化为 XML 字符串 |
| `Save<T>(T obj, string filePath, bool indent)` | 保存到 XML 文件 |
| `Deserialize<T>(string xml)` | 从 XML 字符串反序列化 |
| `Load<T>(string filePath)` | 从 XML 文件加载 |
| `TryLoad<T>(string filePath, out T result)` | 尝试从文件加载（不抛异常） |

**使用示例：**

```csharp
using Wpf.CustomControl.Utilities;

public class AppSettings
{
    public string UserName { get; set; }
    public int Port { get; set; }
    public bool AutoConnect { get; set; }
}

// 保存
var settings = new AppSettings { UserName = "admin", Port = 5025, AutoConnect = true };
XmlHelper.Save(settings, "settings.xml");

// 加载
AppSettings loaded = XmlHelper.Load<AppSettings>("settings.xml");

// 安全加载
if (XmlHelper.TryLoad<AppSettings>("settings.xml", out var result))
{
    // 加载成功
}
```

### 7.2 FileHelper（文件操作工具）

提供文件读写、复制、移动、删除、目录操作等功能。

**主要功能：**

| 类别 | 方法 | 说明 |
|------|------|------|
| 文本读写 | `ReadAllText` / `WriteAllText` / `AppendAllText` | 文本文件读写 |
| 行读写 | `ReadAllLines` / `WriteAllLines` | 按行读写 |
| 字节读写 | `ReadAllBytes` / `WriteAllBytes` | 二进制读写 |
| 文件操作 | `Copy` / `Move` / `Delete` / `Exists` | 文件操作 |
| 目录操作 | `EnsureDirectory` / `EnsureDirectoryExists` | 自动创建目录 |
| 目录操作 | `CopyDirectory` / `DeleteDirectory` / `ClearDirectory` | 目录复制/删除/清空 |
| 目录查询 | `GetFiles(string, string, bool recursive)` | 获取目录文件列表 |
| 信息查询 | `GetFileSize` / `GetDirectorySize` / `FormatFileSize` | 文件大小 |

**使用示例：**

```csharp
using Wpf.CustomControl.Utilities;

// 写入文本（自动创建目录）
FileHelper.WriteAllText("logs/app.log", "应用启动");

// 追加日志
FileHelper.AppendAllText("logs/app.log", "\n新日志内容");

// 复制目录（递归）
FileHelper.CopyDirectory("source", "backup", overwrite: true);

// 获取目录大小并格式化
long size = FileHelper.GetDirectorySize("data");
Console.WriteLine(FileHelper.FormatFileSize(size));  // 如 "1.23 MB"
```

### 7.3 JsonHelper（JSON 序列化工具）

基于 `System.Runtime.Serialization.Json.DataContractJsonSerializer`，无需第三方库。

> **注意**：类型需标记 `[DataContract]`，属性需标记 `[DataMember]`。

**使用示例：**

```csharp
using Wpf.CustomControl.Utilities;
using System.Runtime.Serialization;

[DataContract]
public class UserInfo
{
    [DataMember]
    public string Name { get; set; }

    [DataMember]
    public int Age { get; set; }

    [DataMember]
    public bool IsActive { get; set; }
}

// 序列化为 JSON 字符串
var user = new UserInfo { Name = "张三", Age = 30, IsActive = true };
string json = JsonHelper.Serialize(user);
// 输出: {"Age":30,"IsActive":true,"Name":"张三"}

// 反序列化
UserInfo parsed = JsonHelper.Deserialize<UserInfo>(json);

// 保存到文件
JsonHelper.Save(user, "user.json");

// 从文件加载
UserInfo loaded = JsonHelper.Load<UserInfo>("user.json");
```

### 7.4 ConfigHelper（配置文件工具）

INI 风格的键值对配置文件工具，支持分区、读写、类型转换。

**配置文件格式：**

```ini
[Database]
Server=localhost
Port=3306
Timeout=30

[UI]
Theme=Dark
Language=zh-CN
AutoSave=true

; 注释行以 ; 或 # 开头
```

**使用示例：**

```csharp
using Wpf.CustomControl.Utilities;

// 实例化并加载
var config = new ConfigHelper("app.config");
config.Load();

// 读取（支持类型转换）
string server = config.Get("Database", "Server", "localhost");
int port = config.GetInt("Database", "Port", 3306);
bool autoSave = config.GetBool("UI", "AutoSave", false);
double timeout = config.GetDouble("Database", "Timeout", 30.0);

// 写入
config.Set("Database", "Server", "192.168.1.100");
config.Set("UI", "Theme", "Blue");
config.Set("UI", "AutoSave", true);
config.Save();

// 静态便捷方法（一次性读写）
ConfigHelper.WriteValue("app.config", "UI", "Theme", "Dark");
string theme = ConfigHelper.ReadValue("app.config", "UI", "Theme", "Light");

// 查询
bool hasSection = config.ContainsSection("Database");
bool hasKey = config.ContainsKey("UI", "Theme");
IEnumerable<string> sections = config.GetSectionNames();
```

**读取 API：**

| 方法 | 说明 |
|------|------|
| `Get(section, key, defaultValue)` | 读取字符串 |
| `GetInt(section, key, defaultValue)` | 读取 int |
| `GetBool(section, key, defaultValue)` | 读取 bool（支持 true/false/1/0/yes/no） |
| `GetDouble(section, key, defaultValue)` | 读取 double |

### 7.5 ZipHelper（ZIP 压缩工具）

基于 `System.IO.Compression.ZipArchive`，支持文件、目录、多文件压缩，以及解压覆盖。

**主要功能：**

| 方法 | 说明 |
|------|------|
| `CompressFile(sourceFile, zipPath, entryName, overwrite)` | 压缩单个文件 |
| `CompressDirectory(sourceDir, zipPath, includeBaseDir, overwrite)` | 压缩整个目录 |
| `CompressFiles(sourceFiles, zipPath, overwrite)` | 压缩多个文件 |
| `ExtractToDirectory(zipPath, destDir, overwrite)` | 解压到目录（支持覆盖） |
| `ExtractFile(zipPath, entryName, destFile, overwrite)` | 解压单个条目 |
| `GetEntries(zipPath)` | 获取 ZIP 内所有条目名 |
| `ContainsEntry(zipPath, entryName)` | 检查是否包含条目 |

**使用示例：**

```csharp
using Wpf.CustomControl.Utilities;

// 压缩单个文件
ZipHelper.CompressFile("data.txt", "data.zip", overwrite: true);

// 压缩整个目录（不包含根目录名）
ZipHelper.CompressDirectory("logs", "logs.zip", includeBaseDir: false, overwrite: true);
// ZIP 内容：
//   2024-01.log
//   2024-02.log
//   sub/2024-03.log

// 压缩整个目录（包含根目录名）
ZipHelper.CompressDirectory("logs", "logs.zip", includeBaseDir: true);
// ZIP 内容：
//   logs/2024-01.log
//   logs/2024-02.log

// 压缩多个文件
ZipHelper.CompressFiles(new[] { "a.txt", "b.txt" }, "files.zip");

// 解压到目录（覆盖现有文件）
ZipHelper.ExtractToDirectory("data.zip", "extracted", overwrite: true);

// 解压单个文件
ZipHelper.ExtractFile("data.zip", "data.txt", "output.txt");

// 查询 ZIP 内容
List<string> entries = ZipHelper.GetEntries("data.zip");
bool hasFile = ZipHelper.ContainsEntry("data.zip", "data.txt");
```

**安全性说明：**
- 解压时自动检测路径穿越攻击（path traversal），跳过非法路径条目
- `overwrite` 参数控制是否覆盖已存在的目标文件
- 自动创建目标目录，无需手动 `EnsureDirectoryExists`

### 7.6 ImageHelper（图像处理工具）

提供 WPF 与 GDI+ 图像之间的转换、文件保存、PNG 转 ICO 等常用图像处理功能。

#### 主要功能

| 类别 | 方法 | 说明 |
|------|------|------|
| Buffer 转图像 | `BufferToImageSource(byte[], bool)` | 字节数组转 BitmapImage |
| Buffer 转图像 | `StreamToImageSource(Stream, bool)` | 流转 BitmapImage |
| Buffer 转图像 | `LoadFromFile(string, bool)` | 文件加载 BitmapImage |
| Bitmap 转图像 | `BitmapToImageSource(Bitmap, bool)` | GDI+ Bitmap 转 BitmapSource |
| Bitmap 转图像 | `BitmapToImageSourceByHBitmap(Bitmap, bool)` | 通过 HBitmap 转换（兼容性好） |
| Bitmap 转图像 | `ImageSourceToBitmap(ImageSource)` | ImageSource 转 GDI+ Bitmap |
| 文件保存 | `SaveAsPng(BitmapSource, string)` | 保存为 PNG |
| 文件保存 | `SaveAsBmp(BitmapSource, string)` | 保存为 BMP |
| 文件保存 | `SaveAsJpeg(BitmapSource, string, int)` | 保存为 JPEG（可指定质量） |
| 文件保存 | `SaveBitmapAsPng(Bitmap, string)` / `SaveBitmapAsBmp(Bitmap, string)` | GDI+ Bitmap 保存 |
| 文件保存 | `SaveAuto(BitmapSource, string)` | 根据扩展名自动选择编码器 |
| PNG 转 ICO | `PngToIco(string, string, int)` | PNG 文件转 ICO |
| PNG 转 ICO | `ImageSourceToIco(BitmapSource, string, int)` | ImageSource 转 ICO |
| PNG 转 ICO | `BitmapToIco(Bitmap, string)` | GDI+ Bitmap 转 ICO |
| PNG 转 ICO | `PngsToMultiSizeIco(string[], string)` | 多尺寸 PNG 合并为 ICO |

#### Buffer 转 ImageSource 示例

```csharp
using Wpf.CustomControl.Utilities;

// 字节数组转 ImageSource（常用于网络图片显示）
byte[] imageBuffer = DownloadImage();
BitmapImage image = ImageHelper.BufferToImageSource(imageBuffer);
MyImageControl.Source = image;  // 直接绑定到 WPF Image 控件

// 从文件加载（自动冻结，可跨线程使用）
BitmapImage logo = ImageHelper.LoadFromFile("logo.png");
```

#### Bitmap 转 ImageSource 示例

```csharp
using Wpf.CustomControl.Utilities;
using SD = System.Drawing;

// GDI+ Bitmap 转 WPF ImageSource
using (SD.Bitmap bitmap = new SD.Bitmap("photo.jpg"))
{
    BitmapSource source = ImageHelper.BitmapToImageSource(bitmap);
    PreviewImage.Source = source;
}

// ImageSource 转 GDI+ Bitmap（用于 GDI+ 处理）
BitmapSource wpfSource = (BitmapSource)PreviewImage.Source;
using (SD.Bitmap bitmap = ImageHelper.ImageSourceToBitmap(wpfSource))
{
    // 可使用 GDI+ 的图像处理功能
    bitmap.RotateFlip(SD.RotateFlipType.Rotate90FlipNone);
}
```

#### 文件保存示例

```csharp
using Wpf.CustomControl.Utilities;

// 渲染 UI 元素为图像并保存
RenderTargetBitmap render = new RenderTargetBitmap(
    (int)canvas.ActualWidth, (int)canvas.ActualHeight, 96, 96, PixelFormats.Pbgra32);
render.Render(canvas);

// 保存为 PNG（无损）
ImageHelper.SaveAsPng(render, "screenshot.png");

// 保存为 BMP
ImageHelper.SaveAsBmp(render, "screenshot.bmp");

// 保存为 JPEG（指定质量）
ImageHelper.SaveAsJpeg(render, "photo.jpg", qualityLevel: 85);

// 根据扩展名自动选择
ImageHelper.SaveAuto(render, "output.png");
ImageHelper.SaveAuto(render, "output.bmp");
ImageHelper.SaveAuto(render, "output.jpg");
```

#### PNG 转 ICO 示例

```csharp
using Wpf.CustomControl.Utilities;

// PNG 文件转 ICO（适用于应用图标）
ImageHelper.PngToIco("logo.png", "app.ico", size: 32);

// ImageSource 转 ICO
BitmapSource logoSource = ImageHelper.LoadFromFile("logo.png");
ImageHelper.ImageSourceToIco(logoSource, "app.ico");

// GDI+ Bitmap 转 ICO
using (SD.Bitmap bitmap = new SD.Bitmap("logo.png"))
{
    ImageHelper.BitmapToIco(bitmap, "app.ico");
}

// 多尺寸 PNG 合并为 ICO（适用于不同 DPI 场景）
ImageHelper.PngsToMultiSizeIco(
    new[] { "logo_16.png", "logo_32.png", "logo_48.png", "logo_256.png" },
    "multi_size.ico");
```

#### ICO 文件格式说明

`PngToIco` 方法采用 **PNG 嵌入** 方式生成 ICO 文件：

- **ICONDIR 头部**（6 字节）：标识 ICO 文件，记录图像数量
- **ICONDIRENTRY 条目**（每个 16 字节）：记录图像尺寸、色深、偏移
- **PNG 数据**：原始 PNG 字节直接嵌入

**优点**：
- 支持 32 位色深和 Alpha 透明通道
- 现代操作系统（Windows Vista+）原生支持
- 无需调色板计算，转换效率高

**ICO 自动尺寸识别**：
- 自动解析 PNG 文件头中的 IHDR 块获取真实尺寸
- 尺寸 ≥ 256 时按 ICO 规范用 `0` 表示

#### Freeze 冻结说明

大多数方法提供 `freeze` 参数（默认 `true`）：
- **冻结后**：可跨线程访问、性能更好、无法修改
- **不冻结**：可修改，但只能由创建线程访问

> 建议：UI 显示用图像建议冻结，避免跨线程异常。

### 7.7 HttpRestApiHelper（HTTP REST API 工具）

基于 `System.Net.Http.HttpClient` 的 REST API 调用工具，支持 GET/POST/PUT/DELETE/PATCH，内置 JSON 序列化、Bearer Token 认证、自定义请求头、超时控制。

#### 主要功能

| 类别 | 方法 | 说明 |
|------|------|------|
| 认证 | `SetBearerToken(token)` | 设置 Bearer Token 认证 |
| 认证 | `SetBasicAuth(user, pass)` | 设置 Basic 认证 |
| 认证 | `AddDefaultHeader(name, value)` | 添加默认请求头 |
| GET | `GetAsync(url, headers, token)` | GET 请求返回字符串 |
| GET | `GetAsync<T>(...)` | GET 请求反序列化为对象 |
| GET | `GetBytesAsync(...)` | GET 请求返回字节数组 |
| GET | `GetStreamAsync(...)` | GET 请求返回流（大文件流式） |
| POST | `PostAsync<TBody>(url, body, ...)` | POST JSON 返回字符串 |
| POST | `PostAsync<TBody, TResponse>(...)` | POST JSON 反序列化响应 |
| POST | `PostJsonAsync(url, json, ...)` | POST 原始 JSON 字符串 |
| POST | `PostFormAsync(url, formData, ...)` | POST 表单数据 |
| PUT | `PutAsync<TBody>(...)` / `PutAsync<TBody, TResponse>(...)` | PUT 请求 |
| DELETE | `DeleteAsync(...)` / `DeleteAsync<T>(...)` | DELETE 请求 |
| PATCH | `PatchAsync<TBody>(...)` | PATCH 请求 |
| 通用 | `SendAsync(method, url, content, ...)` | 通用请求方法 |
| 静态 | `HttpRestApiHelper.Get(url, ...)` | 静态 GET |
| 静态 | `HttpRestApiHelper.Post(url, body, ...)` | 静态 POST |

#### 实例化与认证示例

```csharp
using Wpf.CustomControl.Utilities;

// 实例化（指定 BaseUrl 和超时）
using (var api = new HttpRestApiHelper("http://api.example.com/", TimeSpan.FromSeconds(60)))
{
    // 设置 Bearer Token 认证
    api.SetBearerToken("eyJhbGciOiJIUzI1NiIs...");

    // 添加自定义请求头
    api.AddDefaultHeader("X-Api-Version", "2.0");
    api.AddDefaultHeader("Accept-Language", "zh-CN");

    // 后续所有请求都自动带上认证和默认请求头
    var data = await api.GetAsync("/users");
}
```

#### GET 请求示例

```csharp
// 1. GET 返回字符串
string json = await api.GetAsync("/users/1001");

// 2. GET 反序列化为对象
User user = await api.GetAsync<User>("/users/1001");

// 3. GET 带查询参数（手动拼接或使用字典）
string url = "/users?page=1&size=20&keyword=" + Uri.EscapeDataString("张三");
var users = await api.GetAsync<UserList>(url);

// 4. GET 返回二进制（如图片）
byte[] imageBytes = await api.GetBytesAsync("/avatars/1001.png");

// 5. GET 返回流（大文件流式处理）
using (Stream stream = await api.GetStreamAsync("/large-file.dat"))
using (FileStream fs = new FileStream("output.dat", FileMode.Create))
{
    await stream.CopyToAsync(fs);
}
```

#### POST / PUT / DELETE 示例

```csharp
// POST JSON
var newUser = new { Name = "张三", Age = 30, Email = "zhangsan@example.com" };
string response = await api.PostAsync("/users", newUser);

// POST JSON 并反序列化响应
var created = await api.PostAsync<object, User>("/users", newUser);
Console.WriteLine($"创建成功，ID = {created.Id}");

// POST 表单数据
var formData = new Dictionary<string, string>
{
    { "username", "admin" },
    { "password", "123456" }
};
string loginResult = await api.PostFormAsync("/login", formData);

// PUT 更新资源
var updateData = new { Name = "李四" };
await api.PutAsync("/users/1001", updateData);

// DELETE 删除资源
await api.DeleteAsync("/users/1001");

// PATCH 局部更新
var patch = new { Age = 31 };
await api.PatchAsync("/users/1001", patch);
```

#### 静态便捷方法示例

```csharp
// 适合简单一次性调用，自动释放资源
User user = await HttpRestApiHelper.Get<User>("http://api.example.com/users/1001");

var data = new { Name = "张三" };
User created = await HttpRestApiHelper.Post<object, User>(
    "http://api.example.com/users", data, timeout: TimeSpan.FromSeconds(10));
```

#### 错误处理

```csharp
try
{
    var user = await api.GetAsync<User>("/users/9999");
}
catch (HttpRequestException ex)
{
    // 非 2xx 状态码会抛出 HttpRequestException
    // ex.Message 包含状态码和响应内容，如 "HTTP 404 NotFound: ..."
    Console.WriteLine("请求失败: " + ex.Message);
}
```

### 7.8 FileDownloadHelper（文件下载工具）

支持进度回调、断点续传、取消下载，适用于 HTTP/HTTPS 大文件下载。

#### 主要功能

| 方法 | 说明 |
|------|------|
| `Download(url, savePath, overwrite)` | 同步下载（无进度） |
| `DownloadAsync(url, savePath, ...)` | 异步下载（无进度） |
| `DownloadAsync(url, savePath, overwrite, progress, token)` | 异步下载（带进度回调） |
| `DownloadWithResumeAsync(url, savePath, progress, token)` | 异步下载（支持断点续传） |
| `DownloadWithHeadersAsync(url, savePath, headers, ...)` | 带自定义请求头下载 |
| `DownloadBytesAsync(url, token)` | 下载到内存（字节数组） |

#### DownloadProgress 进度类

| 属性 | 说明 |
|------|------|
| `DownloadedBytes` | 已下载字节数 |
| `TotalBytes` | 总字节数（未知为 -1） |
| `Percent` | 百分比（0-100，未知为 -1） |
| `DownloadedSizeText` | 已下载大小文本（如 "1.23 MB"） |
| `TotalSizeText` | 总大小文本 |
| `IsCompleted` | 是否完成 |

#### 基础下载示例

```csharp
using Wpf.CustomControl.Utilities;

// 简单异步下载
await FileDownloadHelper.DownloadAsync(
    "http://example.com/files/data.zip",
    "downloads/data.zip");

// 同步下载
FileDownloadHelper.Download(
    "http://example.com/files/data.zip",
    "downloads/data.zip");
```

#### 带进度回调下载示例

```csharp
// 创建进度回调
var progress = new Progress<DownloadProgress>(p =>
{
    Console.WriteLine($"已下载: {p.DownloadedSizeText} / {p.TotalSizeText} ({p.Percent:F2}%)");
    if (p.IsCompleted)
    {
        Console.WriteLine("下载完成！");
    }
});

// 异步下载
await FileDownloadHelper.DownloadAsync(
    "http://example.com/files/large-file.zip",
    "downloads/large-file.zip",
    overwrite: true,
    progress: progress);
```

#### 断点续传示例

```csharp
// 断点续传：已存在的部分文件会自动追加
// 适合网络不稳定或大文件下载场景
await FileDownloadHelper.DownloadWithResumeAsync(
    "http://example.com/files/big-file.iso",
    "downloads/big-file.iso",
    progress: new Progress<DownloadProgress>(p =>
    {
        Dispatcher.Invoke(() => ProgressBar.Value = p.Percent);
    }));
```

**说明**：
- 断点续传要求服务器支持 `Range` 请求（返回 `206 Partial Content`）
- 若服务器不支持，自动改为从头下载
- 若下载被中断，下次调用同名文件会自动追加

#### 带请求头下载示例

```csharp
// 适用于需要 Authorization 的私有资源下载
var headers = new Dictionary<string, string>
{
    { "Authorization", "Bearer eyJhbGciOiJIUzI1NiIs..." },
    { "X-Request-Source", "Desktop-App" }
};

await FileDownloadHelper.DownloadWithHeadersAsync(
    "http://api.example.com/files/private-doc.pdf",
    "downloads/private-doc.pdf",
    headers: headers,
    overwrite: true,
    progress: new Progress<DownloadProgress>(p =>
    {
        Console.WriteLine(p.ToString());
    }));
```

#### 下载到内存示例

```csharp
// 适合小文件直接加载到内存
byte[] imageBytes = await FileDownloadHelper.DownloadBytesAsync(
    "http://example.com/images/logo.png");

// 直接转 ImageSource 显示
BitmapImage image = ImageHelper.BufferToImageSource(imageBytes);
PreviewImage.Source = image;
```

#### 取消下载示例

```csharp
// 使用 CancellationTokenSource 取消下载
var cts = new CancellationTokenSource();

// 在 UI 中注册取消按钮事件
CancelButton.Click += (s, e) => cts.Cancel();

try
{
    await FileDownloadHelper.DownloadAsync(
        "http://example.com/files/big.zip",
        "downloads/big.zip",
        overwrite: true,
        progress: progress,
        cancellationToken: cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("下载已取消");
    // 可选：删除部分文件
    FileHelper.Delete("downloads/big.zip");
}
```

#### WPF 中绑定进度到 UI

```csharp
public class DownloadViewModel : ObservableObject
{
    private double _progressPercent;
    public double ProgressPercent
    {
        get { return _progressPercent; }
        set { SetProperty(ref _progressPercent, value); }
    }

    private string _progressText;
    public string ProgressText
    {
        get { return _progressText; }
        set { SetProperty(ref _progressText, value); }
    }

    public AsyncCommand DownloadCommand { get; private set; }

    public DownloadViewModel()
    {
        DownloadCommand = new AsyncCommand(async () =>
        {
            var progress = new Progress<DownloadProgress>(p =>
            {
                ProgressPercent = p.Percent >= 0 ? p.Percent : 0;
                ProgressText = p.ToString();
            });

            await FileDownloadHelper.DownloadAsync(
                "http://example.com/files/data.zip",
                "downloads/data.zip",
                overwrite: true,
                progress: progress);
        });
    }
}
```

```xml
<!-- XAML -->
<StackPanel>
    <ProgressBar Value="{Binding ProgressPercent}" Maximum="100" Height="20" />
    <TextBlock Text="{Binding ProgressText}" />
    <Button Content="下载" Command="{Binding DownloadCommand}" />
</StackPanel>
```
### 7.9 实现完整的消息弹框通知系统
> 基于 Wpf.CustomControl 模块新增了 3 个文件 ，实现完整的消息弹框通知系统。

#### 1. NotificationBox.xaml — 通知窗口界面
美观的界面设计：

- 无边框透明窗口 ： AllowsTransparency=True 、 WindowStyle=None 、 ShowInTaskbar=False 、 ShowActivated=False （不抢焦点）
- 圆角阴影容器 ：复用 DialogResources.xaml 的深色主题（圆角 8px、阴影效果）
- 左侧彩色条 ：4px 宽，随通知类型变色
- 圆形图标 ：带彩色背景的图标徽章
- 标题 + 消息 ：双行布局，标题加粗、消息副色
- 关闭按钮 ：右上角圆形按钮，悬停高亮
- 倒计时进度条 ：底部 3px 进度条，可视化剩余时间 

#### 2. NotificationBox.xaml.cs — 通知窗口逻辑
包含两个枚举和通知窗口类：
- NotificationType ：Info / Success / Warning / Error
- NotificationPosition ：TopLeft / TopCenter / TopRight / BottomLeft / BottomCenter / BottomRight
- 淡入淡出动画 ： DoubleAnimation 实现 200ms 过渡
- 延时自动关闭 ： DispatcherTimer 主计时器 + 进度条计时器（60fps 刷新）
- 手动关闭 ：关闭按钮触发 CloseWithAnimation()
- 鼠标悬停暂停 ： MouseEnter / MouseLeave 暂停/恢复倒计时
- 类型配色 ：4 种类型对应蓝/绿/橙/红色 

#### 3. NotificationManager.cs — 通知管理器
静态管理器，提供便捷 API：

- 全局配置 ： DefaultPosition 、 DefaultDurationMs 、 Spacing 、 Margin
- 快捷方法 ： Info() / Success() / Warning() / Error()
- 自定义调用 ： Show(message, type, durationMs, position, title)
- 位置计算 ：基于 SystemParameters.WorkArea 自动定位到屏幕工作区
- 多通知堆叠 ：维护每个位置的活动列表，自动计算偏移
- 关闭重排 ：通知关闭后自动重新排列剩余通知
- 线程安全 ：通过 Application.Current.Dispatcher 确保 UI 线程访问
- 批量关闭 ： CloseAll() 关闭所有通知
#### 4.使用示例
```xaml
using Wpf.CustomControl.Notifications;

// 1. 使用快捷方法（默认位置 BottomRight，3 秒后自动关闭）
NotificationManager.Info("操作已完成");
NotificationManager.Success("数据保存成功");
NotificationManager.Warning("内存不足，请释放空间");
NotificationManager.Error("网络连接失败");

// 2. 自定义配置
NotificationManager.Show(
    "这是一条重要通知",
    NotificationType.Warning,
    durationMs: 5000,                          // 5 秒后自动关闭
    position: NotificationPosition.TopRight,   // 右上角
    title: "系统提示");

// 3. 全局默认配置
NotificationManager.DefaultPosition = NotificationPosition.BottomRight;
NotificationManager.DefaultDurationMs = 4000;

// 4. durationMs = 0 时不自动关闭，仅手动关闭
NotificationManager.Show("需要手动关闭", NotificationType.Info, 0, NotificationPosition.TopCenter);

// 5. 批量关闭
NotificationManager.CloseAll();
```

#### 性能与安全性说明

- **TLS 支持**：自动启用 TLS 1.2，兼容现代 HTTPS 服务器
- **进度节流**：进度回调最高 10 次/秒，避免 UI 卡顿
- **缓冲区**：8KB 缓冲区，平衡内存占用与下载性能
- **取消支持**：所有异步方法支持 `CancellationToken`，可随时中断下载
- **错误处理**：网络错误抛出 `WebException`，取消抛出 `OperationCanceledException`

---

## 完整示例

### ViewModel 示例

```csharp
using Wpf.CustomControl.Mvvm;
using Wpf.CustomControl.Dialogs;

public class MainViewModel : ObservableObject
{
    private string _content;
    
    public string Content
    {
        get { return _content; }
        set { SetProperty(ref _content, value); }
    }

    private ICommand _saveCommand;
    public ICommand SaveCommand
    {
        get
        {
            return _saveCommand ?? (_saveCommand = new RelayCommand(
                () =>
                {
                    TipDialog.Show("保存成功！");
                },
                () => !string.IsNullOrEmpty(Content)
            ));
        }
    }

    private AsyncCommand _loadCommand;
    public AsyncCommand LoadCommand
    {
        get
        {
            return _loadCommand ?? (_loadCommand = new AsyncCommand(
                async () =>
                {
                    LoadingDialog.ShowLoading("正在加载...");
                    await Task.Delay(1000);
                    Content = "加载完成";
                    LoadingDialog.CloseLoading();
                }
            ));
        }
    }
}
```

### XAML 示例

```xml
<Window x:Class="MyApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:mvvm="clr-namespace:Wpf.CustomControl.Mvvm;assembly=Wpf.CustomControl"
        xmlns:drawing="clr-namespace:Wpf.CustomControl.Drawing;assembly=Wpf.CustomControl"
        xmlns:icons="clr-namespace:Wpf.CustomControl.Icons;assembly=Wpf.CustomControl"
        Title="MainWindow" Height="450" Width="800">
    <Grid>
        <!-- 图标按钮 -->
        <Button Command="{Binding SaveCommand}" Width="80" Height="30">
            <StackPanel Orientation="Horizontal">
                <icons:IconElement IconKind="Save" Width="16" Height="16" Margin="0,0,4,0" />
                <TextBlock Text="保存" />
            </StackPanel>
        </Button>

        <!-- 绘图画布 -->
        <drawing:DrawingCanvas x:Name="canvas" Width="400" Height="300" Margin="20" />
    </Grid>
</Window>
```

---

## 兼容性

- **目标框架**: .NET Framework 4.8
- **WPF**: Windows Presentation Foundation
- **语言**: C#（兼容 C# 5.0 语法，不使用 C# 6.0 特性）

---

## 注意事项

1. 使用对话框前必须在 `App.xaml` 中引用 `DialogResources.xaml`
2. 图标基于 24x24 视图框设计，建议使用 24x24 或 48x48 的尺寸
3. `DrawingElement` 的坐标基于其父容器 `DrawingCanvas`
4. `AsyncCommand` 在执行期间 `CanExecute` 返回 `false`，防止重复触发
