using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Rxbit.BrigadeApp.Services
{
    public static class LocalizationService
    {
        public static event EventHandler LanguageChanged;

        public static void ChangeLanguage(string cultureName)
        {
            CultureInfo.CurrentCulture = new CultureInfo(cultureName);
            CultureInfo.CurrentUICulture = new CultureInfo(cultureName);

            var dict = new ResourceDictionary();
            if (cultureName.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
            {
                dict.Source = new Uri("pack://application:,,,/BrigadeTool;component/Localization/Strings.zh-CN.xaml");
            }
            else
            {
                dict.Source = new Uri("pack://application:,,,/BrigadeTool;component/Localization/Strings.xaml");
            }

            var app = Application.Current;
            if (app != null)
            {
                int idx = -1;
                for (int i = 0; i < app.Resources.MergedDictionaries.Count; i++)
                {
                    var d = app.Resources.MergedDictionaries[i];
                    if (d.Source != null && d.Source.OriginalString.Contains("/Localization/Strings"))
                    {
                        idx = i;
                        break;
                    }
                }

                if (idx >= 0)
                {
                    app.Resources.MergedDictionaries.RemoveAt(idx);
                }
                app.Resources.MergedDictionaries.Insert(0, dict);
            }

            LanguageChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}
