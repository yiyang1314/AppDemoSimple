# WPF / .NET 经典面试题精要

---

## 一、UI 基础：ContentControl 与 ItemsControl

| 对比项 | **ContentControl** | **ItemsControl** |
| :--- | :--- | :--- |
| **核心特征** | 只能包含**一个**子元素（内容） | 包含**一组**子元素（集合） |
| **典型控件** | `Button`、`Window`、`Label`、`CheckBox` | `ListBox`、`ListView`、`ComboBox`、`TreeView` |
| **关键属性** | `Content`（内容对象）和 `ContentTemplate`（数据模板） | `ItemsSource`（数据源）和 `ItemTemplate`（项模板） |
| **布局面板** | 通常只有一个根布局（如 `Grid`） | 通过 `ItemsPanel` 可自定义布局容器（如 `WrapPanel`） |

> 💡 面试点拨：为什么 `Button` 能放图片和文字？因为 `Button` 继承自 `ContentControl`，其 `Content` 属性是 `object` 类型，可以放任意 UI 元素。

---

## 二、WPF 中的模板（Template）

WPF 三大模板，面试必考：

| 模板类型 | 作用 | 核心用途 |
| :--- | :--- | :--- |
| **ControlTemplate** | 定义控件的**外观（结构）** | 改变控件外貌（如圆形按钮） |
| **DataTemplate** | 定义数据对象**如何显示** | 将数据对象呈现为带样式的 UI（如用户卡片） |
| **ItemsPanelTemplate** | 定义集合项的**布局方式** | 改变列表项的排列方向（如横向排列） |

> 💡 记忆口诀：ControlTemplate 改外貌，DataTemplate 改内容，ItemsPanelTemplate 改排列。

---

## 三、UserControl 与 CustomControl

| 对比项 | **UserControl（用户控件）** | **CustomControl（自定义控件）** |
| :--- | :--- | :--- |
| **创建方式** | 通过 **XAML + 代码隐藏** 组合现有控件 | 继承自 `Control` 或现有控件，使用 `Generic.xaml` 定义样式 |
| **模板支持** | 不支持重写 `ControlTemplate`（外观固定） | **完全支持** `ControlTemplate`，可自由换肤 |
| **适用场景** | 封装**业务逻辑强、复用性小**的模块（如登录面板） | 开发**高复用性、支持样式重写**的基础组件库（如日期选择器） |
| **属性定义** | 添加普通 CLR 属性即可 | 必须添加**依赖属性（DependencyProperty）** 以支持绑定和样式 |

---

## 四、属性系统：依赖属性、附加属性与普通属性

| 属性类型 | 特点 | 支持绑定/动画/样式 |
| :--- | :--- | :--- |
| **普通属性（CLR Property）** | 面向对象属性封装，无额外功能 | ❌ |
| **依赖属性（DependencyProperty）** | 注册到 WPF 属性系统，值存储在共享字典中，支持绑定、动画、样式继承 | ✅ |
| **附加属性（Attached Property）** | 特殊的依赖属性，用于**子元素设置父元素属性**（如 `Grid.Row`、`Canvas.Left`） | ✅ |

> 💡 面试点拨：为什么不用普通属性做绑定？因为 WPF 绑定需要**属性变更通知**（`INotifyPropertyChanged`）或 `DependencyObject` 的变更追踪，依赖属性内置了变更回调机制。

---

## 五、多线程与异步：Thread、ThreadPool 与 Task

| 对比项 | **Thread（线程）** | **ThreadPool（线程池）** | **Task（任务）** |
| :--- | :--- | :--- | :--- |
| **本质** | 操作系统原生线程，开销大（约1MB栈空间） | 线程缓存池，避免频繁创建销毁 | 基于 TPL（任务并行库），底层默认使用线程池 |
| **可控性** | 可设置优先级、前台/后台状态 | 无法控制线程生命周期 | 支持取消（`CancellationToken`）、等待（`Wait`）、延续（`ContinueWith`） |
| **返回值** | 无直接返回，需通过回调或委托 | 无直接返回 | **支持返回值**（`Task<TResult>`），原生支持 `async/await` |
| **使用场景** | 长时间阻塞操作（如死循环监听），需独立资源 | 简单短时后台计算 | **现代 .NET 首选**，适用于 I/O 密集和异步编程 |

---

## 六、UI 线程访问：Dispatcher.Invoke 与 BeginInvoke

在 WPF 中，只有 **UI 线程** 能修改控件，子线程必须通过 `Dispatcher` 调度。

| 方法 | 同步/异步 | 特点 | 使用建议 |
| :--- | :--- | :--- | :--- |
| **`Dispatcher.Invoke`** | **同步** | 阻塞调用线程直到 UI 执行完，易造成卡顿 | 尽量少用 |
| **`Dispatcher.BeginInvoke`** | **异步** | 立即返回，不阻塞调用线程 | 批量更新 UI 的首选 |

> 💡 现代做法：使用 `await Dispatcher.Yield()` 或 `Application.Current.Dispatcher.InvokeAsync`（本质仍是 BeginInvoke），或配合 `Task.Run` 后使用 `CancellationToken` 取消。

---

## 七、线程同步锁：lock、Monitor 与 Semaphore

| 同步机制 | **lock（语法糖）** | **Monitor（底层）** | **Semaphore（信号量）** |
| :--- | :--- | :--- | :--- |
| **本质** | `Monitor.Enter` 和 `Monitor.Exit` 的简化封装 | 提供 `Wait`（等待）、`Pulse`（唤醒）等精细控制 | 允许**指定数量的线程**同时访问资源（核心特征） |
| **特点** | 简洁易用，发生异常自动释放锁 | 功能更强大，可实现生产者-消费者模式 | 分本地/全局信号量，计数可设置 |
| **适用场景** | 保护共享资源（变量、缓存）的互斥访问 | 需要线程间**交互信号**的复杂逻辑 | **限制并发数**（如最大连接数、限流器） |

---

## 八、C# 语言基础：泛型、装箱与拆箱

- **泛型（Generic）**：延迟声明类型（如 `List<T>`）。  
  **优点**：类型安全、避免装箱拆箱、代码复用。

- **装箱（Boxing）**：**值类型 → 引用类型（object）**，在堆上分配内存，损耗性能。

- **拆箱（Unboxing）**：**引用类型 → 值类型**，需显式转换，可能引发 `InvalidCastException`。

> 💡 面试点拨：`ArrayList` 是非泛型集合，存 `int` 会装箱；`List<int>` 是泛型，直接存储值类型，**零开销**。WPF 的 `IValueConverter` 中常涉及拆箱操作。

---

## 九、ORM 框架对比：EF6、SqlSugar、Dapper

| 框架类型 | **EF6 (Entity Framework)** | **Dapper (Micro ORM)** | **SqlSugar (国产 ORM)** |
| :--- | :--- | :--- | :--- |
| **定位** | 重量级全功能 ORM（微软官方） | 轻量级高性能 ORM（Stack Overflow 出品） | 高性能 + 丰富功能国产 ORM（Dapper 性能 + EF 功能） |
| **SQL 生成** | 自动生成复杂 SQL（变更追踪、延迟加载） | **手写 SQL**，完全掌控执行语句 | 支持 Lambda 生成 SQL，**无跟踪查询** 性能极佳 |
| **性能** | 较慢（反射 + 变更追踪开销大） | **极快**（原生 IDbConnection 扩展） | **极快**（支持批量操作、读写分离、分表分库） |
| **适用场景** | 大型复杂业务、快速开发、团队熟悉度高的项目 | **性能敏感**项目（如实时报表、高并发 API） | 中小型项目、需要 SQL 监控、国产数据库适配 |

---

## 十、架构模式：IOC（控制反转）与 DI（依赖注入）

- **IOC（控制反转）**：一种**设计原则**，将对象的创建和依赖关系的控制权从调用方（类内部）转移到容器（外部）。
- **DI（依赖注入）**：实现 IOC 的**具体方式**，通过构造函数、属性、方法将依赖对象传进来。

| 维度 | **优点** | **缺点** |
| :--- | :--- | :--- |
| **解耦** | 模块间零耦合，修改 A 不影响 B | 调试困难（运行时才报错，需依赖日志） |
| **测试** | **极大方便单元测试**（可替换为模拟 Mock 对象） | 代码量增多（需配置容器、注册接口） |
| **生命周期** | 容器统一管理单例（Singleton）、作用域（Scoped）等 | 过度设计风险（小型项目用工厂模式比 IOC 更简单） |

> 💡 面试点拨：WPF 中的 **Prism**、**MVVM Light** 都内置了 IOC 容器（如 Unity、Autofac）。问“为什么不直接用 `new`？”——因为 `new` 会导致**硬编码耦合**，无法进行 Mock 单元测试。

---

## 面试金句速记

1. **控件**：“ContentControl 单内容，ItemsControl 多集合，控件模板改外貌，数据模板改内容。”
2. **线程**：“耗时操作用 Task，UI 更新用 Dispatcher。”
3. **属性**：“普通属性存数据，依赖属性绑动画，附加属性放子级。”
4. **ORM**：“性能选 Dapper，复杂逻辑选 EF，平衡国产选 SqlSugar。”
5. **设计**：“IOC 是原则，DI 是实现，解耦测试全靠它。”

---

以下是补充整理的 **.NET/WPF 高级面试题精要**，涵盖网络协议、数据库优化、主流 MVVM 框架、委托事件以及异步编程等核心知识点。

---

# .NET / WPF 高级面试题补充篇

---

## 十一、网络协议：TCP 与 UDP

| 对比项 | **TCP（传输控制协议）** | **UDP（用户数据报协议）** |
| :--- | :--- | :--- |
| **连接状态** | **面向连接**（三次握手、四次挥手） | **无连接**（尽最大努力交付） |
| **可靠性** | **可靠传输**（确认重传、流量控制、拥塞控制） | **不可靠传输**（丢包不重传，乱序不纠错） |
| **速度/开销** | 慢（头部开销大，20~60 字节） | 快（头部开销小，8 字节） |
| **数据边界** | **流式传输**（无边界，需自定义协议拆包） | **报文传输**（有边界，每个包独立） |
| **典型场景** | HTTP/HTTPS、FTP、SMTP、数据库连接 | 视频直播、VoIP、DNS 查询、游戏位置同步 |
| **Socket 关键** | 需 Listen/Connect/Accept 建立管道 | 直接 SendTo/ReceiveFrom，无需长久维持 |

> 💡 面试点拨：“QQ 为什么既用 TCP 又用 UDP？”——登录/支付等核心数据用 TCP 保证可靠；实时音视频/位置用 UDP 追求低延迟。

---

## 十二、MySQL 数据库设计、性能优化与索引

### 1. 数据库设计原则

- **三大范式（3NF）**：原子性、消除部分依赖、消除传递依赖。
- **反范式设计**：适当冗余字段（如订单表冗余商品名称），减少联表查询（Join）以提升读性能。
- **数据类型选择**：能用 `TINYINT` 不用 `INT`；能用 `VARCHAR(255)` 不用 `TEXT`；日期用 `DATETIME` 或 `TIMESTAMP`。

### 2. 索引优化策略

| 索引类型 | 特点 | 使用建议 |
| :--- | :--- | :--- |
| **B+Tree 索引**（InnoDB 默认） | 多路平衡树，支持范围查询 | 适用于高选择性列（如 ID、唯一键） |
| **Hash 索引**（Memory 引擎） | 单值查询极快，不支持范围 | 仅用于精确等值匹配 |
| **全文索引** | 支持分词搜索 | 用于 `LIKE '%xxx%'` 场景替代方案 |
| **联合索引** | 多列组合，遵循**最左前缀原则** | `WHERE a=1 AND b=2` 命中 `(a,b)`；`WHERE b=2` 不命中 |

### 3. SQL 性能优化三板斧

1. **慢查询日志**：开启 `slow_query_log`，定位 `EXPLAIN` 中 `type=ALL`（全表扫描）的 SQL。
2. **避免索引失效**：不在索引列上做函数运算（`WHERE DATE(create_time) = '2023-01-01'` 失效，应改为 `WHERE create_time BETWEEN ...`）；避免 `LIKE '%xxx'`（前置 % 会导致索引失效）。
3. **分页优化**：深分页（`LIMIT 100000, 10`）极慢，采用**延迟关联**（先查 ID 再联表）或**游标分页**（记录上一页最大 ID）。

> 💡 面试点拨：“QPS 突降如何排查？”——先看网络/CPU，再看慢查询日志，最后利用 `SHOW PROCESSLIST` 查看当前阻塞锁。

---

## 十三、Prism 与 CommunityToolkit.Mvvm 框架对比

| 功能特性 | **Prism（经典企业级框架）** | **CommunityToolkit.Mvvm（现代轻量级框架）** |
| :--- | :--- | :--- |
| **核心定位** | 大而全（模块化、Region、导航、状态管理） | 小而美（专注 MVVM 基础，高度性能优化） |
| **绑定命令** | `DelegateCommand` / `AsyncDelegateCommand` | `RelayCommand` / `AsyncRelayCommand`（支持泛型） |
| **消息通信** | `EventAggregator`（强类型事件聚合器，松耦合） | `WeakReferenceMessenger`（基于弱引用的信使，防止内存泄漏） |
| **IOC 容器** | 内置 `Unity` 或 `DryIoc`，支持模块按需加载 | **无内置 IOC**，需搭配 `Microsoft.Extensions.DependencyInjection` |
| **导航管理** | `IRegionManager` + 视图注入，支持 Journal（回退） | 无内置，需自定义或使用第三方路由 |
| **适用场景** | 大型复杂 WPF 桌面应用（如产线 MES、IDE 工具） | 中小型项目、UWP/WinUI/MAUI 跨平台应用 |

### 应用到的设计模式

- **MVVM 模式**（表现层分离）。
- **命令模式（Command Pattern）**：将 UI 操作封装为对象。
- **中介者模式（Mediator）**：通过 `EventAggregator` 或 `Messenger` 解耦 ViewModel 间通信。
- **策略模式（Strategy）**：Prism 的 `Navigation` 允许多种视图切换策略。
- **工厂模式（Factory）**：IOC 容器底层实现。
- **观察者模式（Observer）**：INotifyPropertyChanged 本身就是观察者模式的经典实现。

---

## 十四、委托（Delegate）与事件（Event）

| 对比项 | **委托（Delegate）** | **事件（Event）** |
| :--- | :--- | :--- |
| **本质** | 类型安全的**函数指针**，可承载多个方法（多播）。 | **委托的封装器（Wrapper）**，受 `event` 关键字约束。 |
| **声明方式** | `public delegate void MyDel(int x);` | `public event MyDel MyEvent;` |
| **调用权限** | 可在**类外部**直接调用委托（`.Invoke()`）。 | 只能在**定义事件的类内部**调用（外部仅能 `+=` 或 `-=`）。 |
| **赋值权限** | 可直接 `=` 覆盖旧方法。 | 不能直接 `=`（只能 `+=` 追加），防止外部覆盖已注册的逻辑。 |
| **核心用途** | 回调方法、Lambda 表达式、LINQ（`Action`/`Func`）。 | 控件点击事件（`Button.Click`）、属性变更通知（`PropertyChanged`）。 |

> 💡 面试点拨：**`Action`（无返回值）和 `Func`（有返回值）是微软预定义的泛型委托**，日常开发中无需自定义委托，除非有特殊 `ref` 或 `out` 参数需求。

---

## 十五、Async/Await、多线程与 Task.Run 的深度辨析

### 1. 核心区别：异步 ≠ 多线程

| 概念 | **Async/Await（异步编程模型）** | **多线程（Thread / ThreadPool）** | **Task.Run（任务调度）** |
| :--- | :--- | :--- | :--- |
| **本质** | 编译器生成状态机，**不一定会创建新线程**。 | 并发执行多个线程（操作系统调度）。 | 将委托**压入线程池队列**，由线程池线程执行。 |
| **执行线程** | 在**当前上下文（SynchronizationContext）** 上等待 I/O 完成，期间**释放线程**。 | 占用独立或线程池资源。 | 占用**线程池线程**（非 UI 线程）。 |
| **适用场景** | **I/O 密集型**（网络请求、文件读写、数据库查询）。 | **CPU 密集型**计算（复杂循环、大量数学运算）。 | 后台执行 CPU 密集型任务，释放 UI 线程。 |
| **返回值** | 支持 `Task<T>`，方便链式调用。 | 无直接返回值，需回调或共享变量（需加锁）。 | 支持 `Task<T>`。 |
| **阻塞风险** | `await` 不阻塞；错误使用 `.Result` 或 `.Wait()` 会导致**死锁**。 | `Thread.Sleep` 会阻塞线程。 | `Task.Run` 配合 `await` 不会阻塞 UI。 |

### 2. 经典误区解析

- **误区 1**：“`async` 方法会自己跑在后台线程上。”
  - **纠正**：`async` 方法默认在**调用线程**上执行，直到遇到 `await`。若 `await` 尚未完成（I/O 未返回），它会捕获上下文并挂起，**此时线程被释放回线程池**，而不是创建一个新线程。

- **误区 2**：“`Task.Run` 就是开启多线程。”
  - **纠正**：`Task.Run` 是**将任务排队到线程池**，底层复用线程池资源，比 `new Thread()` 开销小得多。适用于需要后台处理的 CPU 运算。

- **误区 3**：“WPF 中 `async void` 可以随意用。”
  - **纠正**：`async void` 只能用于**事件处理器**（如按钮点击）。其他地方请用 `async Task`，否则异常会直接终结进程且无法被调用方捕获。

### 3. 实战代码示例（WPF）

```csharp
// 正确：异步下载，不阻塞 UI
private async void Download_Click(object sender, RoutedEventArgs e)
{
    // 此处处于 UI 线程（捕获 SynchronizationContext）
    var data = await HttpClient.GetStringAsync("https://api...");
    // 此处仍回到 UI 线程，可直接更新 TextBox
    this.MyTextBox.Text = data;
}

// 错误：Task.Run 后直接访问 UI（未回到 UI 线程）
private void BadExample()
{
    Task.Run(() =>
    {
        // 这是在后台线程
        var result = HeavyCompute();
        // 错误！无法直接更新 UI 控件
        // this.MyLabel.Content = result; 
        // 正确：需要 Dispatcher.BeginInvoke 或 await
    });
}
```

---

## 面试金句速记（补充篇）

1. **网络**：“TCP 重可靠握手慢，UDP 快但有风险。”
2. **数据库**：“索引设计看最左，深分页用延迟关联，SQL 写前看 Explain。”
3. **框架**：“大项目用 Prism（模块化），小项目选 Toolkit（源生成器高性能）。”
4. **委托事件**：“委托是多播函数指针，事件是私有委托带约束。”
5. **异步**：“I/O 密集用 Async，CPU 密集用 Task.Run，UI 更新记上下文。”

---



# 经典面试题集（C# / .NET / WPF 基础与进阶）

---

## 十六、进程、线程与多线程基础

### 1. 进程与线程的区别

| 对比项 | **进程（Process）** | **线程（Thread）** |
| :--- | :--- | :--- |
| **定义** | 操作系统分配资源的**最小单位** | CPU 调度的**最小执行单位** |
| **资源开销** | 独立的地址空间（4GB虚拟内存）、文件句柄表、环境变量，**开销大** | 共享进程地址空间，仅保存执行上下文（栈、寄存器），**开销小** |
| **独立性** | 进程间相互隔离，一个进程崩溃不影响其他 | 线程间共享内存，一个线程崩溃可能导致整个进程终止 |
| **通信方式** | IPC（管道、共享内存、消息队列、Socket） | 直接共享内存（需加锁同步） |
| **创建速度** | 慢（约数百毫秒） | 快（约几毫秒） |

> 面试点拨：**多进程**适合隔离性要求高的场景（如浏览器多标签页）；**多线程**适合同一任务内的并发协作（如下载+解析+保存）。

### 2. 开启多线程的方式及优缺点

| 方式 | 代码示例 | 优点 | 缺点 |
| :--- | :--- | :--- | :--- |
| **Thread 类** | `new Thread(() => {})` | 可设置优先级、前台/后台状态，可控性强 | 开销大（~1MB栈），需手动管理生命周期 |
| **ThreadPool** | `ThreadPool.QueueUserWorkItem()` | 线程复用，避免频繁创建销毁 | 无法控制优先级和生命周期，不支持返回值 |
| **Task / Task.Run** | `Task.Run(() => {})` | 支持返回值、取消、异常捕获，**现代首选** | 内部仍使用线程池，长时间任务可能阻塞线程池 |
| **Parallel** | `Parallel.ForEach()` | 自动分区，适合 CPU 密集型循环 | 需注意线程安全，可能产生额外开销 |
| **async/await** | `await HttpClient.GetStringAsync()` | **不创建新线程**，节省线程资源 | 需理解状态机和同步上下文 |

### 3. 线程安全方式详解

| 方式 | 适用场景 | 特点 |
| :--- | :--- | :--- |
| **lock** | 保护共享资源的互斥访问 | Monitor.Enter/Exit 语法糖，异常自动释放 |
| **Monitor** | 需要 Wait/Pulse 精细控制 | 比 lock 灵活，可实现生产者-消费者模式 |
| **SemaphoreSlim** | 限制并发数量 | 轻量信号量，支持异步等待 WaitAsync |
| **ReaderWriterLockSlim** | 读多写少场景 | 允许多个读线程并发，写线程独占 |
| **ConcurrentDictionary** | 线程安全字典 | 内部分段锁，无需外部加锁 |
| **Interlocked** | 原子操作（计数器） | CPU 级别原子指令，无锁设计 |

---

## 十七、面向对象与 C# 语言特性

### 1. 面向对象三大特性

| 特性 | 概念 | 代码示例 |
| :--- | :--- | :--- |
| **封装（Encapsulation）** | 将数据和操作包装在类中，通过**访问修饰符**控制外部访问 | `private` 字段 + `public` 属性，外部无法直接修改内部状态 |
| **继承（Inheritance）** | 子类**复用**父类的属性和方法，建立类层次结构 | `class Dog : Animal`，Dog 继承 Animal 的 Name 属性 |
| **多态（Polymorphism）** | 同一接口有不同实现，**运行时**根据对象类型调用对应方法 | `Animal a = new Dog(); a.Speak()` 调用 Dog 的 Speak |

### 2. 重载、重写与多态

| 概念 | 定义 | 绑定时机 | 示例 |
| :--- | :--- | :--- | :--- |
| **重载（Overload）** | **同一方法名**，参数列表不同（类型/数量/顺序） | **编译时**绑定（早绑定） | `void Foo(int x)` 和 `void Foo(string s)` |
| **重写（Override）** | 子类**重新实现**父类的 virtual/abstract 方法 | **运行时**绑定（晚绑定） | `override void Speak()` 覆盖基类方法 |
| **多态（Polymorphism）** | 基类引用指向派生类实例，调用派生类方法 | **运行时** | `Animal a = new Cat(); a.Speak()` -> Cat 的 Speak |

```csharp
public class Animal
{
    public virtual void Speak() { Console.WriteLine("..."); }
}
public class Dog : Animal
{
    public override void Speak() { Console.WriteLine("汪汪"); }
}
// 多态调用
Animal myDog = new Dog();
myDog.Speak();  // 输出 "汪汪"，运行时确定调用 Dog 的方法
```

### 3. 类与接口、抽象类的区别

| 对比项 | **类（Class）** | **抽象类（Abstract Class）** | **接口（Interface）** |
| :--- | :--- | :--- | :--- |
| **实例化** | 可以直接 `new` | **不能**实例化 | **不能**实例化 |
| **成员实现** | 所有成员必须实现 | 可有已实现的方法（具体方法）和未实现的方法（abstract） | **不能有实现**（C#8.0 前），C#8.0+ 可有默认实现 |
| **状态（字段）** | 可包含字段 | 可包含字段 | **不能有字段**（只能有属性/方法签名） |
| **继承限制** | 单继承 | 单继承 | **多实现**（一个类可实现多个接口） |
| **访问修饰符** | 成员可有各种修饰符 | 成员可有各种修饰符 | 成员默认 `public`，不能指定修饰符 |
| **设计目的** | 具体实现 | "是什么"（Is-A）关系，**模板式设计** | "能做什么"（Can-Do）能力，**契约式设计** |

> 面试点拨：抽象类是"半成品"（有共性有差异），接口是"规范书"（只定义能力不关心实现）。**优先使用接口**，因为接口更灵活（支持多实现）。

### 4. 值类型与引用类型、装箱与拆箱

| 概念 | 说明 | 内存位置 |
| :--- | :--- | :--- |
| **值类型** | `int`、`double`、`bool`、`struct`、`enum` | **栈（Stack）** 上分配，随作用域结束自动释放 |
| **引用类型** | `string`、`array`、`class`、`List<T>` | **堆（Heap）** 上分配，栈上保存引用地址 |
| **装箱（Boxing）** | 值类型 -> `object`（引用类型） | 堆上新建副本，原栈中值不变 |
| **拆箱（Unboxing）** | `object` -> 值类型 | 显式转换 `(int)obj`，可能抛 `InvalidCastException` |

```csharp
int num = 42;           // 栈上
object boxed = num;     // 装箱：堆上分配 42 的副本
int unboxed = (int)boxed; // 拆箱：从堆读取回值类型
```

> 性能警示：**避免在循环中频繁装箱**。`ArrayList` 存 `int` 会装箱，`List<int>` 不会。

### 5. StringBuilder 与 string 拼接

| 对比项 | **string** | **StringBuilder** |
| :--- | :--- | :--- |
| **不可变性** | 不可变（Immutable），每次拼接都**创建新实例** | 可变（Mutable），内部维护字符缓冲区 |
| **拼接性能** | 连续拼接 N 次，时间复杂度 **O(N²)** | 连续拼接 N 次，时间复杂度 **O(N)** |
| **内存分配** | 每次 `+=` 都在堆上**分配新内存** | 预分配缓冲区（默认 16 字符），按需扩容 |
| **适用场景** | 少量拼接（< 10 次），或编译期可确定的字符串 | 大量拼接（循环、动态构建 SQL/JSON） |

```csharp
// 慢：每次拼接都创建新 string 实例
string result = "";
for (int i = 0; i < 10000; i++)
{
    result += i.ToString();  // 10000 次内存分配
}

// 快：StringBuilder 内部缓冲区复用
var sb = new StringBuilder();
for (int i = 0; i < 10000; i++)
{
    sb.Append(i.ToString());  // 仅 1~2 次缓冲区扩容
}
string result = sb.ToString();
```

---

## 十八、内存管理与垃圾回收

### 1. 托管内存与非托管内存

| 对比项 | **托管内存（Managed）** | **非托管内存（Unmanaged）** |
| :--- | :--- | :--- |
| **分配方式** | 通过 `new` 在托管堆上分配 | 通过 `Marshal.AllocHGlobal` 或 C++ 的 `malloc` |
| **回收方式** | **GC 自动回收**，无需手动释放 | **必须手动释放**（`Marshal.FreeHGlobal` / `free`） |
| **指针访问** | **不可**（C# 中不允许 unsafe 指针操作） | **可以**直接通过指针访问 |
| **安全性** | 类型安全，不会内存泄漏 | 可能内存泄漏、悬垂指针 |
| **典型场景** | 所有 CLR 对象（类、数组、字符串） | 调用 Win32 API、COM 互操作、图像像素数据 |

### 2. 垃圾回收（GC）机制

GC 使用 **标记-清除（Mark-and-Sweep）** 算法，分三代管理：

| 代 | 说明 | 回收频率 |
| :--- | :--- | :--- |
| **Gen 0** | 最新分配的对象（< 85KB） | 最频繁，几乎每次 GC 都扫描 |
| **Gen 1** | 在 Gen 0 回收中存活的对象 | 偶尔 |
| **Gen 2** | 在 Gen 1 回收中存活的对象（大对象） | 很少，Full GC 时扫描 |

**GC 工作流程**：
1. **标记（Mark）**：从根对象栈（Root Stack）出发，递归标记所有可达对象
2. **清除（Sweep）**：回收未被标记的对象，压缩内存（移动存活对象使其连续）
3. **提升代数**：存活对象提升一代（Gen0 -> Gen1 -> Gen2）

### 3. IDisposable 接口的作用

**解决问题**：非托管资源（文件句柄、数据库连接、Socket、位图）**不受 GC 管**，必须手动释放。

```csharp
public class ResourceManager : IDisposable
{
    private IntPtr _unmanagedHandle;
    private bool _disposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);  // 告诉 GC 不需要终结器了
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // 释放托管资源
            }
            // 释放非托管资源
            if (_unmanagedHandle != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_unmanagedHandle);
                _unmanagedHandle = IntPtr.Zero;
            }
            _disposed = true;
        }
    }

    ~ResourceManager()
    {
        Dispose(false);  // 终结器：仅释放非托管资源
    }
}
```

> 面试点拨：`using` 语句会自动调用 `Dispose()`，是使用 `IDisposable` 对象的最佳实践。**永远不要依赖终结器释放资源**，因为终结器执行时间不确定。

---

## 十九、经典设计模式（GoF 23 种 + 项目实战）

### 创建型模式（5 种）

| 模式 | 核心意图 | 优点 | 缺点 | WPF 应用 |
| :--- | :--- | :--- | :--- | :--- |
| **单例（Singleton）** | 确保一个类只有一个实例 | 节省内存，全局访问点 | 难以测试（全局状态），隐式依赖 | `Application.Current`、`Dispatcher` |
| **工厂方法（Factory Method）** | 子类决定创建哪个对象 | 符合开闭原则，扩展新类型无需修改工厂 | 增加类的数量 | `DataTemplate` 根据类型创建 UI |
| **抽象工厂（Abstract Factory）** | 创建产品族的接口 | 产品族切换容易，保证产品一致性 | 扩展新产品需修改接口 | WPF 主题（不同主题的控件工厂） |
| **建造者（Builder）** | 分步创建复杂对象 | 构建逻辑与表示分离，参数化构造 | 增加代码量 | `Grid`/`StackPanel` 的程序化构建 |
| **原型（Prototype）** | 通过克隆创建新实例 | 避免重复创建开销 | 需实现深拷贝，克隆可能有副作用 | `Freezable.Clone()` 克隆动画 |

### 结构型模式（7 种）

| 模式 | 核心意图 | 优点 | 缺点 | WPF 应用 |
| :--- | :--- | :--- | :--- | :--- |
| **适配器（Adapter）** | 将不兼容的接口转换为兼容 | 复用现有代码，无需修改原接口 | 增加间接层 | `IValueConverter` 适配数据到 UI |
| **装饰器（Decorator）** | 动态添加对象职责 | 灵活，比继承更灵活 | 增加对象包装层数 | WPF 的 `Effect`（DropShadowEffect 装饰） |
| **代理（Proxy）** | 控制对象的访问 | 延迟加载、权限控制 | 增加间接调用开销 | `WebBrowser` 的代理加载 |
| **组合（Composite）** | 树形结构统一处理 | 简化客户端代码 | 难以限制组合类型 | `Panel` 包含子元素（树结构） |
| **外观（Facade）** | 简化子系统的接口 | 降低复杂度，解耦 | 可能成为上帝对象 | `Window` 封装 `Show/ShowDialog` |
| **桥接（Bridge）** | 分离抽象和实现 | 独立变化，扩展性好 | 增加抽象层次 | WPF 的属性系统（DependencyObject 映射） |
| **享元（Flyweight）** | 共享对象以节省内存 | 减少内存占用 | 增加复杂度，需管理共享状态 | `Brushes.Red`、`Brushes.Blue`（共享画刷实例） |

### 行为型模式（11 种）

| 模式 | 核心意图 | 优点 | WPF 应用 |
| :--- | :--- | :--- | :--- |
| **观察者（Observer）** | 一对多依赖关系，状态变更自动通知 | 解耦，支持广播 | `INotifyPropertyChanged`（MVVM 核心） |
| **命令（Command）** | 将请求封装为对象 | 参数化、支持撤销/排队 | `ICommand`（WPF 命令绑定） |
| **策略（Strategy）** | 算法族可互相替换 | 独立变化，避免条件分支 | WPF 的 `DataTrigger` 切换样式策略 |
| **状态（State）** | 对象状态改变时行为跟着变 | 消除状态条件分支 | `WindowState`（Normal/Maximized/Minimized） |
| **模板方法（Template Method）** | 固定算法骨架，子类实现变化步骤 | 复用不变部分，扩展可变部分 | `OnRender`、`OnApplyTemplate`（WPF 控件模板方法） |
| **中介者（Mediator）** | 对象间通过中介者通信 | 减少对象间直接依赖 | `EventAggregator`（Prism 消息机制） |
| **迭代器（Iterator）** | 顺序访问集合元素 | 不暴露集合内部 | `ItemsControl.GetItemsOwner` |
| **责任链（Chain of Responsibility）** | 请求沿链传递直到被处理 | 灵活的处理流程 | WPF 路由事件（Routed Event）隧道/冒泡 |
| **备忘录（Memento）** | 保存/恢复对象状态 | 支持撤销/回滚 | 撤销/重做功能（UndoEngine） |
| **访问者（Visitor）** | 分离数据结构和操作 | 新增操作无需修改结构 | WPF 可视化树遍历 |
| **解释器（Interpreter）** | 定义文法解释器 | 可扩展文法 | WPF 绑定表达式解析 |

### 项目实战中最常用的设计模式

| 模式 | 频率 | 典型应用场景 |
| :--- | :--- | :--- |
| **观察者模式** | 5星 | MVVM 的 PropertyChanged、Prism 的 EventAggregator |
| **命令模式** | 5星 | WPF 的 ICommand、自定义按钮命令 |
| **单例模式** | 4星 | 全局服务（日志、配置、缓存管理器） |
| **工厂模式** | 4星 | 创建视图/ViewModel、数据库连接工厂 |
| **策略模式** | 4星 | 数据导出（Excel/PDF/JSON）、支付方式选择 |
| **适配器模式** | 3星 | 老接口适配、第三方 SDK 封装 |
| **装饰器模式** | 3星 | WPF 控件效果（阴影、模糊）、日志包装 |
| **模板方法** | 3星 | WPF 自定义控件（重写 OnRender） |

---

---

## 二十、MVVM 开发模式

### 1. MVVM 核心架构

MVVM（Model-View-ViewModel）是 WPF 的**官方推荐架构模式**，实现了 UI 与业务逻辑的彻底分离。

| 层次 | 职责 | 示例 |
| :--- | :--- | :--- |
| **Model（模型）** | 数据实体、业务逻辑、数据访问 | User 实体、UserRepository 数据访问类 |
| **View（视图）** | 纯 UI 展示，无业务逻辑 | MainWindow.xaml、UserControl.xaml |
| **ViewModel（视图模型）** | View 与 Model 之间的桥梁，管理 View 所需的状态和命令 | MainViewModel.cs、UserViewModel.cs |

> 💡 核心思想：**View 绑定 ViewModel，ViewModel 绑定 Model**，通过双向绑定实现数据流动。

### 2. MVVM 三要素

| 要素 | 作用 | 实现方式 |
| :--- | :--- | :--- |
| **属性绑定** | View 自动反映 ViewModel 属性变更 | INotifyPropertyChanged 接口 + PropertyChanged 事件 |
| **命令绑定** | View 的操作（按钮点击）映射到 ViewModel 的方法 | ICommand 接口（RelayCommand / DelegateCommand） |
| **数据模板** | 定义数据对象如何渲染为 UI | DataTemplate 隐式/显式映射 |

### 3. MVVM 优点

| 优点 | 说明 |
| :--- | :--- |
| **解耦** | UI 设计师与程序员可并行开发，互不干扰 |
| **可测试性** | ViewModel 可独立进行单元测试，无需启动 UI |
| **可维护性** | 业务逻辑集中在 ViewModel，修改 UI 不影响逻辑 |
| **可复用性** | 同一 ViewModel 可绑定到不同 View（如桌面端/移动端共用） |

### 4. MVVM 实战代码示例

```csharp
// Model 层：数据实体
public class User
{
    public string Name { get; set; }
    public string Email { get; set; }
}

// ViewModel 层：继承 ObservableObject（CommunityToolkit.Mvvm）
public partial class UserViewModel : ObservableObject
{
    [ObservableProperty]
    private string _userName = "";

    [ObservableProperty]
    private string _email = "";

    [RelayCommand]
    private async Task SaveUserAsync()
    {
        var user = new User { Name = UserName, Email = Email };
        await _userRepository.AddAsync(user);
        await MessageBox.ShowAsync("保存成功！");
    }
}

// View 层：XAML 绑定
/*
<StackPanel>
    <TextBox Text="{Binding UserName, UpdateSourceTrigger=PropertyChanged}" />
    <TextBox Text="{Binding Email, UpdateSourceTrigger=PropertyChanged}" />
    <Button Content="保存" Command="{Binding SaveUserCommand}" />
</StackPanel>
*/
```

### 5. 常见 MVVM 框架对比

| 框架 | 特点 | 适用场景 |
| :--- | :--- | :--- |
| **CommunityToolkit.Mvvm** | 源生成器驱动，零反射，性能最佳 | 新项目首选，轻量级应用 |
| **Prism** | 模块化、Region 导航、EventAggregator、内置 IOC | 大型企业级应用（MES、IDE 工具） |
| **MVVM Light** | 轻量级，ViewModelBase + Messenger | 中小型项目，入门学习 |
| **ReactiveUI** | 响应式扩展，基于 IObservable | 复杂异步交互、实时数据流 |

> 💡 面试点拨：选择框架看需求——**新项目用 Toolkit**，**大项目用 Prism**，**复杂异步用 ReactiveUI**。

---

## 二十一、数据库使用与优化

### 1. SQL 层面优化

| 优化策略 | 说明 | 示例 |
| :--- | :--- | :--- |
| **避免 SELECT \*** | 只查询需要的列，减少网络和内存开销 | `SELECT id, name, email FROM users` 而非 `SELECT *` |
| **参数化查询** | 防止 SQL 注入，利用查询计划缓存 | `cmd.CommandText = "SELECT * FROM users WHERE id=@id"; cmd.Parameters.AddWithValue("@id", 1)` |
| **避免全表扫描** | 使用索引，避免 `LIKE '%xxx'`（前置通配符） | `WHERE name LIKE '张%'` 可走索引，`LIKE '%三'` 不行 |
| **合理使用分页** | 避免深分页（`LIMIT 100000, 10`） | 使用游标分页或延迟关联 |
| **避免在索引列上运算** | 函数运算导致索引失效 | `WHERE DATE(create_time) = '2024-01-01'` 应改为 `WHERE create_time BETWEEN '2024-01-01' AND '2024-01-02'` |

### 2. 索引优化

| 索引类型 | 特点 | 使用场景 |
| :--- | :--- | :--- |
| **B+Tree 索引** | 多路平衡树，支持范围查询和排序 | 主键、唯一键、高选择性列 |
| **Hash 索引** | 等值查询极快，不支持范围查询 | Memory 引擎的精确匹配 |
| **联合索引** | 多列组合，遵循**最左前缀原则** | `WHERE a=1 AND b=2` 命中 `(a,b)`；`WHERE b=2` 不命中 |
| **覆盖索引** | 索引包含查询所需所有列 | 避免回表查询，提升性能 |

> 💡 最左前缀原则：联合索引 `(a, b, c)` 只对 `a`、`a,b`、`a,b,c` 查询生效，`b` 或 `c` 单独查询**不会使用索引**。

### 3. C# / ADO.NET 层面优化

| 优化策略 | 说明 | 代码示例 |
| :--- | :--- | :--- |
| **连接池** | 使用连接池避免频繁创建销毁连接 | `SqlConnection` 默认启用连接池（`Max Pool Size=100`） |
| **异步操作** | 使用 `ExecuteReaderAsync` / `ReadAsync` 避免阻塞 | `await cmd.ExecuteReaderAsync()` |
| **批量操作** | 使用 `SqlBulkCopy` 批量导入数据 | `using var bulkCopy = new SqlBulkCopy(connection); bulkCopy.WriteToServer(dataTable);` |
| **使用 `AsNoTracking`** | EF 查询无跟踪，减少变更追踪开销 | `context.Users.AsNoTracking().ToList()` |
| **合理使用 `Include`** | 避免过度 eager loading，按需加载关联数据 | `context.Users.Include(u => u.Orders).ToList()` |

### 4. Entity Framework 优化技巧

```csharp
// 1. 使用 AsNoTracking 进行只读查询
var users = context.Users.AsNoTracking()
    .Where(u => u.IsActive)
    .ToList();

// 2. 避免 N+1 查询问题
var orders = context.Orders
    .Include(o => o.User)       // 一次性加载用户
    .Include(o => o.Product)   // 一次性加载产品
    .ToList();

// 3. 使用编译查询缓存（EF6）
// EF Core 5+ 自动缓存查询计划
var query = context.Users
    .Where(u => u.Id == userId)
    .Select(u => new { u.Name, u.Email })
    .FirstOrDefault();

// 4. 批量操作（EF Core 7+）
context.Users.Where(u => u.IsDeleted).ExecuteUpdateAsync(
    u => u.SetProperty(x => x.Status, "已删除"));
```

### 5. 慢查询排查步骤

1. **开启慢查询日志**：`SET GLOBAL slow_query_log = 'ON';`
2. **使用 EXPLAIN 分析执行计划**：关注 `type` 列（`ALL` 表示全表扫描，需优化）
3. **检查索引命中**：`key` 列显示实际使用的索引
4. **查看扫描行数**：`rows` 列显示预估扫描行数
5. **定位瓶颈**：检查是否有 `Using where`、`Using filesort`、`Using temporary`

> 💡 面试点拨："数据库查询慢怎么优化？"——三步走：**加索引 → 改 SQL → 分库分表**。先看执行计划，再针对性优化。

---

## 面试金句速记（终章）

1. **MVVM**："View 绑 ViewModel，VM 绑 Model，双向驱动解耦强。"
2. **数据库**："索引设计看选择性，查询避免全表扫，深分页用延迟关联。"
3. **EF**："AsNoTracking 查只读，Include 避免 N+1，批量操作用 BulkCopy。"
4. **设计模式**："观察者 + 命令 = MVVM；单例 + 工厂 = IOC 容器。"
5. **性能**："先测量后优化，过早优化是万恶之源。"

---

## 全书总结与速查表

| 主题 | 核心关键词 |
| :--- | :--- |
| **WPF 基础** | ContentControl、ItemsControl、Template、DependencyProperty |
| **多线程** | Thread、ThreadPool、Task、async/await、Dispatcher |
| **线程同步** | lock、Monitor、SemaphoreSlim、ConcurrentDictionary |
| **面向对象** | 封装、继承、多态、重载、重写、接口、抽象类 |
| **内存管理** | 值类型/引用类型、装箱/拆箱、GC 三代、IDisposable |
| **设计模式** | 观察者、命令、单例、工厂、策略、适配器、装饰器 |
| **MVVM** | INotifyPropertyChanged、ICommand、DataTemplate、Prism |
| **数据库** | B+Tree 索引、最左前缀、EXPLAIN、连接池、AsNoTracking |
| **网络** | TCP 三次握手、UDP 无连接、Socket 编程 |
| **ORM** | EF6、Dapper、SqlSugar、N+1 问题、Include |

