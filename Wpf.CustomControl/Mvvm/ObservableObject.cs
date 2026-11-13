using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Wpf.CustomControl.Mvvm
{
    /// <summary>
    /// 属性通知基类，实现 INotifyPropertyChanged 接口，
    /// 提供 SetProperty 方法用于属性变更通知。
    /// 兼容 .NET Framework 4.8（不使用 C# 6.0 语法）。
    /// </summary>
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 触发属性变更事件
        /// </summary>
        /// <param name="propertyName">属性名，默认由 CallerMemberName 自动获取</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// 设置属性值并触发变更通知
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="field">字段引用</param>
        /// <param name="value">新值</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>值是否发生变化</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
            {
                return false;
            }
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// 设置属性值并触发变更通知，同时触发关联属性的变更通知
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value, string[] relatedProperties, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
            {
                return false;
            }
            field = value;
            OnPropertyChanged(propertyName);
            if (relatedProperties != null)
            {
                for (int i = 0; i < relatedProperties.Length; i++)
                {
                    OnPropertyChanged(relatedProperties[i]);
                }
            }
            return true;
        }
    }
}
