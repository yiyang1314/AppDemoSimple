using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Wpf.CustomControl.Utilities
{
    /// <summary>
    /// 文件下载工具类。
    /// 支持进度回调、断点续传、取消下载，适用于 HTTP/HTTPS 大文件下载。
    /// </summary>
    public static class FileDownloadHelper
    {
        #region 同步下载

        /// <summary>
        /// 同步下载文件（简单模式，无进度回调）
        /// </summary>
        /// <param name="url">下载地址</param>
        /// <param name="savePath">本地保存路径</param>
        /// <param name="overwrite">是否覆盖已存在文件</param>
        public static void Download(string url, string savePath, bool overwrite = true)
        {
            DownloadAsync(url, savePath, overwrite, null, CancellationToken.None).Wait();
        }

        #endregion

        #region 异步下载

        /// <summary>
        /// 异步下载文件（无进度回调）
        /// </summary>
        /// <param name="url">下载地址</param>
        /// <param name="savePath">本地保存路径</param>
        /// <param name="overwrite">是否覆盖已存在文件</param>
        /// <param name="cancellationToken">取消令牌</param>
        public static async Task DownloadAsync(string url, string savePath, bool overwrite = true,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await DownloadAsync(url, savePath, overwrite, null, cancellationToken);
        }

        /// <summary>
        /// 异步下载文件（带进度回调）
        /// </summary>
        /// <param name="url">下载地址</param>
        /// <param name="savePath">本地保存路径</param>
        /// <param name="overwrite">是否覆盖已存在文件</param>
        /// <param name="progress">进度回调（参数：DownloadProgress）</param>
        /// <param name="cancellationToken">取消令牌</param>
        public static async Task DownloadAsync(string url, string savePath, bool overwrite = true,
            IProgress<DownloadProgress> progress = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }
            if (string.IsNullOrEmpty(savePath))
            {
                throw new ArgumentNullException("savePath");
            }

            // 启用 TLS 1.2
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            FileHelper.EnsureDirectory(savePath);
            if (overwrite && File.Exists(savePath))
            {
                File.Delete(savePath);
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.UserAgent = "Wpf.CustomControl.FileDownloadHelper/1.0";

            using (WebResponse response = await GetResponseAsync(request, cancellationToken))
            using (Stream responseStream = response.GetResponseStream())
            using (FileStream fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write))
            {
                long totalBytes = response.ContentLength;
                byte[] buffer = new byte[8192];
                int bytesRead;
                long downloadedBytes = 0;
                DateTime lastReportTime = DateTime.UtcNow;

                while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                    downloadedBytes += bytesRead;

                    // 节流进度回调，避免回调过于频繁
                    if (progress != null &&
                        (DateTime.UtcNow - lastReportTime).TotalMilliseconds >= 100)
                    {
                        progress.Report(new DownloadProgress(downloadedBytes, totalBytes));
                        lastReportTime = DateTime.UtcNow;
                    }
                }

                // 最终进度
                if (progress != null)
                {
                    progress.Report(new DownloadProgress(downloadedBytes, totalBytes));
                }
            }
        }

        #endregion

        #region 断点续传

        /// <summary>
        /// 异步下载文件（支持断点续传）
        /// </summary>
        /// <param name="url">下载地址</param>
        /// <param name="savePath">本地保存路径（已存在的部分文件会继续追加）</param>
        /// <param name="progress">进度回调</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <remarks>
        /// 断点续传要求服务器支持 Range 请求（返回 206 Partial Content）。
        /// 若服务器不支持，将从头下载。
        /// </remarks>
        public static async Task DownloadWithResumeAsync(string url, string savePath,
            IProgress<DownloadProgress> progress = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }
            if (string.IsNullOrEmpty(savePath))
            {
                throw new ArgumentNullException("savePath");
            }

            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            FileHelper.EnsureDirectory(savePath);

            // 检查本地已下载大小
            long existingLength = 0;
            if (File.Exists(savePath))
            {
                existingLength = new FileInfo(savePath).Length;
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.UserAgent = "Wpf.CustomControl.FileDownloadHelper/1.0";
            if (existingLength > 0)
            {
                request.AddRange(existingLength);
            }

            using (WebResponse response = await GetResponseAsync(request, cancellationToken))
            {
                Stream responseStream = response.GetResponseStream();

                // 判断是否支持续传（206 响应表示支持）
                bool canResume = existingLength > 0 &&
                    response is HttpWebResponse &&
                    ((HttpWebResponse)response).StatusCode == HttpStatusCode.PartialContent;

                long totalBytes = response.ContentLength;
                if (canResume)
                {
                    totalBytes += existingLength; // 续传时 ContentLength 为剩余部分
                }

                FileMode fileMode = canResume ? FileMode.Append : FileMode.Create;
                long downloadedBytes = canResume ? existingLength : 0;

                using (FileStream fileStream = new FileStream(savePath, fileMode, FileAccess.Write))
                {
                    byte[] buffer = new byte[8192];
                    int bytesRead;
                    DateTime lastReportTime = DateTime.UtcNow;

                    while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                        downloadedBytes += bytesRead;

                        if (progress != null &&
                            (DateTime.UtcNow - lastReportTime).TotalMilliseconds >= 100)
                        {
                            progress.Report(new DownloadProgress(downloadedBytes, totalBytes));
                            lastReportTime = DateTime.UtcNow;
                        }
                    }

                    if (progress != null)
                    {
                        progress.Report(new DownloadProgress(downloadedBytes, totalBytes));
                    }
                }
            }
        }

        #endregion

        #region 带请求头下载

        /// <summary>
        /// 异步下载文件（带自定义请求头，常用于需要 Authorization 的下载）
        /// </summary>
        /// <param name="url">下载地址</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="headers">请求头字典</param>
        /// <param name="overwrite">是否覆盖</param>
        /// <param name="progress">进度回调</param>
        /// <param name="cancellationToken">取消令牌</param>
        public static async Task DownloadWithHeadersAsync(string url, string savePath,
            Dictionary<string, string> headers, bool overwrite = true,
            IProgress<DownloadProgress> progress = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }
            if (string.IsNullOrEmpty(savePath))
            {
                throw new ArgumentNullException("savePath");
            }

            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            FileHelper.EnsureDirectory(savePath);
            if (overwrite && File.Exists(savePath))
            {
                File.Delete(savePath);
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.UserAgent = "Wpf.CustomControl.FileDownloadHelper/1.0";
            if (headers != null)
            {
                foreach (var kv in headers)
                {
                    if (kv.Key.Equals("User-Agent", StringComparison.OrdinalIgnoreCase))
                    {
                        request.UserAgent = kv.Value;
                    }
                    else if (kv.Key.Equals("Referer", StringComparison.OrdinalIgnoreCase))
                    {
                        request.Referer = kv.Value;
                    }
                    else if (kv.Key.Equals("Accept", StringComparison.OrdinalIgnoreCase))
                    {
                        request.Accept = kv.Value;
                    }
                    else
                    {
                        request.Headers.Set(kv.Key, kv.Value);
                    }
                }
            }

            using (WebResponse response = await GetResponseAsync(request, cancellationToken))
            using (Stream responseStream = response.GetResponseStream())
            using (FileStream fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write))
            {
                long totalBytes = response.ContentLength;
                byte[] buffer = new byte[8192];
                int bytesRead;
                long downloadedBytes = 0;
                DateTime lastReportTime = DateTime.UtcNow;

                while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                    downloadedBytes += bytesRead;

                    if (progress != null &&
                        (DateTime.UtcNow - lastReportTime).TotalMilliseconds >= 100)
                    {
                        progress.Report(new DownloadProgress(downloadedBytes, totalBytes));
                        lastReportTime = DateTime.UtcNow;
                    }
                }

                if (progress != null)
                {
                    progress.Report(new DownloadProgress(downloadedBytes, totalBytes));
                }
            }
        }

        #endregion

        #region 下载为字节数组

        /// <summary>
        /// 异步下载文件到内存（返回字节数组），适合小文件
        /// </summary>
        /// <param name="url">下载地址</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>文件字节数组</returns>
        public static async Task<byte[]> DownloadBytesAsync(string url,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }

            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.UserAgent = "Wpf.CustomControl.FileDownloadHelper/1.0";

            using (WebResponse response = await GetResponseAsync(request, cancellationToken))
            using (Stream responseStream = response.GetResponseStream())
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] buffer = new byte[8192];
                int bytesRead;
                while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    await ms.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                }
                return ms.ToArray();
            }
        }

        #endregion

        #region 内部辅助

        private static Task<WebResponse> GetResponseAsync(WebRequest request, CancellationToken cancellationToken)
        {
            // .NET Framework 4.8 的 GetResponseAsync 不直接支持 CancellationToken
            // 通过 TaskCompletionSource + cancellationToken.Register 实现
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<WebResponse>(cancellationToken);
            }

            TaskCompletionSource<WebResponse> tcs = new TaskCompletionSource<WebResponse>();
            cancellationToken.Register(() =>
            {
                if (request != null)
                {
                    request.Abort();
                }
                tcs.TrySetCanceled();
            });

            request.BeginGetResponse(ar =>
            {
                try
                {
                    WebResponse response = request.EndGetResponse(ar);
                    tcs.TrySetResult(response);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }, null);

            return tcs.Task;
        }

        #endregion
    }

    /// <summary>
    /// 下载进度信息
    /// </summary>
    public class DownloadProgress
    {
        /// <summary>
        /// 已下载字节数
        /// </summary>
        public long DownloadedBytes { get; private set; }

        /// <summary>
        /// 总字节数（服务器未返回时为 -1）
        /// </summary>
        public long TotalBytes { get; private set; }

        /// <summary>
        /// 下载进度百分比（0-100，TotalBytes 未知时为 -1）
        /// </summary>
        public double Percent
        {
            get
            {
                if (TotalBytes <= 0)
                {
                    return -1;
                }
                return Math.Round((double)DownloadedBytes / TotalBytes * 100, 2);
            }
        }

        /// <summary>
        /// 已下载大小（格式化字符串，如 "1.23 MB"）
        /// </summary>
        public string DownloadedSizeText
        {
            get { return FileHelper.FormatFileSize(DownloadedBytes); }
        }

        /// <summary>
        /// 总大小（格式化字符串）
        /// </summary>
        public string TotalSizeText
        {
            get { return TotalBytes > 0 ? FileHelper.FormatFileSize(TotalBytes) : "未知"; }
        }

        /// <summary>
        /// 下载是否完成
        /// </summary>
        public bool IsCompleted
        {
            get { return TotalBytes > 0 && DownloadedBytes >= TotalBytes; }
        }

        public DownloadProgress(long downloadedBytes, long totalBytes)
        {
            DownloadedBytes = downloadedBytes;
            TotalBytes = totalBytes;
        }

        public override string ToString()
        {
            if (TotalBytes > 0)
            {
                return string.Format("{0} / {1} ({2:F2}%)",
                    DownloadedSizeText, TotalSizeText, Percent);
            }
            return DownloadedSizeText;
        }
    }
}
