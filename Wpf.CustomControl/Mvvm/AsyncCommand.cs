using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Wpf.CustomControl.Mvvm
{
    /// <summary>
    /// 异步命令实现，基于 Task，支持 IsExecuting 状态绑定和异常捕获。
    /// 执行期间 CanExecute 返回 false，防止重复触发。
    /// 兼容 .NET Framework 4.8（不使用 C# 6.0 语法）。
    /// </summary>
    public class AsyncCommand : ObservableObject, ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;
        private bool _isExecuting;
        private Exception _exception;

        /// <summary>
        /// 是否正在执行
        /// </summary>
        public bool IsExecuting
        {
            get { return _isExecuting; }
            private set
            {
                if (SetProperty(ref _isExecuting, value))
                {
                    RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// 执行过程中捕获的异常（若有）
        /// </summary>
        public Exception Exception
        {
            get { return _exception; }
            private set { SetProperty(ref _exception, value); }
        }

        public event EventHandler CanExecuteChanged;

        public event EventHandler<AsyncCommandErrorEventArgs> ErrorOccurred;

        public AsyncCommand(Func<Task> execute)
            : this(execute, null)
        {
        }

        public AsyncCommand(Func<Task> execute, Func<bool> canExecute)
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
            if (_isExecuting)
            {
                return false;
            }
            return _canExecute == null || _canExecute();
        }

        public async void Execute(object parameter)
        {
            if (!CanExecute(parameter))
            {
                return;
            }
            IsExecuting = true;
            Exception = null;
            try
            {
                await _execute();
            }
            catch (Exception ex)
            {
                Exception = ex;
                EventHandler<AsyncCommandErrorEventArgs> handler = ErrorOccurred;
                if (handler != null)
                {
                    handler(this, new AsyncCommandErrorEventArgs(ex));
                }
            }
            finally
            {
                IsExecuting = false;
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

    /// <summary>
    /// 带参数的异步命令实现
    /// </summary>
    public class AsyncCommand<T> : ObservableObject, ICommand
    {
        private readonly Func<T, Task> _execute;
        private readonly Func<T, bool> _canExecute;
        private bool _isExecuting;
        private Exception _exception;

        public bool IsExecuting
        {
            get { return _isExecuting; }
            private set
            {
                if (SetProperty(ref _isExecuting, value))
                {
                    RaiseCanExecuteChanged();
                }
            }
        }

        public Exception Exception
        {
            get { return _exception; }
            private set { SetProperty(ref _exception, value); }
        }

        public event EventHandler CanExecuteChanged;

        public event EventHandler<AsyncCommandErrorEventArgs> ErrorOccurred;

        public AsyncCommand(Func<T, Task> execute)
            : this(execute, null)
        {
        }

        public AsyncCommand(Func<T, Task> execute, Func<T, bool> canExecute)
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
            if (_isExecuting)
            {
                return false;
            }
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

        public async void Execute(object parameter)
        {
            if (!CanExecute(parameter))
            {
                return;
            }
            T arg = parameter == null ? default(T) : (T)parameter;
            IsExecuting = true;
            Exception = null;
            try
            {
                await _execute(arg);
            }
            catch (Exception ex)
            {
                Exception = ex;
                EventHandler<AsyncCommandErrorEventArgs> handler = ErrorOccurred;
                if (handler != null)
                {
                    handler(this, new AsyncCommandErrorEventArgs(ex));
                }
            }
            finally
            {
                IsExecuting = false;
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

    /// <summary>
    /// 异步命令错误事件参数
    /// </summary>
    public class AsyncCommandErrorEventArgs : EventArgs
    {
        public Exception Exception { get; private set; }

        public AsyncCommandErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
}
