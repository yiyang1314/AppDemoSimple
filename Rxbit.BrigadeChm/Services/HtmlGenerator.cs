using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Rxbit.BrigadeChm.Models;

namespace Rxbit.BrigadeChm.Services
{
    public class HtmlGenerator
    {
        private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

        public string GenerateTopicHtml(HelpTopic topic, HelpContentModel model)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html xmlns='http://www.w3.org/1999/xhtml'>");
            sb.AppendLine("<head>");
            sb.AppendLine($"<meta charset='utf-8'>");
            sb.AppendLine($"<title>{topic.Title} - {model.Title}</title>");
            sb.AppendLine("<style>");
            sb.AppendLine(GetCssStyle());
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class='container'>");

            if (topic.ParentSection != null)
            {
                sb.AppendLine($"<div class='breadcrumb'><a href='index.html'>首页</a> &raquo; {topic.ParentSection.Title} &raquo; {topic.Title}</div>");
            }

            var content = ApplyContentTemplate(topic.HtmlContent ?? "", topic.Title);
            sb.AppendLine(content);

            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        public string GenerateIndexHtml(HelpContentModel model)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html xmlns='http://www.w3.org/1999/xhtml'>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='utf-8'>");
            sb.AppendLine($"<title>{model.Title}</title>");
            sb.AppendLine("<style>");
            sb.AppendLine(GetCssStyle());
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class='container'>");
            sb.AppendLine($"<h1>{model.Title}</h1>");
            sb.AppendLine($"<p class='subtitle'>版本 {model.Version} | 作者 {model.Author}</p>");

            sb.AppendLine("<h2>目录</h2>");
            sb.AppendLine("<ul class='toc'>");
            foreach (var section in model.Sections.OrderBy(s => s.SortOrder))
            {
                sb.AppendLine($"<li><strong>{section.Title}</strong>");
                sb.AppendLine("<ul>");
                foreach (var topic in section.Topics.OrderBy(t => t.SortOrder))
                {
                    sb.AppendLine($"<li><a href='{topic.FileName}'>{topic.Title}</a></li>");
                }
                sb.AppendLine("</ul>");
                sb.AppendLine("</li>");
            }
            sb.AppendLine("</ul>");

            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        public string GenerateSearchPage(HelpContentModel model)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html xmlns='http://www.w3.org/1999/xhtml'>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='utf-8'>");
            sb.AppendLine("<title>搜索 - " + model.Title + "</title>");
            sb.AppendLine("<style>");
            sb.AppendLine(GetCssStyle());
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class='container'>");
            sb.AppendLine("<h1>搜索</h1>");
            sb.AppendLine("<div class='search-box'>");
            sb.AppendLine("<input type='text' id='searchInput' placeholder='输入关键词搜索...' onkeyup='doSearch(this.value)'>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div id='results'></div>");
            sb.AppendLine("<script>");
            sb.AppendLine("var topics = [");

            var first = true;
            foreach (var section in model.Sections)
            {
                foreach (var topic in section.Topics)
                {
                    if (!first) sb.Append(",");
                    sb.Append($"{{title: '{topic.Title}', file: '{topic.FileName}', keywords: '{topic.Keywords ?? ""}'}}");
                    first = false;
                }
            }

            sb.AppendLine("];");
            sb.AppendLine("function doSearch(q) {");
            sb.AppendLine("  var results = document.getElementById('results');");
            sb.AppendLine("  if (!q) { results.innerHTML = ''; return; }");
            sb.AppendLine("  q = q.toLowerCase();");
            sb.AppendLine("  var matched = topics.filter(function(t) {");
            sb.AppendLine("    return t.title.toLowerCase().indexOf(q) >= 0 || t.keywords.toLowerCase().indexOf(q) >= 0;");
            sb.AppendLine("  });");
            sb.AppendLine("  if (matched.length === 0) {");
            sb.AppendLine("    results.innerHTML = '<p>未找到相关内容</p>';");
            sb.AppendLine("  } else {");
            sb.AppendLine("    results.innerHTML = '<ul>' + matched.map(function(t) {");
            sb.AppendLine("      return '<li><a href=\"' + t.file + '\">' + t.title + '</a></li>';");
            sb.AppendLine("    }).join('') + '</ul>';");
            sb.AppendLine("  }");
            sb.AppendLine("}");
            sb.AppendLine("</script>");
            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        public void WriteAllFiles(HelpContentModel model, string outputDir)
        {
            var htmlDir = Path.Combine(outputDir, "html");
            Directory.CreateDirectory(htmlDir);

            File.WriteAllText(
                Path.Combine(htmlDir, "index.html"),
                GenerateIndexHtml(model),
                Utf8NoBom);

            File.WriteAllText(
                Path.Combine(htmlDir, "search.html"),
                GenerateSearchPage(model),
                Utf8NoBom);

            foreach (var section in model.Sections)
            {
                foreach (var topic in section.Topics)
                {
                    var html = GenerateTopicHtml(topic, model);
                    File.WriteAllText(
                        Path.Combine(htmlDir, topic.FileName),
                        html,
                        Utf8NoBom);
                }
            }
        }

        private string ApplyContentTemplate(string content, string title)
        {
            if (string.IsNullOrEmpty(content))
                return $"<h1>{title}</h1><p>暂无内容。</p>";

            var cssClasses = new HashSet<string>();
            var result = new StringBuilder();
            result.Append(content);
            return result.ToString();
        }

        private string GetCssStyle()
        {
            return @"
body {
    font-family: 'Segoe UI', 'Microsoft YaHei', Tahoma, sans-serif;
    margin: 0;
    padding: 20px;
    background: #1a1b1d;
    color: #d0d0d0;
    font-size: 14px;
    line-height: 1.6;
}
.container {
    max-width: 900px;
    margin: 0 auto;
    background: #1e2a3d;
    padding: 30px 40px;
    border-radius: 8px;
    box-shadow: 0 4px 20px rgba(0,0,0,0.3);
}
h1 {
    color: #00d9ff;
    font-size: 24px;
    border-bottom: 2px solid #0f3460;
    padding-bottom: 10px;
    margin-top: 0;
}
h2 {
    color: #00d9ff;
    font-size: 18px;
    margin-top: 20px;
}
h3 {
    color: #4fc3f7;
    font-size: 16px;
}
a {
    color: #00d9ff;
    text-decoration: none;
}
a:hover {
    text-decoration: underline;
}
ul, ol {
    padding-left: 20px;
}
li {
    margin: 5px 0;
}
table {
    width: 100%;
    border-collapse: collapse;
    margin: 15px 0;
}
th {
    background: #0f3460;
    color: #00d9ff;
    padding: 10px;
    text-align: left;
}
td {
    padding: 8px 10px;
    border: 1px solid #2a3a4d;
}
tr:nth-child(even) {
    background: #16213e;
}
.breadcrumb {
    color: #888;
    font-size: 12px;
    margin-bottom: 15px;
    padding: 8px 12px;
    background: #0f3460;
    border-radius: 4px;
}
.breadcrumb a {
    color: #00d9ff;
}
.subtitle {
    color: #888;
    font-size: 13px;
}
.toc {
    list-style: none;
    padding: 0;
}
.toc ul {
    margin: 5px 0 10px 20px;
}
.toc li {
    margin: 8px 0;
}
.toc strong {
    color: #00d9ff;
    font-size: 16px;
}
.tip {
    background: #0f3460;
    border-left: 4px solid #00d9ff;
    padding: 10px 15px;
    margin: 15px 0;
    border-radius: 4px;
}
.search-box {
    margin: 20px 0;
}
.search-box input {
    width: 100%;
    padding: 10px 15px;
    border: 1px solid #2a3a4d;
    border-radius: 4px;
    background: #16213e;
    color: #d0d0d0;
    font-size: 14px;
}
.search-box input:focus {
    outline: none;
    border-color: #00d9ff;
}
";
        }
    }
}
