using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace Wpf.CustomControl.Behaviors
{
    /// <summary>
    /// DataGrid 自动滚动行为（附加属性方式）。
    /// 当 ItemsSource 新增项时自动滚动到末尾或开头，常用于日志、实时数据列表。
    ///
    /// 用法 1（启用，默认滚动到末尾）:
    ///   &lt;DataGrid behaviors:DataGridAutoScrollBehavior.IsEnabled="True" /&gt;
    ///
    /// 用法 2（指定滚动到开头）:
    ///   &lt;DataGrid behaviors:DataGridAutoScrollBehavior.IsEnabled="True"
    ///              behaviors:DataGridAutoScrollBehavior.ScrollDirection="First" /&gt;
    /// </summary>
    public static class DataGridAutoScrollBehavior
    {
        /// <summary>
        /// 滚动方向枚举
        /// </summary>
        public enum ScrollDirectionKind
        {
            /// <summary>滚动到末尾</summary>
            Last,
            /// <summary>滚动到开头</summary>
            First
        }

        #region IsEnabled 附加属性

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(DataGridAutoScrollBehavior),
                new FrameworkPropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnabledProperty, value);
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataGrid dataGrid = d as DataGrid;
            if (dataGrid == null)
            {
                return;
            }

            bool newValue = (e.NewValue != null) && (bool)e.NewValue;
            bool oldValue = (e.OldValue != null) && (bool)e.OldValue;
            if (newValue == oldValue)
            {
                return;
            }

            if (newValue)
            {
                Attach(dataGrid);
            }
            else
            {
                Detach(dataGrid);
            }
        }

        #endregion

        #region ScrollDirection 附加属性

        public static readonly DependencyProperty ScrollDirectionProperty =
            DependencyProperty.RegisterAttached(
                "ScrollDirection",
                typeof(ScrollDirectionKind),
                typeof(DataGridAutoScrollBehavior),
                new FrameworkPropertyMetadata(ScrollDirectionKind.Last));

        public static ScrollDirectionKind GetScrollDirection(DependencyObject obj)
        {
            return (ScrollDirectionKind)obj.GetValue(ScrollDirectionProperty);
        }

        public static void SetScrollDirection(DependencyObject obj, ScrollDirectionKind value)
        {
            obj.SetValue(ScrollDirectionProperty, value);
        }

        #endregion

        #region 内部附加属性：保存订阅句柄

        private static readonly DependencyProperty SubscriptionProperty =
            DependencyProperty.RegisterAttached(
                "Subscription",
                typeof(Subscription),
                typeof(DataGridAutoScrollBehavior),
                new FrameworkPropertyMetadata(null));

        private static Subscription GetSubscription(DependencyObject obj)
        {
            return (Subscription)obj.GetValue(SubscriptionProperty);
        }

        private static void SetSubscription(DependencyObject obj, Subscription value)
        {
            obj.SetValue(SubscriptionProperty, value);
        }

        #endregion

        #region 订阅/取消订阅

        private static void Attach(DataGrid dataGrid)
        {
            // 清理旧的订阅
            Detach(dataGrid);

            var sub = new Subscription(dataGrid);
            SetSubscription(dataGrid, sub);
            sub.Attach();
        }

        private static void Detach(DataGrid dataGrid)
        {
            Subscription sub = GetSubscription(dataGrid);
            if (sub != null)
            {
                sub.Detach();
                SetSubscription(dataGrid, null);
            }
        }

        #endregion

        #region Subscription 内部类

        /// <summary>
        /// 封装 DataGrid 与其集合订阅关系，便于解绑
        /// </summary>
        private class Subscription
        {
            private readonly WeakReference _dataGridRef;
            private INotifyCollectionChanged _collection;

            public Subscription(DataGrid dataGrid)
            {
                _dataGridRef = new WeakReference(dataGrid);
            }

            public void Attach()
            {
                DataGrid dataGrid = (DataGrid)_dataGridRef.Target;
                if (dataGrid == null)
                {
                    return;
                }

                // 监听 ItemsSource 变更，重新订阅集合
                dataGrid.TargetUpdated += OnTargetUpdated;
                dataGrid.Loaded += OnLoaded;
                dataGrid.Unloaded += OnUnloaded;

                SubscribeCollection(dataGrid);

                // 首次加载时滚动一次
                ScrollToPosition(dataGrid);
            }

            public void Detach()
            {
                DataGrid dataGrid = (DataGrid)_dataGridRef.Target;
                if (dataGrid != null)
                {
                    dataGrid.TargetUpdated -= OnTargetUpdated;
                    dataGrid.Loaded -= OnLoaded;
                    dataGrid.Unloaded -= OnUnloaded;
                }

                UnsubscribeCollection();
            }

            private void OnLoaded(object sender, RoutedEventArgs e)
            {
                DataGrid dataGrid = sender as DataGrid;
                if (dataGrid != null)
                {
                    SubscribeCollection(dataGrid);
                    ScrollToPosition(dataGrid);
                }
            }

            private void OnUnloaded(object sender, RoutedEventArgs e)
            {
                UnsubscribeCollection();
            }

            private void OnTargetUpdated(object sender, DataTransferEventArgs e)
            {
                DataGrid dataGrid = sender as DataGrid;
                if (dataGrid != null && e.Property == ItemsControl.ItemsSourceProperty)
                {
                    SubscribeCollection(dataGrid);
                    ScrollToPosition(dataGrid);
                }
            }

            private void SubscribeCollection(DataGrid dataGrid)
            {
                UnsubscribeCollection();

                _collection = dataGrid.ItemsSource as INotifyCollectionChanged;
                if (_collection != null)
                {
                    _collection.CollectionChanged += OnCollectionChanged;
                }
            }

            private void UnsubscribeCollection()
            {
                if (_collection != null)
                {
                    _collection.CollectionChanged -= OnCollectionChanged;
                    _collection = null;
                }
            }

            private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                DataGrid dataGrid = _dataGridRef.Target as DataGrid;
                if (dataGrid == null)
                {
                    return;
                }

                if (e.Action == NotifyCollectionChangedAction.Add ||
                    e.Action == NotifyCollectionChangedAction.Reset ||
                    e.Action == NotifyCollectionChangedAction.Replace)
                {
                    ScrollToPosition(dataGrid);
                }
            }

            private void ScrollToPosition(DataGrid dataGrid)
            {
                if (dataGrid == null || dataGrid.Items == null || dataGrid.Items.Count == 0)
                {
                    return;
                }

                ScrollDirectionKind direction = GetScrollDirection(dataGrid);
                int index = direction == ScrollDirectionKind.First ? 0 : dataGrid.Items.Count - 1;

                dataGrid.Dispatcher.BeginInvoke(new Action(delegate
                {
                    try
                    {
                        if (dataGrid.Items == null || dataGrid.Items.Count == 0)
                        {
                            return;
                        }
                        if (index < 0 || index >= dataGrid.Items.Count)
                        {
                            index = dataGrid.Items.Count - 1;
                        }
                        if (index < 0)
                        {
                            return;
                        }
                        dataGrid.ScrollIntoView(dataGrid.Items[index]);
                    }
                    catch
                    {
                        // 忽略滚动失败，避免影响主流程
                    }
                }), DispatcherPriority.Background);
            }
        }

        #endregion
    }
}
