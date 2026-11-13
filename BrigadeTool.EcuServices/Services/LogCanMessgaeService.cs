using Rxbit.Brigade.CanDevices.Config;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Rxbit.Brigade.EcuServices.Services
{
    public class LogCanMessgaeService
    {
        private ConcurrentQueue<CanMessageWrapper> LogsMessages = new ConcurrentQueue<CanMessageWrapper>();

        public string File { get; set; }

        private AutoResetEvent resetEvent = new AutoResetEvent(false);
        public bool IsRecodding { get; set; }

        public void Log_CanDataComed(CanMessageWrapper obj)
        {
            if (!this.IsRecodding) return;
            this.LogsMessages.Enqueue(obj);
        }

        public async Task Start(string file) 
        {
            this.File = file;
            if (this.IsRecodding) return;
            this.IsRecodding = true;
            Task.Run(() => WriteLog());
            await Task.Delay(1000);
        }

        private void WriteLog()
        {
            this.IsRecodding = true;
            using ( StreamWriter streamWriter = new StreamWriter(this.File))
            {
                while (this.IsRecodding) 
                {
                    if (this.LogsMessages.IsEmpty)
                    {
                        Task.Delay(20);
                    }
                    else 
                    {
                        if (this.LogsMessages.TryDequeue(out CanMessageWrapper msg)) 
                        {
                        
                        
                        }
                    }    
                }
                this.resetEvent.Set();
            }
        }


        public async Task Stop() 
        {
            this.IsRecodding = false;
            this.resetEvent.WaitOne(3000);
            await Task.Delay(20);
        }
    }
}
