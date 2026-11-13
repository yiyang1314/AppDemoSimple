using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Rxbit.Brigade.ZcanPro.Views
{
    public class EcuOperationItem
    {
        public int Index { get; set; }
        public string Description { get; set; }
        public string Data { get; set; }
        public bool IsHighlighted { get; set; }
    }

    public partial class EcuFlashEditorView : UserControl
    {
        public ObservableCollection<EcuOperationItem> Operations { get; set; }

        public EcuFlashEditorView()
        {
            InitializeComponent();
            Operations = new ObservableCollection<EcuOperationItem>
            {
                new EcuOperationItem { Index = 1,  Description = "诊断会话控制", Data = "10 03" },
                new EcuOperationItem { Index = 2,  Description = "控制DTC设置", Data = "85 02" },
                new EcuOperationItem { Index = 3,  Description = "通讯控制", Data = "28 03 01" },
                new EcuOperationItem { Index = 4,  Description = "诊断会话控制", Data = "10 02" },
                new EcuOperationItem { Index = 5,  Description = "安全访问", Data = "级别:17 DLL:SeednKey.dll", IsHighlighted = true },
                new EcuOperationItem { Index = 6,  Description = "写指纹信息", Data = "2E F1 84 25 06 12 20 20 20 20 20 20 22" },
                new EcuOperationItem { Index = 7,  Description = "按标识符读取数据", Data = "22 F1 84" },
                new EcuOperationItem { Index = 8,  Description = "文件下载", Data = "文件名:flash_driver_ram_IS_GD.hex" },
                new EcuOperationItem { Index = 9,  Description = "CRC校验", Data = "31 01 F1 A0 C5 93 D0 6A" },
                new EcuOperationItem { Index = 10, Description = "文件下载", Data = "文件名:410TC_A_FCTEOL_AppV1.10.04XCX_78.hex" },
                new EcuOperationItem { Index = 11, Description = "CRC校验", Data = "31 01 F1 A0 D7 9E FA 3D" },
                new EcuOperationItem { Index = 12, Description = "检查程序依赖性", Data = "31 01 FF 01" },
                new EcuOperationItem { Index = 13, Description = "ECU重置", Data = "11 01" },
                new EcuOperationItem { Index = 14, Description = "延时", Data = "1500毫秒" },
                new EcuOperationItem { Index = 15, Description = "诊断会话控制", Data = "10 03" },
                new EcuOperationItem { Index = 16, Description = "通讯控制", Data = "28 00 03" },
                new EcuOperationItem { Index = 17, Description = "控制DTC设置", Data = "85 01" },
                new EcuOperationItem { Index = 18, Description = "诊断会话控制", Data = "10 01" },
                new EcuOperationItem { Index = 19, Description = "清除诊断信息", Data = "14 FF FF FF", IsHighlighted = true },
            };

            OperationList.ItemsSource = Operations;
            OperationList.ItemTemplate = TryFindResource("OperationItemTemplate") as DataTemplate ?? CreateDefaultItemTemplate();
            LoadOperationList();

            Loaded += (s, e) => BuildSecurityAccessForm();
            ServiceTree.SelectedItemChanged += (s, e) =>
            {
                if (e.NewValue is not TreeViewItem tvi) return;

                string header = null;
                if (tvi.Header is string s1)
                    header = s1;
                else if (tvi.Header is Border bd && bd.Child is TextBlock tb1)
                    header = tb1.Text;
                else
                    header = tvi.Header?.ToString();

                SwitchForm(header);
            };
        }

        private DataTemplate CreateDefaultItemTemplate()
        {
            var xaml =
@"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <Grid Height=""22"" x:Name=""row"">
    <Grid.ColumnDefinitions>
      <ColumnDefinition Width=""28""/>
      <ColumnDefinition Width=""*""/>
      <ColumnDefinition Width=""220""/>
    </Grid.ColumnDefinitions>
    <Border Grid.Column=""0"" BorderBrush=""#CCCCCC"" BorderThickness=""0,0,1,1"">
      <TextBlock Text=""{Binding Index}"" FontSize=""11"" Foreground=""#555"" VerticalAlignment=""Center"" HorizontalAlignment=""Center""/>
    </Border>
    <Border Grid.Column=""1"" BorderBrush=""#CCCCCC"" BorderThickness=""0,0,1,1"" Padding=""4,0"">
      <TextBlock Text=""{Binding Description}"" FontSize=""11"" Foreground=""#222"" VerticalAlignment=""Center"" TextTrimming=""CharacterEllipsis""/>
    </Border>
    <Border Grid.Column=""2"" BorderBrush=""#CCCCCC"" BorderThickness=""0,0,0,1"" Padding=""4,0"">
      <TextBlock Text=""{Binding Data}"" FontSize=""11"" Foreground=""#0078D4"" VerticalAlignment=""Center"" TextTrimming=""CharacterEllipsis"" FontFamily=""Consolas""/>
    </Border>
  </Grid>
  <DataTemplate.Triggers>
    <DataTrigger Binding=""{Binding IsHighlighted}"" Value=""True"">
      <Setter TargetName=""row"" Property=""Background"" Value=""#CCE0F5""/>
    </DataTrigger>
  </DataTemplate.Triggers>
</DataTemplate>";
            return System.Windows.Markup.XamlReader.Parse(xaml) as DataTemplate;
        }

        private void LoadOperationList()
        {
            // Already populated via ItemsSource above
        }

        private void SwitchForm(string header)
        {
            EditFormHost.Children.Clear();
            if (string.IsNullOrEmpty(header)) return;
            if (header.Contains("安全访问")) BuildSecurityAccessForm();
            else if (header.Contains("文件下载")) BuildFileDownloadForm();
            else if (header.Contains("周期性标识符读取")) BuildPeriodicReadForm();
            else if (header.Contains("按标识符写数据")) BuildWriteDataForm();
            else if (header.Contains("按标识符读取数据") && !header.Contains("周期") && !header.Contains("映像")) BuildReadDataByIdForm();
            else if (header.Contains("按地址读取内容")) BuildReadByAddressForm();
            else if (header.Contains("诊断会话控制")) BuildSessionControlForm();
            else if (header.Contains("通讯控制")) BuildCommControlForm();
            else if (header.Contains("控制DTC设置")) BuildDtcControlForm();
            else if (header.Contains("清除诊断信息")) BuildClearDtcForm();
            else if (header.Contains("ECU重置")) BuildEcuResetForm();
            else if (header.Contains("CRC") || header.Contains("例程控制")) BuildRoutineControlForm();
            else BuildGenericForm(header);
        }

        // ---- 通用表单构建辅助 ----
        private Border GroupBox(string title, UIElement content, int mt = 8)
        {
            var borderBrush = TryFindResource("EcuBorder") as Brush ?? new SolidColorBrush(Color.FromRgb(0xC0, 0xC4, 0xCC));
            var bd = new Border
            {
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(1),
                Background = Brushes.White,
                Margin = new Thickness(0, mt, 0, 0),
                Padding = new Thickness(8)
            };
            var sp = new StackPanel();
            var h = new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0x2B, 0x57, 0x9A)),
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 6)
            };
            sp.Children.Add(h);
            if (content != null) sp.Children.Add(content);
            bd.Child = sp;
            return bd;
        }

        private Grid FieldRow(string label, params UIElement[] controls)
        {
            var g = new Grid { Margin = new Thickness(0, 4, 0, 4) };
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var tb = new TextBlock
            {
                Text = label,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33))
            };
            Grid.SetColumn(tb, 0);
            g.Children.Add(tb);
            var host = new WrapPanel { VerticalAlignment = VerticalAlignment.Center };
            foreach (var c in controls) host.Children.Add(c);
            Grid.SetColumn(host, 1);
            g.Children.Add(host);
            return g;
        }

        private TextBox Tb(string text = "", double w = double.NaN)
        {
            var t = new TextBox { Text = text, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 4, 0) };
            if (!double.IsNaN(w)) t.Width = w;
            return t;
        }

        private ComboBox Cb(string[] items, int sel = 0, double w = double.NaN)
        {
            var c = new ComboBox { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 4, 0) };
            foreach (var i in items) c.Items.Add(i);
            c.SelectedIndex = sel;
            if (!double.IsNaN(w)) c.Width = w;
            return c;
        }

        private void AddBtn(object panel, string content, RoutedEventHandler click = null, bool primary = false)
        {
            var sp = panel as Panel;
            var styleKey = primary ? "EcuPrimaryBtn" : "EcuBtn";
            var btn = new Button
            {
                Content = content,
                Style = TryFindResource(styleKey) as Style,
                Margin = new Thickness(6, 12, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            if (click != null) btn.Click += click;
            var w = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Right };
            w.Children.Add(btn);
            sp.Children.Add(w);
        }

        // ---- 具体表单 ----
        private void BuildSecurityAccessForm()
        {
            var sp = new StackPanel();
            // 描述区
            var descF = new StackPanel();
            descF.Children.Add(FieldRow("描述：", Tb("安全访问", 300)));
            descF.Children.Add(FieldRow("地址", new TextBlock { Text = "请求地址：", VerticalAlignment = VerticalAlignment.Center, FontSize = 12 },
                Cb(new[] { "物理地址", "功能地址" }, 0, 140)));
            sp.Children.Add(GroupBox("基本信息", descF, 0));

            // 安全访问区
            var sa = new StackPanel();
            sa.Children.Add(FieldRow("安全级别：", Tb("1", 80), new CheckBox { Content = "忽略已解锁检查" }));
            sa.Children.Add(FieldRow("安全算法链接库路径：",
                new TextBox { Width = 260, Text = "", Margin = new Thickness(0, 0, 4, 0) },
                new Button { Content = "...", Style = TryFindResource("EcuBtn") as Style, Width = 30 }));
            sp.Children.Add(GroupBox("安全访问", sa));

            AddBtn(sp, "添加到列表");
            EditFormHost.Children.Add(sp);
        }

        private void BuildFileDownloadForm()
        {
            var sp = new StackPanel();
            sp.Children.Add(GroupBox("基本信息", WrapFields(
                FieldRow("描述：", Tb("文件下载", 300)),
                FieldRow("地址", new TextBlock { Text = "请求地址：", VerticalAlignment = VerticalAlignment.Center, FontSize = 12 },
                    Cb(new[] { "物理地址", "功能地址" }, 0, 140))
            ), 0));

            var fd = new StackPanel();
            fd.Children.Add(FieldRow("擦除内存：", Cb(new[] { "不擦除", "擦除整个分区", "按地址范围擦除" }, 0, 140),
                new TextBlock { Text = "$34请求参数：", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 4, 0) }, Tb("00", 50)));
            fd.Children.Add(FieldRow("块延时(ms)：", Tb("0", 80),
                new TextBlock { Text = "地址大小：", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 4, 0) },
                Cb(new[] { "1", "2", "4" }, 0, 70),
                new TextBlock { Text = "长度大小：", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 4, 0) },
                Cb(new[] { "1", "2", "4" }, 0, 70)));
            fd.Children.Add(FieldRow("文件路径：",
                new TextBox { Width = 260, Text = "", Margin = new Thickness(0, 0, 4, 0) },
                new Button { Content = "...", Style = TryFindResource("EcuBtn") as Style, Width = 30 }));
            sp.Children.Add(GroupBox("文件下载", fd));

            var crc1 = new StackPanel();
            crc1.Children.Add(FieldRow("CRC 类型：", Cb(new[] { "CRC32", "CRC16", "CRC8" }, 0, 140),
                new TextBlock { Text = "多项式：", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 4, 0) },
                Tb("0x4C11DB7", 140)));
            crc1.Children.Add(FieldRow("初始值：", Tb("0xFFFFFFFF", 140),
                new TextBlock { Text = "输出异或值：", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 4, 0) },
                Tb("0xFFFFFFFF", 140)));
            crc1.Children.Add(new WrapPanel { Margin = new Thickness(0, 4, 0, 0), Children = {
                new CheckBox { Content = "输入反转", IsChecked = true, Margin = new Thickness(0,0,14,0) },
                new CheckBox { Content = "输出反转", IsChecked = true }
            }});
            sp.Children.Add(GroupBox("CRC 校验算法", crc1));

            var crc2 = new StackPanel();
            crc2.Children.Add(new CheckBox { Content = "每个文件块校验  $37服务  +  块校验码", Margin = new Thickness(0, 2, 0, 2) });
            crc2.Children.Add(new CheckBox { Content = "每个文件块校验  31 01 F1 A0  +  块校验码", Margin = new Thickness(0, 2, 0, 2) });
            crc2.Children.Add(new CheckBox { Content = "总校验指令  31 01 02 02  +  D7 9E FA 3D", IsChecked = true, Margin = new Thickness(0, 2, 0, 2) });
            sp.Children.Add(GroupBox("CRC 校验方式", crc2));

            AddBtn(sp, "添加到列表");
            EditFormHost.Children.Add(sp);
        }

        private void BuildPeriodicReadForm()
        {
            var sp = new StackPanel();
            sp.Children.Add(GroupBox("基本信息", WrapFields(
                FieldRow("描述：", Tb("按周期性标识符读取数据", 300)),
                FieldRow("地址", new TextBlock { Text = "请求地址：", VerticalAlignment = VerticalAlignment.Center, FontSize = 12 },
                    Cb(new[] { "物理地址", "功能地址" }, 0, 140))
            ), 0));

            var req = new StackPanel();
            req.Children.Add(FieldRow("服务ID(0x)：", Tb("2A", 80)));
            req.Children.Add(FieldRow("子服务：", Cb(new[] { "发送定时数据", "停止发送", "请求记录", "请求分配" }, 0, 200)));
            req.Children.Add(FieldRow("抑制响应：", Cb(new[] { "否", "是" }, 0, 100)));
            req.Children.Add(FieldRow("数据(0x)：", new TextBox { Width = 320, Text = "", Height = 60, AcceptsReturn = true, VerticalContentAlignment = VerticalAlignment.Top, TextWrapping = TextWrapping.Wrap }));
            sp.Children.Add(GroupBox("请求", req));

            AddBtn(sp, "添加到列表");
            EditFormHost.Children.Add(sp);
        }

        private void BuildWriteDataForm()
        {
            var sp = new StackPanel();
            sp.Children.Add(GroupBox("基本信息", WrapFields(
                FieldRow("描述：", Tb("按标识符写数据", 300)),
                FieldRow("地址", new TextBlock { Text = "请求地址：", VerticalAlignment = VerticalAlignment.Center, FontSize = 12 },
                    Cb(new[] { "物理地址", "功能地址" }, 0, 140))
            ), 0));
            var req = new StackPanel();
            req.Children.Add(FieldRow("数据标识符(0x)：", Tb("F1 84", 120)));
            req.Children.Add(FieldRow("数据(0x)：", new TextBox { Width = 320, Text = "25 06 12 20 20 20 20 20 20 22", Height = 60, AcceptsReturn = true, VerticalContentAlignment = VerticalAlignment.Top }));
            req.Children.Add(new WrapPanel { Children = { new CheckBox { Content = "抑制正响应" } } });
            sp.Children.Add(GroupBox("按标识符写数据", req));
            AddBtn(sp, "添加到列表");
            EditFormHost.Children.Add(sp);
        }

        private void BuildReadDataByIdForm()
        {
            var sp = new StackPanel();
            sp.Children.Add(GroupBox("基本信息", WrapFields(
                FieldRow("描述：", Tb("按标识符读取数据", 300)),
                FieldRow("地址", new TextBlock { Text = "请求地址：", VerticalAlignment = VerticalAlignment.Center, FontSize = 12 },
                    Cb(new[] { "物理地址", "功能地址" }, 0, 140))
            ), 0));
            var req = new StackPanel();
            req.Children.Add(FieldRow("数据标识符(0x)：", Tb("F1 84", 120)));
            req.Children.Add(new WrapPanel { Children = { new CheckBox { Content = "抑制正响应" } } });
            sp.Children.Add(GroupBox("请求", req));
            AddBtn(sp, "添加到列表");
            EditFormHost.Children.Add(sp);
        }

        private void BuildReadByAddressForm()
        {
            var sp = new StackPanel();
            sp.Children.Add(GroupBox("基本信息", WrapFields(
                FieldRow("描述：", Tb("按地址读取内容", 300)),
                FieldRow("地址", new TextBlock { Text = "请求地址：", VerticalAlignment = VerticalAlignment.Center, FontSize = 12 },
                    Cb(new[] { "物理地址", "功能地址" }, 0, 140))
            ), 0));
            var req = new StackPanel();
            req.Children.Add(FieldRow("地址大小：", Cb(new[] { "1", "2", "4" }, 2, 100),
                new TextBlock { Text = "内存大小：", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 4, 0) },
                Cb(new[] { "1", "2", "4" }, 0, 100)));
            req.Children.Add(FieldRow("起始地址(0x)：", Tb("00000000", 160)));
            req.Children.Add(FieldRow("数据长度(0x)：", Tb("100", 100)));
            sp.Children.Add(GroupBox("按地址读取", req));
            AddBtn(sp, "添加到列表");
            EditFormHost.Children.Add(sp);
        }

        private void BuildSessionControlForm()
        {
            var sp = new StackPanel();
            sp.Children.Add(GroupBox("基本信息", WrapFields(
                FieldRow("描述：", Tb("诊断会话控制", 300)),
                FieldRow("地址", new TextBlock { Text = "请求地址：", VerticalAlignment = VerticalAlignment.Center, FontSize = 12 },
                    Cb(new[] { "物理地址", "功能地址" }, 0, 140))
            ), 0));
            var req = new StackPanel();
            req.Children.Add(FieldRow("子功能：", Cb(new[] { "01 - 默认会话", "02 - 编程会话", "03 - 扩展诊断会话", "04 - 安全系统诊断会话" }, 2, 240)));
            sp.Children.Add(GroupBox("诊断会话控制", req));
            AddBtn(sp, "添加到列表");
            EditFormHost.Children.Add(sp);
        }

        private void BuildCommControlForm()
        {
            var sp = new StackPanel();
            sp.Children.Add(GroupBox("基本信息", WrapFields(
                FieldRow("描述：", Tb("通讯控制", 300)),
                FieldRow("地址", new TextBlock { Text = "请求地址：", VerticalAlignment = VerticalAlignment.Center, FontSize = 12 },
                    Cb(new[] { "物理地址", "功能地址" }, 0, 140))
            ), 0));
            var req = new StackPanel();
            req.Children.Add(FieldRow("控制类型：", Cb(new[] { "00 - 使能接收和发送", "01 - 使能接收、禁止发送", "02 - 禁止接收、使能发送", "03 - 禁止接收和发送" }, 3, 260)));
            req.Children.Add(FieldRow("通信类型：", Tb("01", 80)));
            sp.Children.Add(GroupBox("通讯控制", req));
            AddBtn(sp, "添加到列表");
            EditFormHost.Children.Add(sp);
        }

        private void BuildDtcControlForm()
        {
            var sp = new StackPanel();
            sp.Children.Add(GroupBox("基本信息", WrapFields(
                FieldRow("描述：", Tb("控制DTC设置", 300)),
                FieldRow("地址", new TextBlock { Text = "请求地址：", VerticalAlignment = VerticalAlignment.Center, FontSize = 12 },
                    Cb(new[] { "物理地址", "功能地址" }, 0, 140))
            ), 0));
            var req = new StackPanel();
            req.Children.Add(FieldRow("子功能：", Cb(new[] { "01 - 开", "02 - 关" }, 1, 160)));
            sp.Children.Add(GroupBox("控制 DTC 设置", req));
            AddBtn(sp, "添加到列表");
            EditFormHost.Children.Add(sp);
        }

        private void BuildClearDtcForm()
        {
            var sp = new StackPanel();
            sp.Children.Add(GroupBox("基本信息", WrapFields(
                FieldRow("描述：", Tb("清除诊断信息", 300)),
                FieldRow("地址", new TextBlock { Text = "请求地址：", VerticalAlignment = VerticalAlignment.Center, FontSize = 12 },
                    Cb(new[] { "物理地址", "功能地址" }, 0, 140))
            ), 0));
            var req = new StackPanel();
            req.Children.Add(FieldRow("DTC 组掩码(0x)：", Tb("FF FF FF", 160)));
            sp.Children.Add(GroupBox("清除诊断信息", req));
            AddBtn(sp, "添加到列表");
            EditFormHost.Children.Add(sp);
        }

        private void BuildEcuResetForm()
        {
            var sp = new StackPanel();
            sp.Children.Add(GroupBox("基本信息", WrapFields(
                FieldRow("描述：", Tb("ECU重置", 300)),
                FieldRow("地址", new TextBlock { Text = "请求地址：", VerticalAlignment = VerticalAlignment.Center, FontSize = 12 },
                    Cb(new[] { "物理地址", "功能地址" }, 0, 140))
            ), 0));
            var req = new StackPanel();
            req.Children.Add(FieldRow("重置类型：", Cb(new[] { "01 - 硬重置", "02 - 键断开重置", "03 - 软重置", "04 - 启用/禁用快速断电" }, 0, 260)));
            sp.Children.Add(GroupBox("ECU 重置", req));
            AddBtn(sp, "添加到列表");
            EditFormHost.Children.Add(sp);
        }

        private void BuildRoutineControlForm()
        {
            var sp = new StackPanel();
            sp.Children.Add(GroupBox("基本信息", WrapFields(
                FieldRow("描述：", Tb("CRC校验 / 例程控制", 300)),
                FieldRow("地址", new TextBlock { Text = "请求地址：", VerticalAlignment = VerticalAlignment.Center, FontSize = 12 },
                    Cb(new[] { "物理地址", "功能地址" }, 0, 140))
            ), 0));
            var req = new StackPanel();
            req.Children.Add(FieldRow("例程控制类型：", Cb(new[] { "01 - 启动例程", "02 - 停止例程", "03 - 请求例程结果" }, 0, 200)));
            req.Children.Add(FieldRow("例程标识符(0x)：", Tb("F1 A0", 120)));
            req.Children.Add(FieldRow("数据(0x)：", new TextBox { Width = 320, Text = "C5 93 D0 6A", Height = 48, AcceptsReturn = true, VerticalContentAlignment = VerticalAlignment.Top }));
            sp.Children.Add(GroupBox("例程控制 / CRC 校验", req));
            AddBtn(sp, "添加到列表");
            EditFormHost.Children.Add(sp);
        }

        private void BuildGenericForm(string title)
        {
            var sp = new StackPanel();
            sp.Children.Add(GroupBox("基本信息", WrapFields(
                FieldRow("描述：", Tb(title.Replace("(", "").Replace(")", ""), 300)),
                FieldRow("地址", new TextBlock { Text = "请求地址：", VerticalAlignment = VerticalAlignment.Center, FontSize = 12 },
                    Cb(new[] { "物理地址", "功能地址" }, 0, 140))
            ), 0));
            var req = new StackPanel();
            req.Children.Add(FieldRow("数据(0x)：", new TextBox { Width = 320, Text = "", Height = 80, AcceptsReturn = true, VerticalContentAlignment = VerticalAlignment.Top, TextWrapping = TextWrapping.Wrap }));
            sp.Children.Add(GroupBox("请求参数", req));
            AddBtn(sp, "添加到列表");
            EditFormHost.Children.Add(sp);
        }

        private StackPanel WrapFields(params UIElement[] fields)
        {
            var s = new StackPanel();
            foreach (var f in fields) s.Children.Add(f);
            return s;
        }
    }
}
