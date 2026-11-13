﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rxbit.Brigade.Services
{
    public class DataMediatorService<T> : IDataService
    {
        public event Action<T> DataComed = delegate { };
        public Boolean Open()
        {
            Clear();
            if (!_pushing)
            {
                _pushing = true;
                Task.Run(async () => { await PushAsync(); });
            }
            return true;
        }

        private void Clear()
        {
            while (!this._datas.IsEmpty)
            {
                this._datas.TryDequeue(out var frame);
            }
        }

        private async Task PushAsync()
        {
            while (this._pushing)
            {
                if (this._datas.IsEmpty)
                {
                    await Task.Delay(10);
                }
                else
                {
                    while (!this._datas.IsEmpty)
                    {
                        if (this._datas.TryDequeue(out var frame))
                        {
                            try
                            {
                                Parallel.ForEach(this.DataComed.GetInvocationList(),
                                    (callBack) => { callBack?.DynamicInvoke(frame); });
                            }
                            catch (Exception ex)
                            {
                             //   this._logger.Error(ex);
                            }
                        }
                    }
                }
            }
            _are.Set();
        }

        public Boolean Close()
        {
            Clear();
            if (_pushing)
            {
                _are.Reset();
                _pushing = false;
                _are.WaitOne(5000);
            }
            return true;
        }

        public void Enqueue(T e)
        {
            if (this._pushing)
            {
                this._datas.Enqueue(e);
            }
        }

        public DataMediatorService()
        {
            //this._logger = nls.logger;
        }

        private readonly ConcurrentQueue<T> _datas = new ConcurrentQueue<T>();
        private Boolean _pushing = false;
        private readonly AutoResetEvent _are = new AutoResetEvent(false);
       // private readonly Logger _logger;
    }
    public interface IDataService
    {
        Boolean Open();
        Boolean Close();
    }
}
