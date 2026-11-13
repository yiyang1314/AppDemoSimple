using System.Collections.ObjectModel;
using Rxbit.Brigade.Controll.Mvvm;

namespace Rxbit.Brigade.Controll.ViewModels
{
    // 商店导航项
    public class ShopNavItem : ObservableObject
    {
        private string _icon;
        private string _label;
        private bool _isActive;
        private string _target;
        private string _badge;

        public string Icon { get => _icon; set { SetProperty(ref _icon, value); } }
        public string Label { get => _label; set { SetProperty(ref _label, value); } }
        public bool IsActive { get => _isActive; set { SetProperty(ref _isActive, value); } }
        public string Target { get => _target; set { SetProperty(ref _target, value); } }
        public string Badge { get => _badge; set { SetProperty(ref _badge, value); } }
    }

    // 应用项
    public class ShopAppItem : ObservableObject
    {
        private string _name;
        private string _icon;
        private string _downloads;
        private string _size;
        private string _version;
        private string _installDate;
        private string _category;
        private string _desc;

        public string Name { get => _name; set { SetProperty(ref _name, value); } }
        public string Icon { get => _icon; set { SetProperty(ref _icon, value); } }
        public string Downloads { get => _downloads; set { SetProperty(ref _downloads, value); } }
        public string Size { get => _size; set { SetProperty(ref _size, value); } }
        public string Version { get => _version; set { SetProperty(ref _version, value); } }
        public string InstallDate { get => _installDate; set { SetProperty(ref _installDate, value); } }
        public string Category { get => _category; set { SetProperty(ref _category, value); } }
        public string Desc { get => _desc; set { SetProperty(ref _desc, value); } }
    }

    // 分类筛选项
    public class ShopFilterItem : ObservableObject
    {
        private string _label;
        private bool _isActive;

        public string Label { get => _label; set { SetProperty(ref _label, value); } }
        public bool IsActive { get => _isActive; set { SetProperty(ref _isActive, value); } }
    }

    // Banner项
    public class ShopBannerItem : ObservableObject
    {
        private string _title;
        private string _subtitle;
        private string _actionText;
        private string _bgColor;

        public string Title { get => _title; set { SetProperty(ref _title, value); } }
        public string Subtitle { get => _subtitle; set { SetProperty(ref _subtitle, value); } }
        public string ActionText { get => _actionText; set { SetProperty(ref _actionText, value); } }
        public string BgColor { get => _bgColor; set { SetProperty(ref _bgColor, value); } }
    }

    // 商店Shell ViewModel
    public class ShopShellViewModel : ObservableObject
    {
        private ObservableCollection<ShopNavItem> _menus;
        private string _current;
        private string _searchText;

        public ObservableCollection<ShopNavItem> Menus { get => _menus; set { SetProperty(ref _menus, value); } }
        public string Current { get => _current; set { SetProperty(ref _current, value); } }
        public string SearchText { get => _searchText; set { SetProperty(ref _searchText, value); } }

        public ShopShellViewModel()
        {
            Menus = new ObservableCollection<ShopNavItem>
            {
                new ShopNavItem { Icon = "🏠", Label = "推荐", Target = "shop1", IsActive = true },
                new ShopNavItem { Icon = "🎯", Label = "无畏契约", Target = "shop2" },
                new ShopNavItem { Icon = "🎮", Label = "游戏", Target = "game" },
                new ShopNavItem { Icon = "💬", Label = "微信小游戏", Target = "wxgame", Badge = "HOT" },
                new ShopNavItem { Icon = "📂", Label = "分类", Target = "category" },
                new ShopNavItem { Icon = "🖼", Label = "动态壁纸", Target = "shop5" },
                new ShopNavItem { Icon = "🤖", Label = "AI 专区", Target = "ai" },
                new ShopNavItem { Icon = "💼", Label = "办公", Target = "shop4" },
                new ShopNavItem { Icon = "⬆", Label = "升级", Target = "upgrade", Badge = "5" },
                new ShopNavItem { Icon = "👤", Label = "我的", Target = "shop3" },
            };
            Current = "shop1";
            SearchText = "";
        }

        public void Navigate(string target)
        {
            Current = target;
            foreach (var m in Menus)
                m.IsActive = m.Target == target;
        }
    }

    // Shop1 - 分类/推荐页（应用网格）
    public class Shop1ViewModel : ObservableObject
    {
        private ObservableCollection<ShopFilterItem> _tabFilters;
        private ObservableCollection<ShopFilterItem> _categoryFilters;
        private ObservableCollection<ShopFilterItem> _tagFilters;
        private ObservableCollection<ShopAppItem> _apps;

        public ObservableCollection<ShopFilterItem> TabFilters { get => _tabFilters; set { SetProperty(ref _tabFilters, value); } }
        public ObservableCollection<ShopFilterItem> CategoryFilters { get => _categoryFilters; set { SetProperty(ref _categoryFilters, value); } }
        public ObservableCollection<ShopFilterItem> TagFilters { get => _tagFilters; set { SetProperty(ref _tagFilters, value); } }
        public ObservableCollection<ShopAppItem> Apps { get => _apps; set { SetProperty(ref _apps, value); } }

        public Shop1ViewModel()
        {
            TabFilters = new ObservableCollection<ShopFilterItem>
            {
                new ShopFilterItem { Label = "软件", IsActive = true },
                new ShopFilterItem { Label = "游戏" },
            };

            CategoryFilters = new ObservableCollection<ShopFilterItem>
            {
                new ShopFilterItem { Label = "PC" },
                new ShopFilterItem { Label = "安卓" },
                new ShopFilterItem { Label = "网页" },
            };

            TagFilters = new ObservableCollection<ShopFilterItem>
            {
                new ShopFilterItem { Label = "全部", IsActive = true },
                new ShopFilterItem { Label = "聊天社交" },
                new ShopFilterItem { Label = "输入法" },
                new ShopFilterItem { Label = "浏览器" },
                new ShopFilterItem { Label = "下载工具" },
                new ShopFilterItem { Label = "视频软件" },
                new ShopFilterItem { Label = "音乐软件" },
                new ShopFilterItem { Label = "图形图片" },
                new ShopFilterItem { Label = "安全防护" },
                new ShopFilterItem { Label = "解压刻录" },
                new ShopFilterItem { Label = "系统工具" },
                new ShopFilterItem { Label = "驱动工具" },
                new ShopFilterItem { Label = "办公教育" },
                new ShopFilterItem { Label = "编程软件" },
                new ShopFilterItem { Label = "理财金融" },
                new ShopFilterItem { Label = "助手工具" },
                new ShopFilterItem { Label = "桌面美化" },
                new ShopFilterItem { Label = "智能生成" },
                new ShopFilterItem { Label = "其他应用" },
            };

            Apps = new ObservableCollection<ShopAppItem>
            {
                new ShopAppItem { Name = "芒果TV", Icon = "🥭", Downloads = "3.878亿万次安装" },
                new ShopAppItem { Name = "微信电脑版", Icon = "💚", Downloads = "2.95亿次安装" },
                new ShopAppItem { Name = "元气桌面壁纸", Icon = "🌸", Downloads = "495万次安装" },
                new ShopAppItem { Name = "豆包", Icon = "🫘", Downloads = "412万次安装" },
                new ShopAppItem { Name = "美图秀秀: AI图片与...", Icon = "🎨", Downloads = "1377万次安装" },
                new ShopAppItem { Name = "腾讯视频", Icon = "🎬", Downloads = "1.59亿次安装" },
                new ShopAppItem { Name = "搜狗拼音输入法", Icon = "⌨", Downloads = "1.5亿次安装" },
                new ShopAppItem { Name = "腾讯会议", Icon = "📹", Downloads = "1.36亿次安装" },
                new ShopAppItem { Name = "谷歌浏览器: 稳定版...", Icon = "🌐", Downloads = "2007万次安装" },
                new ShopAppItem { Name = "WPS Office", Icon = "📝", Downloads = "1.09亿次安装" },
                new ShopAppItem { Name = "QQ音乐", Icon = "🎵", Downloads = "1.07亿次安装" },
                new ShopAppItem { Name = "夸克", Icon = "🔍", Downloads = "1922万次安装" },
                new ShopAppItem { Name = "爱奇艺", Icon = "🎞", Downloads = "8.878亿万次安装" },
                new ShopAppItem { Name = "火绒安全软件", Icon = "🛡", Downloads = "8531万次安装" },
                new ShopAppItem { Name = "百度网盘", Icon = "☁", Downloads = "7751万次安装" },
                new ShopAppItem { Name = "Microsoft Edge", Icon = "🌀", Downloads = "1337万次安装" },
                new ShopAppItem { Name = "钉钉", Icon = "📌", Downloads = "2623万次安装" },
                new ShopAppItem { Name = "企业微信: 智慧办公公...", Icon = "💼", Downloads = "3443万次安装" },
                new ShopAppItem { Name = "元宝", Icon = "💰", Downloads = "617万次安装" },
                new ShopAppItem { Name = "ToDesk", Icon = "🖥", Downloads = "896万次安装" },
                new ShopAppItem { Name = "迅雷", Icon = "⚡", Downloads = "3773万次安装" },
                new ShopAppItem { Name = "驾驶专业版", Icon = "🚗", Downloads = "4519万次安装" },
                new ShopAppItem { Name = "汽水音乐", Icon = "🥤", Downloads = "809万次安装" },
                new ShopAppItem { Name = "优酷-票务: 正在...", Icon = "🎥", Downloads = "5161万次安装" },
                new ShopAppItem { Name = "谷歌浏览器: 稳定版", Icon = "🌐", Downloads = "207万次安装" },
                new ShopAppItem { Name = "7-Zip", Icon = "📦", Downloads = "1314万次安装" },
                new ShopAppItem { Name = "QQ (怀旧版)", Icon = "🐧", Downloads = "2344万次安装" },
                new ShopAppItem { Name = "向日葵远程控制", Icon = "🌻", Downloads = "1204万次安装" },
                new ShopAppItem { Name = "360压缩", Icon = "📋", Downloads = "1972万次安装" },
                new ShopAppItem { Name = "联想电脑管家", Icon = "🔧", Downloads = "6123万次安装" },
                new ShopAppItem { Name = "夸克网盘", Icon = "☁", Downloads = "1816万次安装" },
                new ShopAppItem { Name = "爱思助手", Icon = "📱", Downloads = "1730万次安装" },
                new ShopAppItem { Name = "WinRAR", Icon = "📁", Downloads = "3107万次安装" },
                new ShopAppItem { Name = "CAD快速看图", Icon = "📐", Downloads = "1075万次安装" },
                new ShopAppItem { Name = "WorkBuddy", Icon = "🧰", Downloads = "51万次安装" },
                new ShopAppItem { Name = "超级互联", Icon = "🔗", Downloads = "488万次安装" },
                new ShopAppItem { Name = "千问", Icon = "💬", Downloads = "1631万次安装" },
                new ShopAppItem { Name = "直播伴侣", Icon = "🎙", Downloads = "5843万次安装" },
                new ShopAppItem { Name = "Visual Studio Code...", Icon = "💻", Downloads = "1025万次安装" },
                new ShopAppItem { Name = "Zip压缩标准版", Icon = "🗜", Downloads = "4708万次安装" },
                new ShopAppItem { Name = "网易云音乐", Icon = "🎶", Downloads = "1301万次安装" },
                new ShopAppItem { Name = "腾讯文档", Icon = "📄", Downloads = "891万次安装" },
            };
        }
    }

    // Shop2 - 游戏推荐页
    public class Shop2ViewModel : ObservableObject
    {
        private ObservableCollection<ShopAppItem> _recommendApps;
        private ObservableCollection<ShopAppItem> _hotGames;
        private string _bannerTitle;
        private string _bannerSubtitle;

        public ObservableCollection<ShopAppItem> RecommendApps { get => _recommendApps; set { SetProperty(ref _recommendApps, value); } }
        public ObservableCollection<ShopAppItem> HotGames { get => _hotGames; set { SetProperty(ref _hotGames, value); } }
        public string BannerTitle { get => _bannerTitle; set { SetProperty(ref _bannerTitle, value); } }
        public string BannerSubtitle { get => _bannerSubtitle; set { SetProperty(ref _bannerSubtitle, value); } }

        public Shop2ViewModel()
        {
            BannerTitle = "赛博演猿";
            BannerSubtitle = "轻松开启3D级爽战冒险";

            RecommendApps = new ObservableCollection<ShopAppItem>
            {
                new ShopAppItem { Name = "豆包", Icon = "🫘", Downloads = "412万次安装", Desc = "轻松应对问答、文案、翻译创作" },
                new ShopAppItem { Name = "精选专题合集", Icon = "⭐", Downloads = "特别定制技能，效率UP!", Desc = "" },
                new ShopAppItem { Name = "修真龙魄", Icon = "🐉", Downloads = "", Desc = "游戏玩家" },
                new ShopAppItem { Name = "休闲视频", Icon = "🎬", Downloads = "", Desc = "这世界挑战" },
                new ShopAppItem { Name = "逗逗游戏机", Icon = "🎮", Downloads = "AI选择趣味挑战", Desc = "" },
            };

            HotGames = new ObservableCollection<ShopAppItem>
            {
                new ShopAppItem { Name = "千问", Icon = "💬", Downloads = "2400万次安装" },
                new ShopAppItem { Name = "网易云音乐", Icon = "🎶", Downloads = "853万次安装" },
                new ShopAppItem { Name = "Visual Studio Code", Icon = "💻", Downloads = "1025万次安装" },
                new ShopAppItem { Name = "Zip压缩标准版", Icon = "🗜", Downloads = "4708万次安装" },
            };
        }
    }

    // Shop3 - 我的/已安装页
    public class Shop3ViewModel : ObservableObject
    {
        private ObservableCollection<ShopFilterItem> _tabs;
        private ObservableCollection<ShopAppItem> _installedApps;
        private string _totalApps;
        private string _totalSize;

        public ObservableCollection<ShopFilterItem> Tabs { get => _tabs; set { SetProperty(ref _tabs, value); } }
        public ObservableCollection<ShopAppItem> InstalledApps { get => _installedApps; set { SetProperty(ref _installedApps, value); } }
        public string TotalApps { get => _totalApps; set { SetProperty(ref _totalApps, value); } }
        public string TotalSize { get => _totalSize; set { SetProperty(ref _totalSize, value); } }

        public Shop3ViewModel()
        {
            Tabs = new ObservableCollection<ShopFilterItem>
            {
                new ShopFilterItem { Label = "本机安装应用", IsActive = true },
                new ShopFilterItem { Label = "历史安装应用" },
                new ShopFilterItem { Label = "历史安装包" },
            };

            TotalApps = "已安装50款软件";
            TotalSize = "18.93 GB";

            InstalledApps = new ObservableCollection<ShopAppItem>
            {
                new ShopAppItem { Name = "google chrome", Icon = "🌐", Version = "Google LLC", Size = "495.84 MB", InstallDate = "2026-08-05" },
                new ShopAppItem { Name = "python launcher", Icon = "🐍", Version = "Python Software Foundation", Size = "1.43 MB", InstallDate = "2026-08-01" },
                new ShopAppItem { Name = "python 3.10.8 (64-bit)", Icon = "🐍", Version = "Python Software Foundation", Size = "246.61 MB", InstallDate = "2026-08-01" },
                new ShopAppItem { Name = "microsoft visual c++ 2015-2022 redistributable (x86)", Icon = "🅰", Version = "Microsoft Corporation", Size = "18.24 MB", InstallDate = "2026-07-31" },
                new ShopAppItem { Name = "microsoft visual c++ 2015-2022 redistributable (x64)", Icon = "🅰", Version = "Microsoft Corporation", Size = "20.74 MB", InstallDate = "2026-07-31" },
                new ShopAppItem { Name = "microsoft 家庭和学生版 2021 - zh-cn", Icon = "📄", Version = "Microsoft Corporation", Size = "4.35 GB", InstallDate = "2026-07-31" },
            };
        }
    }

    // Shop4 - 办公页
    public class Shop4ViewModel : ObservableObject
    {
        private ObservableCollection<ShopFilterItem> _tabs;
        private ObservableCollection<ShopAppItem> _recommendApps;
        private string _bannerTitle;
        private string _bannerSubtitle;

        public ObservableCollection<ShopFilterItem> Tabs { get => _tabs; set { SetProperty(ref _tabs, value); } }
        public ObservableCollection<ShopAppItem> RecommendApps { get => _recommendApps; set { SetProperty(ref _recommendApps, value); } }
        public string BannerTitle { get => _bannerTitle; set { SetProperty(ref _bannerTitle, value); } }
        public string BannerSubtitle { get => _bannerSubtitle; set { SetProperty(ref _bannerSubtitle, value); } }

        public Shop4ViewModel()
        {
            Tabs = new ObservableCollection<ShopFilterItem>
            {
                new ShopFilterItem { Label = "精选", IsActive = true },
                new ShopFilterItem { Label = "教育" },
            };

            BannerTitle = "高效剪辑音频";
            BannerSubtitle = "轻松打造专业级音频作品";

            RecommendApps = new ObservableCollection<ShopAppItem>
            {
                new ShopAppItem { Name = "豆包", Icon = "🫘", Downloads = "412万次安装", Desc = "轻松应对问答、文案、翻译创作" },
                new ShopAppItem { Name = "元宝", Icon = "💰", Downloads = "617万次安装", Desc = "Hy3 模型正式上线，智能本地一键修一修" },
                new ShopAppItem { Name = "DClaw 龙虾本地一体化", Icon = "🦞", Downloads = "", Desc = "龙虾本地一键修一修" },
                new ShopAppItem { Name = "搜狗拼音输入法", Icon = "⌨", Downloads = "1.5亿次安装", Desc = "搜狗输入法，输入极致体验" },
                new ShopAppItem { Name = "智能清理大师", Icon = "🧹", Downloads = "", Desc = "智能AI清理系统，全功能清理" },
                new ShopAppItem { Name = "微软Office 365正版", Icon = "📄", Downloads = "", Desc = "正版Office正版，永不过期" },
                new ShopAppItem { Name = "讯飞听见-语音转文字", Icon = "🎙", Downloads = "", Desc = "语音转文字，多人转写，多语转写" },
                new ShopAppItem { Name = "图片编辑王", Icon = "🖼", Downloads = "", Desc = "功能强大、极速预览、一站式修图" },
                new ShopAppItem { Name = "CAD 极速看图", Icon = "📐", Downloads = "", Desc = "打开DWG、DXF等格式" },
                new ShopAppItem { Name = "千问", Icon = "💬", Downloads = "1631万次安装", Desc = "智能问答、阅读帮助、写作小说" },
                new ShopAppItem { Name = "百度网盘", Icon = "☁", Downloads = "7751万次安装", Desc = "大容量云存储，个人云盘" },
                new ShopAppItem { Name = "鸿鸟CAD", Icon = "🔧", Downloads = "", Desc = "最强大的CAD制图软件" },
                new ShopAppItem { Name = "PDF转换器阅读", Icon = "📕", Downloads = "", Desc = "专业PDF转换、编辑、阅读" },
                new ShopAppItem { Name = "PDF阅读转换极光版", Icon = "📗", Downloads = "", Desc = "极光PDF阅读器" },
                new ShopAppItem { Name = "AI写作王", Icon = "✍", Downloads = "", Desc = "智能写作、万能规划、写作小说" },
                new ShopAppItem { Name = "美图秀秀: AI图片与", Icon = "🎨", Downloads = "1377万次安装", Desc = "AI图片、AI滤镜、智能美化" },
                new ShopAppItem { Name = "DS-2本地智能AI", Icon = "🤖", Downloads = "", Desc = "一部超强大16大模型自由切换" },
                new ShopAppItem { Name = "元气桌面壁纸", Icon = "🌸", Downloads = "495万次安装", Desc = "超萌动态壁纸，一键戳戳" },
            };
        }
    }

    // Shop5 - 动态壁纸页
    public class Shop5ViewModel : ObservableObject
    {
        private ObservableCollection<ShopFilterItem> _tabs;
        private ObservableCollection<ShopFilterItem> _categoryFilters;
        private ObservableCollection<ShopAppItem> _wallpapers;
        private string _selectedFilter;

        public ObservableCollection<ShopFilterItem> Tabs { get => _tabs; set { SetProperty(ref _tabs, value); } }
        public ObservableCollection<ShopFilterItem> CategoryFilters { get => _categoryFilters; set { SetProperty(ref _categoryFilters, value); } }
        public ObservableCollection<ShopAppItem> Wallpapers { get => _wallpapers; set { SetProperty(ref _wallpapers, value); } }
        public string SelectedFilter { get => _selectedFilter; set { SetProperty(ref _selectedFilter, value); } }

        public Shop5ViewModel()
        {
            Tabs = new ObservableCollection<ShopFilterItem>
            {
                new ShopFilterItem { Label = "动态壁纸", IsActive = true },
                new ShopFilterItem { Label = "静态壁纸" },
                new ShopFilterItem { Label = "壁纸创作" },
                new ShopFilterItem { Label = "我的" },
            };

            CategoryFilters = new ObservableCollection<ShopFilterItem>
            {
                new ShopFilterItem { Label = "精选", IsActive = true },
                new ShopFilterItem { Label = "4K" },
                new ShopFilterItem { Label = "风景" },
                new ShopFilterItem { Label = "联想官方" },
                new ShopFilterItem { Label = "动漫" },
                new ShopFilterItem { Label = "简约" },
                new ShopFilterItem { Label = "美女" },
                new ShopFilterItem { Label = "科幻" },
                new ShopFilterItem { Label = "萌宠" },
                new ShopFilterItem { Label = "游戏" },
                new ShopFilterItem { Label = "汽车" },
                new ShopFilterItem { Label = "其他" },
            };

            SelectedFilter = "精选";

            Wallpapers = new ObservableCollection<ShopAppItem>
            {
                new ShopAppItem { Name = "", Icon = "🎨", Downloads = "YOGA" },
                new ShopAppItem { Name = "", Icon = "🏔", Downloads = "" },
                new ShopAppItem { Name = "", Icon = "🌊", Downloads = "" },
                new ShopAppItem { Name = "", Icon = "🌌", Downloads = "" },
                new ShopAppItem { Name = "", Icon = "🚗", Downloads = "3840匈牙利" },
                new ShopAppItem { Name = "", Icon = "🏙", Downloads = "3840深圳湾" },
                new ShopAppItem { Name = "", Icon = "🏜", Downloads = "3840迪拜" },
                new ShopAppItem { Name = "", Icon = "🌃", Downloads = "3840中东" },
                new ShopAppItem { Name = "", Icon = "🏯", Downloads = "3840利玛" },
                new ShopAppItem { Name = "", Icon = "🌉", Downloads = "3840杭州桥" },
                new ShopAppItem { Name = "", Icon = "🏛", Downloads = "3840亚历山大" },
                new ShopAppItem { Name = "", Icon = "🏙", Downloads = "3840迪拜" },
                new ShopAppItem { Name = "", Icon = "🌄", Downloads = "" },
                new ShopAppItem { Name = "", Icon = "🎆", Downloads = "" },
                new ShopAppItem { Name = "", Icon = "🌺", Downloads = "" },
                new ShopAppItem { Name = "", Icon = "🏮", Downloads = "" },
                new ShopAppItem { Name = "", Icon = "🌐", Downloads = "" },
                new ShopAppItem { Name = "", Icon = "🌠", Downloads = "" },
                new ShopAppItem { Name = "", Icon = "🎨", Downloads = "" },
                new ShopAppItem { Name = "", Icon = "🏙", Downloads = "" },
            };
        }
    }
}
