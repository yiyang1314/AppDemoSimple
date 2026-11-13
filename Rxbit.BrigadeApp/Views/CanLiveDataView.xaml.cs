using Rxbit.BrigadeApp.Controls;
using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Rxbit.BrigadeApp.Views
{
    public partial class CanLiveDataView : WindowBase
    {
        public CanLiveDataView()
        {
            InitializeComponent();
        }
    }

    public static class DataGridScrollAttach
    {
        public static bool GetAutoScrollToBottom(DependencyObject obj)
            => (bool)obj.GetValue(AutoScrollToBottomProperty);

        public static void SetAutoScrollToBottom(DependencyObject obj, bool value)
            => obj.SetValue(AutoScrollToBottomProperty, value);

        public static readonly DependencyProperty AutoScrollToBottomProperty =
            DependencyProperty.RegisterAttached(
                "AutoScrollToBottom",
                typeof(bool),
                typeof(DataGridScrollAttach),
                new PropertyMetadata(false, OnAutoScrollChanged));

        private static void OnAutoScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is DataGrid grid)) return;

            bool enableAutoScroll = (bool)e.NewValue;
            if (!enableAutoScroll) return;

            grid.Loaded += (s, ev) =>
            {
                var view = CollectionViewSource.GetDefaultView(grid.ItemsSource);
                if (view == null) return;

                view.CollectionChanged += (s2, ev2) =>
                {
                    if (ev2.Action != NotifyCollectionChangedAction.Add)
                        return;

                    // 修复：显式指定 Action 委托，消除CS1660报错
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (grid.Items.Count > 0)
                        {
                            var lastItem = grid.Items[grid.Items.Count - 1];
                            grid.ScrollIntoView(lastItem);
                        }
                    }));
                };
            };
        }
    }
}
