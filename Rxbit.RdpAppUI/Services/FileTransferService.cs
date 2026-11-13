using Rxbit.RdpAppUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Rxbit.RdpAppUI.Services
{
    public class FileTransferService : IDisposable
    {
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private readonly int _bufferSize = 8192;

        public event Action<FileTransferInfo> TransferProgressChanged;
        public event Action<string> TransferCompleted;
        public event Action<string, string> TransferFailed;

        public async Task<bool> ConnectAsync(string host, int port)
        {
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(host, port);
                _networkStream = _tcpClient.GetStream();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"文件传输服务连接失败: {ex.Message}");
            }
        }

        public async Task UploadFileAsync(string localFilePath, string remoteFilePath)
        {
            var fileInfo = new FileInfo(localFilePath);
            var transferInfo = new FileTransferInfo
            {
                FileName = fileInfo.Name,
                FileSize = fileInfo.Length,
                Direction = TransferDirection.Upload,
                Status = TransferStatus.InProgress
            };

            try
            {
                // 发送文件元数据
                byte[] metadata = System.Text.Encoding.UTF8.GetBytes(
                    $"UPLOAD|{remoteFilePath}|{fileInfo.Length}|{fileInfo.Name}");
                await SendCommandAsync(metadata);

                // 发送文件内容
                using (var fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[_bufferSize];
                    int bytesRead;
                    long totalBytes = 0;

                    while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await _networkStream.WriteAsync(buffer, 0, bytesRead);
                        totalBytes += bytesRead;
                        transferInfo.BytesTransferred = totalBytes;
                        TransferProgressChanged?.Invoke(transferInfo);
                    }
                }

                transferInfo.Status = TransferStatus.Completed;
                TransferCompleted?.Invoke($"文件上传成功: {fileInfo.Name}");
            }
            catch (Exception ex)
            {
                transferInfo.Status = TransferStatus.Failed;
                TransferFailed?.Invoke(fileInfo.Name, ex.Message);
            }
        }

        public async Task DownloadFileAsync(string remoteFilePath, string localFilePath)
        {
            var transferInfo = new FileTransferInfo
            {
                FileName = Path.GetFileName(remoteFilePath),
                Direction = TransferDirection.Download,
                Status = TransferStatus.InProgress
            };

            try
            {
                // 发送下载请求
                byte[] request = System.Text.Encoding.UTF8.GetBytes($"DOWNLOAD|{remoteFilePath}");
                await SendCommandAsync(request);

                // 接收文件大小
                byte[] sizeBuffer = new byte[8];
                await _networkStream.ReadAsync(sizeBuffer, 0, 8);
                long fileSize = BitConverter.ToInt64(sizeBuffer, 0);
                transferInfo.FileSize = fileSize;

                // 接收文件内容
                using (var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = new byte[_bufferSize];
                    int bytesRead;
                    long totalBytes = 0;

                    while (totalBytes < fileSize &&
                           (bytesRead = await _networkStream.ReadAsync(buffer, 0,
                               (int)Math.Min(_bufferSize, fileSize - totalBytes))) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        totalBytes += bytesRead;
                        transferInfo.BytesTransferred = totalBytes;
                        TransferProgressChanged?.Invoke(transferInfo);
                    }
                }

                transferInfo.Status = TransferStatus.Completed;
                TransferCompleted?.Invoke($"文件下载成功: {transferInfo.FileName}");
            }
            catch (Exception ex)
            {
                transferInfo.Status = TransferStatus.Failed;
                TransferFailed?.Invoke(transferInfo.FileName, ex.Message);
            }
        }

        private async Task SendCommandAsync(byte[] data)
        {
            byte[] lengthPrefix = BitConverter.GetBytes(data.Length);
            await _networkStream.WriteAsync(lengthPrefix, 0, 4);
            await _networkStream.WriteAsync(data, 0, data.Length);
        }

        public void Disconnect()
        {
            _networkStream?.Close();
            _tcpClient?.Close();
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
