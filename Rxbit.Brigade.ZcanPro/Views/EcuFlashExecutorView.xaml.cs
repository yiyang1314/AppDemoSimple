using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Rxbit.Brigade.ZcanPro.Views
{
    public class EcuStepItem
    {
        public int Index { get; set; }
        public string Title { get; set; }
        public string SubData { get; set; }
    }

    public class EcuFileBlockItem
    {
        public string BlockName { get; set; }
        public string Address { get; set; }
        public long Length { get; set; }
        public int Progress { get; set; }
    }

    public class EcuLogItem
    {
        public string Time { get; set; }
        public string Type { get; set; }
        public string Id { get; set; }
        public string Direction { get; set; }
        public string Length { get; set; }
        public string Data { get; set; }
        public Brush TypeBrush => (Type ?? "") switch
        {
            "发送" => new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xD4)),
            "接收" => new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32)),
            "报错" => new SolidColorBrush(Color.FromRgb(0xC6, 0x28, 0x28)),
            "信息" => new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80)),
            _ => new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33))
        };
    }

    public partial class EcuFlashExecutorView : UserControl
    {
        public ObservableCollection<EcuStepItem> Steps { get; set; }
        public ObservableCollection<EcuFileBlockItem> Blocks { get; set; }
        public ObservableCollection<EcuLogItem> Logs { get; set; }

        public EcuFlashExecutorView()
        {
            InitializeComponent();
            Steps = new ObservableCollection<EcuStepItem>
            {
                new EcuStepItem { Index = 1, Title = "诊断会话控制", SubData = "数据[10 03 ]" },
                new EcuStepItem { Index = 2, Title = "控制DTC设置", SubData = "数据[85 02 ]" },
                new EcuStepItem { Index = 3, Title = "通讯控制", SubData = "数据[28 03 01 ]" },
                new EcuStepItem { Index = 4, Title = "诊断会话控制", SubData = "数据[10 02 ]" },
                new EcuStepItem { Index = 5, Title = "安全访问", SubData = "级别:17 DLL:SeednKey.dll" },
                new EcuStepItem { Index = 6, Title = "写指纹信息", SubData = "数据[2E F1 84 25 06 12 20 20 2" },
                new EcuStepItem { Index = 7, Title = "按标识符读取数据", SubData = "数据[22 F1 84 ]" },
                new EcuStepItem { Index = 8, Title = "文件下载", SubData = "文件名:flash_driver_ram_IS_GD.hex" },
                new EcuStepItem { Index = 9, Title = "CRC校验", SubData = "数据[31 01 F1 A0 C5 93 D0 6A]" },
                new EcuStepItem { Index = 10, Title = "文件下载", SubData = "文件名:410TC_A_FCTEOL_AppV1.10" },
                new EcuStepItem { Index = 11, Title = "CRC校验", SubData = "数据[31 01 F1 A0 D7 9E FA 3D]" },
                new EcuStepItem { Index = 12, Title = "检查程序依赖性", SubData = "" },
            };
            Blocks = new ObservableCollection<EcuFileBlockItem>
            {
                new EcuFileBlockItem { BlockName = "文件_1_块_1", Address = "地址:0x10250000", Length = 67948, Progress = 0 },
                new EcuFileBlockItem { BlockName = "文件_2_块_1", Address = "地址:0x00280000", Length = 584929, Progress = 0 },
            };
            Logs = new ObservableCollection<EcuLogItem>
            {
                new EcuLogItem { Time = "16:22:03.112", Type = "信息", Id = "--", Direction = "--", Length = "--", Data = "初始化 ECU Flash 执行器" },
                new EcuLogItem { Time = "16:22:03.118", Type = "信息", Id = "--", Direction = "--", Length = "--", Data = "加载刷写方案文件：Public2944_APP_UP.zflash" },
                new EcuLogItem { Time = "16:22:03.201", Type = "发送", Id = "7DF", Direction = "→", Length = "8", Data = "02 10 03 AA AA AA AA AA" },
                new EcuLogItem { Time = "16:22:03.214", Type = "接收", Id = "791", Direction = "←", Length = "7", Data = "06 50 03 00 32 01 F4" },
                new EcuLogItem { Time = "16:22:03.260", Type = "发送", Id = "7DF", Direction = "→", Length = "8", Data = "02 85 02 AA AA AA AA AA" },
                new EcuLogItem { Time = "16:22:03.274", Type = "接收", Id = "791", Direction = "←", Length = "3", Data = "02 C5 02" },
            };

            StepItemsControl.ItemsSource = Steps;
            StepItemsControl.ItemTemplate = BuildStepTemplate();

            FileBlockItemsControl.ItemsSource = Blocks;
            FileBlockItemsControl.ItemTemplate = BuildFileBlockTemplate();

            LogItemsControl.ItemsSource = Logs;
            LogItemsControl.ItemTemplate = BuildLogTemplate();
        }

        private DataTemplate BuildStepTemplate()
        {
            var xaml = @"
<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
              xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <Border Background=""#5F7FAE"" BorderBrush=""#2B579A"" BorderThickness=""0,0,0,1"" Padding=""8,6"" Margin=""0,0"">
    <StackPanel>
      <StackPanel Orientation=""Horizontal"">
        <TextBlock Text=""{Binding Index}"" Foreground=""White"" FontSize=""13"" FontWeight=""Bold"" VerticalAlignment=""Center"" />
        <TextBlock Text="".  "" Foreground=""White"" FontSize=""13"" />
        <TextBlock Text=""{Binding Title}"" Foreground=""White"" FontSize=""12"" FontWeight=""Bold"" VerticalAlignment=""Center"" TextTrimming=""CharacterEllipsis""/>
      </StackPanel>
      <TextBlock Text=""{Binding SubData}"" Foreground=""#DCE5F1"" FontSize=""11"" Margin=""0,3,0,0"" TextTrimming=""CharacterEllipsis"" FontFamily=""Consolas""/>
    </StackPanel>
  </Border>
</DataTemplate>";
            return System.Windows.Markup.XamlReader.Parse(xaml) as DataTemplate;
        }

        private DataTemplate BuildFileBlockTemplate()
        {
            var xaml = @"
<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
              xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <Border Background=""#5F7FAE"" Padding=""10,8"" Margin=""0,0,0,6"" CornerRadius=""2"">
    <Grid>
      <Grid.RowDefinitions>
        <RowDefinition Height=""Auto""/>
        <RowDefinition Height=""Auto""/>
        <RowDefinition Height=""Auto""/>
      </Grid.RowDefinitions>
      <Grid.ColumnDefinitions>
        <ColumnDefinition Width=""*""/>
        <ColumnDefinition Width=""50""/>
      </Grid.ColumnDefinitions>
      <TextBlock Grid.Row=""0"" Grid.Column=""0"" Text=""{Binding BlockName}"" Foreground=""White"" FontSize=""13"" FontWeight=""Bold""/>
      <TextBlock Grid.Row=""0"" Grid.Column=""1"" Text=""{Binding Progress, StringFormat='{}{0}%'}"" Foreground=""White"" FontSize=""16"" FontWeight=""Bold"" HorizontalAlignment=""Right""/>
      <TextBlock Grid.Row=""1"" Grid.Column=""0"" Grid.ColumnSpan=""2"" Text=""{Binding Address}"" Foreground=""#DCE5F1"" FontSize=""11"" Margin=""0,3,0,0""/>
      <Grid Grid.Row=""2"" Grid.Column=""0"" Grid.ColumnSpan=""2"" Margin=""0,5,0,0"">
        <Border Background=""#3C5A80"" Height=""4"" CornerRadius=""2""/>
        <Border Background=""#00E080"" HorizontalAlignment=""Left"" Height=""4"" CornerRadius=""2""
                Width=""{Binding Progress}"" MaxWidth=""{Binding ActualWidth, RelativeSource={RelativeSource AncestorType=Grid}}""/>
      </Grid>
    </Grid>
  </Border>
</DataTemplate>";
            return System.Windows.Markup.XamlReader.Parse(xaml) as DataTemplate;
        }

        private DataTemplate BuildLogTemplate()
        {
            var xaml = @"
<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
              xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <Grid Height=""20"">
    <Grid.Style>
      <Style TargetType=""Grid"">
        <Setter Property=""Background"" Value=""White""/>
        <Style.Triggers>
          <Trigger Property=""IsMouseOver"" Value=""True""><Setter Property=""Background"" Value=""#EFF6FF""/></Trigger>
        </Style.Triggers>
      </Style>
    </Grid.Style>
    <Grid.ColumnDefinitions>
      <ColumnDefinition Width=""120""/>
      <ColumnDefinition Width=""60""/>
      <ColumnDefinition Width=""70""/>
      <ColumnDefinition Width=""50""/>
      <ColumnDefinition Width=""50""/>
      <ColumnDefinition Width=""*""/>
    </Grid.ColumnDefinitions>
    <Border Grid.Column=""0"" BorderBrush=""#DCE5F1"" BorderThickness=""0,0,1,1"" Padding=""4,2"">
      <TextBlock Text=""{Binding Time}"" FontFamily=""Consolas"" FontSize=""11"" Foreground=""#333""/>
    </Border>
    <Border Grid.Column=""1"" BorderBrush=""#DCE5F1"" BorderThickness=""0,0,1,1"" Padding=""4,2"">
      <TextBlock Text=""{Binding Type}"" FontSize=""11"" Foreground=""{Binding TypeBrush}"" FontWeight=""SemiBold""/>
    </Border>
    <Border Grid.Column=""2"" BorderBrush=""#DCE5F1"" BorderThickness=""0,0,1,1"" Padding=""4,2"">
      <TextBlock Text=""{Binding Id}"" FontFamily=""Consolas"" FontSize=""11"" Foreground=""#0078D4"" FontWeight=""Bold""/>
    </Border>
    <Border Grid.Column=""3"" BorderBrush=""#DCE5F1"" BorderThickness=""0,0,1,1"" Padding=""4,2"">
      <TextBlock Text=""{Binding Direction}"" FontSize=""11"" Foreground=""#666"" HorizontalAlignment=""Center""/>
    </Border>
    <Border Grid.Column=""4"" BorderBrush=""#DCE5F1"" BorderThickness=""0,0,1,1"" Padding=""4,2"">
      <TextBlock Text=""{Binding Length}"" FontSize=""11"" Foreground=""#333"" HorizontalAlignment=""Center""/>
    </Border>
    <Border Grid.Column=""5"" BorderBrush=""#DCE5F1"" BorderThickness=""0,0,0,1"" Padding=""4,2"">
      <TextBlock Text=""{Binding Data}"" FontFamily=""Consolas"" FontSize=""11"" Foreground=""#222"" TextTrimming=""CharacterEllipsis""/>
    </Border>
  </Grid>
</DataTemplate>";
            return System.Windows.Markup.XamlReader.Parse(xaml) as DataTemplate;
        }
    }
}
