using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Rxbit.BrigadeChm.Models
{
    public class HelpContentModel
    {
        public string Title { get; set; } = "Brigade 软件帮助文档";
        public string Version { get; set; } = "V1.0.0";
        public string Author { get; set; } = "Rxbit";
        public string OutputDirectory { get; set; }
        public string ChmFilePath { get; set; }
        public ObservableCollection<HelpSection> Sections { get; set; } = new ObservableCollection<HelpSection>();
        public ObservableCollection<HelpIndex> IndexItems { get; set; } = new ObservableCollection<HelpIndex>();

        public HelpSection FindSectionById(string id)
        {
            return Sections.FirstOrDefault(s => s.Id == id);
        }

        public HelpTopic FindTopicById(string id)
        {
            foreach (var section in Sections)
            {
                var topic = section.Topics.FirstOrDefault(t => t.Id == id);
                if (topic != null) return topic;

                foreach (var subSection in section.SubSections)
                {
                    topic = subSection.Topics.FirstOrDefault(t => t.Id == id);
                    if (topic != null) return topic;
                }
            }
            return null;
        }

        public void AddDefaultContent()
        {
            Sections.Clear();
            IndexItems.Clear();

            var gettingStarted = new HelpSection
            {
                Id = "gs",
                Title = "快速入门",
                SortOrder = 0
            };

            gettingStarted.Topics.Add(new HelpTopic
            {
                Id = "gs_intro",
                Title = "软件简介",
                FileName = "introduction.html",
                SortOrder = 0,
                Keywords = "简介,介绍,概述,introduction,overview",
                HtmlContent = GetIntroductionHtml()
            });

            gettingStarted.Topics.Add(new HelpTopic
            {
                Id = "gs_install",
                Title = "安装指南",
                FileName = "installation.html",
                SortOrder = 1,
                Keywords = "安装,部署,install,setup,deployment",
                HtmlContent = GetInstallationHtml()
            });

            gettingStarted.Topics.Add(new HelpTopic
            {
                Id = "gs_quickstart",
                Title = "快速上手",
                FileName = "quickstart.html",
                SortOrder = 2,
                Keywords = "快速,上手,开始,quickstart,start",
                HtmlContent = GetQuickStartHtml()
            });

            Sections.Add(gettingStarted);

            var deviceSection = new HelpSection
            {
                Id = "dev",
                Title = "设备配置",
                SortOrder = 1
            };

            deviceSection.Topics.Add(new HelpTopic
            {
                Id = "dev_can",
                Title = "CAN 设备配置",
                FileName = "can_config.html",
                SortOrder = 0,
                Keywords = "CAN,设备,配置,device,can,bus",
                HtmlContent = GetCanConfigHtml()
            });

            deviceSection.Topics.Add(new HelpTopic
            {
                Id = "dev_sensor",
                Title = "传感器配置",
                FileName = "sensor_config.html",
                SortOrder = 1,
                Keywords = "传感器,配置,sensor,config",
                HtmlContent = GetSensorConfigHtml()
            });

            deviceSection.Topics.Add(new HelpTopic
            {
                Id = "dev_uds",
                Title = "UDS 诊断配置",
                FileName = "uds_config.html",
                SortOrder = 2,
                Keywords = "UDS,诊断,配置,diagnostic",
                HtmlContent = GetUdsConfigHtml()
            });

            Sections.Add(deviceSection);

            var toolsSection = new HelpSection
            {
                Id = "tools",
                Title = "工具使用",
                SortOrder = 2
            };

            toolsSection.Topics.Add(new HelpTopic
            {
                Id = "tools_crc",
                Title = "CRC 校验工具",
                FileName = "crc_tool.html",
                SortOrder = 0,
                Keywords = "CRC,校验,工具,crc,calc",
                HtmlContent = GetCrcToolHtml()
            });

            toolsSection.Topics.Add(new HelpTopic
            {
                Id = "tools_blf",
                Title = "BLF 转 ASC 工具",
                FileName = "blf_tool.html",
                SortOrder = 1,
                Keywords = "BLF,ASC,转换,工具,convert",
                HtmlContent = GetBlfToolHtml()
            });

            toolsSection.Topics.Add(new HelpTopic
            {
                Id = "tools_asc2brigade",
                Title = "ASC 转 Brigade 格式",
                FileName = "asc_brigade.html",
                SortOrder = 2,
                Keywords = "ASC,Brigade,格式,转换",
                HtmlContent = GetAscBrigadeHtml()
            });

            Sections.Add(toolsSection);

            var troubleshooting = new HelpSection
            {
                Id = "trouble",
                Title = "故障排除",
                SortOrder = 3
            };

            troubleshooting.Topics.Add(new HelpTopic
            {
                Id = "trouble_common",
                Title = "常见问题",
                FileName = "faq.html",
                SortOrder = 0,
                Keywords = "常见,问题,FAQ,故障,troubleshoot",
                HtmlContent = GetFaqHtml()
            });

            Sections.Add(troubleshooting);

            RebuildIndex();
        }

        public void RebuildIndex()
        {
            IndexItems.Clear();
            foreach (var section in Sections)
            {
                foreach (var topic in section.Topics)
                {
                    if (topic.ParentSection == null)
                        topic.ParentSection = section;

                    var keywords = topic.Keywords?.Split(',');
                    if (keywords != null)
                    {
                        foreach (var kw in keywords)
                        {
                            var trimmed = kw.Trim();
                            if (!string.IsNullOrEmpty(trimmed))
                            {
                                IndexItems.Add(new HelpIndex
                                {
                                    Keyword = trimmed,
                                    TopicId = topic.Id,
                                    TopicTitle = topic.Title
                                });
                            }
                        }
                    }
                }
            }
        }

        public HelpSection LoadFromHtmlFile(string filePath, string sectionTitle = "导入文档")
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"文件不存在: {filePath}");

            var html = File.ReadAllText(filePath);
            var title = ExtractTitle(html) ?? Path.GetFileNameWithoutExtension(filePath);
            var content = ExtractBody(html);

            var section = new HelpSection
            {
                Id = "import_" + Guid.NewGuid().ToString("N").Substring(0, 8),
                Title = sectionTitle,
                SortOrder = Sections.Count
            };

            var topicId = "topic_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            var safeTitle = SanitizeFileName(title);

            section.Topics.Add(new HelpTopic
            {
                Id = topicId,
                Title = title,
                FileName = safeTitle + ".html",
                SortOrder = 0,
                Keywords = title + ",导入,import",
                HtmlContent = content
            });

            Sections.Add(section);
            RebuildIndex();
            return section;
        }

        public int LoadFromDirectory(string directoryPath, string sectionTitle = "导入文档")
        {
            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException($"目录不存在: {directoryPath}");

            var htmlFiles = Directory.GetFiles(directoryPath, "*.html", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(directoryPath, "*.htm", SearchOption.AllDirectories))
                .OrderBy(f => f)
                .ToList();

            if (htmlFiles.Count == 0)
                throw new InvalidOperationException("指定目录中未找到 HTML 文件");

            var section = new HelpSection
            {
                Id = "import_" + Guid.NewGuid().ToString("N").Substring(0, 8),
                Title = sectionTitle,
                SortOrder = Sections.Count
            };

            int index = 0;
            foreach (var filePath in htmlFiles)
            {
                var html = File.ReadAllText(filePath);
                var title = ExtractTitle(html) ?? Path.GetFileNameWithoutExtension(filePath);
                var content = ExtractBody(html);
                var safeTitle = SanitizeFileName(title);

                var topicId = "topic_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                var uniqueFileName = index == 0
                    ? safeTitle + ".html"
                    : $"{safeTitle}_{index}.html";

                section.Topics.Add(new HelpTopic
                {
                    Id = topicId,
                    Title = title,
                    FileName = uniqueFileName,
                    SortOrder = index,
                    Keywords = title + ",导入,import",
                    HtmlContent = content
                });

                index++;
            }

            Sections.Add(section);
            RebuildIndex();
            return htmlFiles.Count;
        }

        private static string ExtractTitle(string html)
        {
            var match = Regex.Match(html, @"<title[^>]*>(.*?)</title>",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            return match.Success ? match.Groups[1].Value.Trim() : null;
        }

        private static string ExtractBody(string html)
        {
            var bodyMatch = Regex.Match(html, @"<body[^>]*>(.*?)</body>",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (bodyMatch.Success)
            {
                var body = bodyMatch.Groups[1].Value;
                body = Regex.Replace(body, @"<script[^>]*>.*?</script>", "",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline);
                body = Regex.Replace(body, @"<style[^>]*>.*?</style>", "",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline);
                return body.Trim();
            }

            return html;
        }

        private static string SanitizeFileName(string name)
        {
            var invalid = new[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };
            var sanitized = string.Join("_",
                name.Select(c => invalid.Contains(c) ? '_' : c));
            return sanitized.Length > 60 ? sanitized.Substring(0, 60) : sanitized;
        }

        private string GetIntroductionHtml() => @"<h1>软件简介</h1>
<p>Backsense BS-9200X 测试工具是一款用于雷达传感器测试、CAN 总线通信分析、UDS 诊断和数据录制的综合性软件平台。</p>
<h2>主要功能</h2>
<ul>
<li>CAN 总线实时数据监控与录制</li>
<li>传感器参数配置与检测</li>
<li>UDS 诊断协议支持</li>
<li>数据文件格式转换（BLF → ASC → Brigade）</li>
<li>CRC 校验工具</li>
<li>实时雷达可视化显示</li>
</ul>";

        private string GetInstallationHtml() => @"<h1>安装指南</h1>
<h2>系统要求</h2>
<ul>
<li>操作系统：Windows 10/11 (64位)</li>
<li>.NET Framework 4.8 或更高版本</li>
<li>内存：至少 4GB RAM</li>
<li>硬盘：至少 200MB 可用空间</li>
</ul>
<h2>安装步骤</h2>
<ol>
<li>运行安装程序 setup.exe</li>
<li>选择安装目录</li>
<li>等待安装完成</li>
<li>从桌面或开始菜单启动程序</li>
</ol>";

        private string GetQuickStartHtml() => @"<h1>快速上手</h1>
<h2>基本工作流程</h2>
<ol>
<li>连接 CAN 设备到电脑</li>
<li>在【设备】菜单中配置 CAN 接口</li>
<li>选择传感器 ID 并连接</li>
<li>在【视图】菜单中打开需要的功能窗口</li>
<li>开始数据录制或实时监控</li>
</ol>
<div class='tip'><strong>提示：</strong>首次使用请先在【传感器配置】中设置正确的参数。</div>";

        private string GetCanConfigHtml() => @"<h1>CAN 设备配置</h1>
<h2>支持的设备类型</h2>
<ul>
<li>PEAK PCAN (USB / PCI)</li>
<li>Vector CAN (XL 系列)</li>
<li>Kvaser CAN (USB / PCI)</li>
<li>周立功 USBCAN</li>
<li>创芯科技 CXKJ CAN</li>
</ul>
<h2>配置步骤</h2>
<ol>
<li>连接 CAN 硬件</li>
<li>打开【设备 → CAN 设备配置】</li>
<li>选择硬件类型和通道</li>
<li>设置波特率（500K / 250K / 125K）</li>
<li>点击【连接】</li>
</ol>";

        private string GetSensorConfigHtml() => @"<h1>传感器配置</h1>
<h2>参数说明</h2>
<table border='1' cellpadding='5'>
<tr><th>参数</th><th>说明</th><th>默认值</th></tr>
<tr><td>传感器 ID</td><td>CAN 协议中的设备标识符</td><td>0x100</td></tr>
<tr><td>检测距离</td><td>传感器最大检测范围</td><td>200m</td></tr>
<tr><td>角度范围</td><td>水平/垂直视角</td><td>±75°</td></tr>
<tr><td>刷新率</td><td>数据输出频率</td><td>50ms</td></tr>
</table>";

        private string GetUdsConfigHtml() => @"<h1>UDS 诊断配置</h1>
<h2>诊断服务</h2>
<ul>
<li>0x10 - 会话控制</li>
<li>0x11 - ECU 复位</li>
<li>0x14 - 清除诊断信息</li>
<li>0x19 - 读取诊断信息</li>
<li>0x22 - 读取数据标识符</li>
<li>0x27 - 安全访问</li>
<li>0x2E - 写入数据标识符</li>
<li>0x28 - 通信控制</li>
</ul>";

        private string GetCrcToolHtml() => @"<h1>CRC 校验工具</h1>
<h2>支持的算法</h2>
<ul>
<li>CRC-8</li>
<li>CRC-16</li>
<li>CRC-16/CCITT</li>
<li>CRC-32</li>
<li>自定义多项式 CRC</li>
</ul>
<h2>使用方法</h2>
<ol>
<li>打开【工具 → CRC 工具】</li>
<li>选择 CRC 算法类型</li>
<li>输入待计算的十六进制数据</li>
<li>设置初始值和结果异或值</li>
<li>点击【计算】获取校验结果</li>
</ol>";

        private string GetBlfToolHtml() => @"<h1>BLF 转 ASC 工具</h1>
<p>将 Vector BLF 格式日志文件转换为标准 ASC 格式。</p>
<h2>使用方法</h2>
<ol>
<li>打开【工具 → BLF2Asc 工具】</li>
<li>选择输入的 BLF 文件</li>
<li>设置输出目录</li>
<li>选择时间戳格式</li>
<li>点击【转换】</li>
</ol>";

        private string GetAscBrigadeHtml() => @"<h1>ASC 转 Brigade 格式</h1>
<p>将标准 ASC 日志文件转换为 Brigade 专用格式。</p>
<h2>使用方法</h2>
<ol>
<li>打开【工具 → Asc to Brigade】</li>
<li>选择输入的 ASC 文件</li>
<li>设置输出路径</li>
<li>点击【转换】</li>
</ol>";

        private string GetFaqHtml() => @"<h1>常见问题</h1>
<h2>Q1: CAN 设备无法连接？</h2>
<p>请检查：1) 驱动是否正确安装 2) USB 线是否连接 3) 波特率设置是否匹配。</p>
<h2>Q2: 传感器无数据输出？</h2>
<p>请检查：1) 传感器供电是否正常 2) CAN 总线终端电阻（120Ω） 3) 传感器 ID 配置是否正确。</p>
<h2>Q3: BLF 文件转换失败？</h2>
<p>请确认文件版本兼容性。BLF 格式不同版本可能存在差异。</p>
<h2>Q4: 如何导出录制数据？</h2>
<p>使用【文件 → 打开录制路径】找到录制文件，可直接复制或转换格式。</p>";
    }
}
