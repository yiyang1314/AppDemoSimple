using System;
using System.Windows.Input;

namespace Wpf.CustomControl.Mvvm
{
    /// <summary>
    /// 同步命令实现，支持 CanExecute 判断和 RaiseCanExecuteChanged 通知。
    /// 兼容 .NET Framework 4.8（不使用 C# 6.0 语法）。
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// 创建命令实例
        /// </summary>
        /// <param name="execute">执行方法</param>
        public RelayCommand(Action execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// 创建命令实例
        /// </summary>
        /// <param name="execute">执行方法</param>
        /// <param name="canExecute">可执行判断方法</param>
        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public void Execute(object parameter)
        {
            _execute();
        }

        /// <summary>
        /// 触发 CanExecuteChanged 事件，通知 UI 重新评估命令可执行状态
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            EventHandler handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// 带参数的同步命令实现
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
            {
                return true;
            }
            if (parameter == null)
            {
                return _canExecute(default(T));
            }
            if (parameter is T)
            {
                return _canExecute((T)parameter);
            }
            return false;
        }

        public void Execute(object parameter)
        {
            if (parameter == null)
            {
                _execute(default(T));
            }
            else
            {
                _execute((T)parameter);
            }
        }

        public void RaiseCanExecuteChanged()
        {
            EventHandler handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
