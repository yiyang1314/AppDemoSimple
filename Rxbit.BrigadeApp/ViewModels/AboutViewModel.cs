﻿using Rxbit.Brigade.Models.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace Rxbit.BrigadeApp.ViewModels
{
    public class AboutViewModel : ViewModelBase
    {
        public string Version
        {
            get { return _Version; }
            set { SetProperty(ref _Version, value); }
        }
        private string _Version = "V1.0.0";

        public string PublishTime
        {
            get { return _PublishTime; }
            set { SetProperty(ref _PublishTime, value); }
        }
        private string _PublishTime = "2026/07/02";

        public string Devloper
        {
            get { return _Devloper; }
            set { SetProperty(ref _Devloper, value); }
        }
        private string _Devloper = "Yi.Tang";

        public ObservableCollection<LogItem> LogItems { get; set; } = new ObservableCollection<LogItem>();

        public AboutViewModel()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            this.Version = $"V{version.Major}.{version.Minor}.{version.Build}";
            this.PublishTime = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss");
            this.Devloper = "Yi.Tang";
            this.LoadLogInfo();
        }

        private void LoadLogInfo()
        {
            var logArray = Application.Current.TryFindResource("LogInfo") as string[];
            if (logArray != null)
            {
                for (int i = 0; i < logArray.Length; i++)
                {
                    LogItems.Add(new LogItem
                    {
                        Index = i + 1,
                        Description = logArray[i]
                    });
                }
            }
        }
    }

    public class LogItem
    {
        public int Index { get; set; }
        public string Description { get; set; }
    }
}
