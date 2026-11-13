using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Rxbit.RdpAppUI.Models
{
    public class RemoteConnection : IDisposable
    {
        public string Host { get; set; }
        public int Port { get; set; } = 3389;
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsConnected { get; set; }
        public TcpClient TcpClient { get; set; }
        public NetworkStream NetworkStream { get; set; }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                TcpClient = new TcpClient();
                await TcpClient.ConnectAsync(Host, Port);
                NetworkStream = TcpClient.GetStream();
                IsConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                IsConnected = false;
                throw new Exception($"连接失败: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            NetworkStream?.Close();
            TcpClient?.Close();
            IsConnected = false;
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
