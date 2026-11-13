using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wpf.CustomControl.Utilities
{
    /// <summary>
    /// HTTP REST API 调用工具类。
    /// 基于 System.Net.Http.HttpClient，支持 GET/POST/PUT/DELETE/PATCH，
    /// 内置 JSON 序列化、自定义 Header、Bearer Token 认证、超时控制。
    /// </summary>
    public class HttpRestApiHelper : IDisposable
    {
        private readonly HttpClient _client;
        private bool _disposed;

        /// <summary>
        /// 创建 HTTP REST API 客户端
        /// </summary>
        /// <param name="baseUrl">基础 URL（可选，如 http://api.example.com/）</param>
        /// <param name="timeout">超时时间，默认 30 秒</param>
        public HttpRestApiHelper(string baseUrl = null, TimeSpan? timeout = null)
        {
            // 在 .NET Framework 4.8 中使用 TLS 1.2 以支持现代 HTTPS
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            _client = new HttpClient
            {
                BaseAddress = string.IsNullOrEmpty(baseUrl) ? null : new Uri(baseUrl),
                Timeout = timeout ?? TimeSpan.FromSeconds(30)
            };
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        #region 属性

        /// <summary>
        /// 当前 HttpClient 实例（用于高级自定义）
        /// </summary>
        public HttpClient Client
        {
            get { return _client; }
        }

        /// <summary>
        /// 获取或设置超时时间
        /// </summary>
        public TimeSpan Timeout
        {
            get { return _client.Timeout; }
            set { _client.Timeout = value; }
        }

        #endregion

        #region 认证

        /// <summary>
        /// 设置 Bearer Token 认证
        /// </summary>
        /// <param name="token">JWT Token 或 Access Token</param>
        public void SetBearerToken(string token)
        {
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// 设置 Basic 认证
        /// </summary>
        public void SetBasicAuth(string username, string password)
        {
            string credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(username + ":" + password));
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", credentials);
        }

        /// <summary>
        /// 添加默认请求头
        /// </summary>
        public void AddDefaultHeader(string name, string value)
        {
            _client.DefaultRequestHeaders.Add(name, value);
        }

        /// <summary>
        /// 移除默认请求头
        /// </summary>
        public void RemoveDefaultHeader(string name)
        {
            _client.DefaultRequestHeaders.Remove(name);
        }

        /// <summary>
        /// 清除认证信息
        /// </summary>
        public void ClearAuth()
        {
            _client.DefaultRequestHeaders.Authorization = null;
        }

        #endregion

        #region GET

        /// <summary>
        /// GET 请求，返回字符串
        /// </summary>
        /// <param name="url">相对或绝对 URL</param>
        /// <param name="headers">额外请求头（可选）</param>
        /// <param name="cancellationToken">取消令牌</param>
        public async Task<string> GetAsync(string url,
            Dictionary<string, string> headers = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (HttpRequestMessage request = CreateRequest(HttpMethod.Get, url, headers))
            using (HttpResponseMessage response = await _client.SendAsync(request, cancellationToken))
            {
                await EnsureSuccessStatusCodeAsync(response);
                return await response.Content.ReadAsStringAsync();
            }
        }

        /// <summary>
        /// GET 请求，反序列化为对象
        /// </summary>
        public async Task<T> GetAsync<T>(string url,
            Dictionary<string, string> headers = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            string json = await GetAsync(url, headers, cancellationToken);
            return JsonHelper.Deserialize<T>(json);
        }

        /// <summary>
        /// GET 请求，返回字节数组（用于下载二进制数据）
        /// </summary>
        public async Task<byte[]> GetBytesAsync(string url,
            Dictionary<string, string> headers = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (HttpRequestMessage request = CreateRequest(HttpMethod.Get, url, headers))
            using (HttpResponseMessage response = await _client.SendAsync(request, cancellationToken))
            {
                await EnsureSuccessStatusCodeAsync(response);
                return await response.Content.ReadAsByteArrayAsync();
            }
        }

        /// <summary>
        /// GET 请求，返回流（用于大文件流式处理）
        /// </summary>
        public async Task<Stream> GetStreamAsync(string url,
            Dictionary<string, string> headers = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (HttpRequestMessage request = CreateRequest(HttpMethod.Get, url, headers))
            {
                HttpResponseMessage response = await _client.SendAsync(request,
                    HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                await EnsureSuccessStatusCodeAsync(response);
                return await response.Content.ReadAsStreamAsync();
            }
        }

        #endregion

        #region POST

        /// <summary>
        /// POST 请求，发送 JSON 内容，返回字符串响应
        /// </summary>
        /// <typeparam name="TBody">请求体类型</typeparam>
        /// <param name="url">URL</param>
        /// <param name="body">请求体对象</param>
        /// <param name="headers">额外请求头</param>
        /// <param name="cancellationToken">取消令牌</param>
        public async Task<string> PostAsync<TBody>(string url, TBody body,
            Dictionary<string, string> headers = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            string json = JsonHelper.Serialize(body);
            return await PostJsonAsync(url, json, headers, cancellationToken);
        }

        /// <summary>
        /// POST 请求，发送 JSON 内容，反序列化响应
        /// </summary>
        public async Task<TResponse> PostAsync<TBody, TResponse>(string url, TBody body,
            Dictionary<string, string> headers = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            string json = await PostAsync(url, body, headers, cancellationToken);
            return JsonHelper.Deserialize<TResponse>(json);
        }

        /// <summary>
        /// POST 请求，直接发送 JSON 字符串
        /// </summary>
        public async Task<string> PostJsonAsync(string url, string json,
            Dictionary<string, string> headers = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (HttpContent content = new StringContent(json, Encoding.UTF8, "application/json"))
            using (HttpRequestMessage request = CreateRequest(HttpMethod.Post, url, headers, content))
            using (HttpResponseMessage response = await _client.SendAsync(request, cancellationToken))
            {
                await EnsureSuccessStatusCodeAsync(response);
                return await response.Content.ReadAsStringAsync();
            }
        }

        /// <summary>
        /// POST 请求，发送表单数据
        /// </summary>
        public async Task<string> PostFormAsync(string url, Dictionary<string, string> formData,
            Dictionary<string, string> headers = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (HttpContent content = new FormUrlEncodedContent(formData))
            using (HttpRequestMessage request = CreateRequest(HttpMethod.Post, url, headers, content))
            using (HttpResponseMessage response = await _client.SendAsync(request, cancellationToken))
            {
                await EnsureSuccessStatusCodeAsync(response);
                return await response.Content.ReadAsStringAsync();
            }
        }

        #endregion

        #region PUT

        /// <summary>
        /// PUT 请求，发送 JSON 内容
        /// </summary>
        public async Task<string> PutAsync<TBody>(string url, TBody body,
            Dictionary<string, string> headers = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            string json = JsonHelper.Serialize(body);
            using (HttpContent content = new StringContent(json, Encoding.UTF8, "application/json"))
            using (HttpRequestMessage request = CreateRequest(HttpMethod.Put, url, headers, content))
            using (HttpResponseMessage response = await _client.SendAsync(request, cancellationToken))
            {
                await EnsureSuccessStatusCodeAsync(response);
                return await response.Content.ReadAsStringAsync();
            }
        }

        /// <summary>
        /// PUT 请求，发送 JSON 内容，反序列化响应
        /// </summary>
        public async Task<TResponse> PutAsync<TBody, TResponse>(string url, TBody body,
            Dictionary<string, string> headers = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            string json = await PutAsync(url, body, headers, cancellationToken);
            return JsonHelper.Deserialize<TResponse>(json);
        }

        #endregion

        #region DELETE

        /// <summary>
        /// DELETE 请求
        /// </summary>
        public async Task<string> DeleteAsync(string url,
            Dictionary<string, string> headers = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (HttpRequestMessage request = CreateRequest(HttpMethod.Delete, url, headers))
            using (HttpResponseMessage response = await _client.SendAsync(request, cancellationToken))
            {
                await EnsureSuccessStatusCodeAsync(response);
                return await response.Content.ReadAsStringAsync();
            }
        }

        /// <summary>
        /// DELETE 请求，反序列化响应
        /// </summary>
        public async Task<T> DeleteAsync<T>(string url,
            Dictionary<string, string> headers = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            string json = await DeleteAsync(url, headers, cancellationToken);
            return JsonHelper.Deserialize<T>(json);
        }

        #endregion

        #region PATCH

        /// <summary>
        /// PATCH 请求，发送 JSON 内容
        /// </summary>
        public async Task<string> PatchAsync<TBody>(string url, TBody body,
            Dictionary<string, string> headers = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            string json = JsonHelper.Serialize(body);
            using (HttpContent content = new StringContent(json, Encoding.UTF8, "application/json"))
            using (HttpRequestMessage request = CreateRequest(new HttpMethod("PATCH"), url, headers, content))
            using (HttpResponseMessage response = await _client.SendAsync(request, cancellationToken))
            {
                await EnsureSuccessStatusCodeAsync(response);
                return await response.Content.ReadAsStringAsync();
            }
        }

        #endregion

        #region 通用请求

        /// <summary>
        /// 通用请求方法
        /// </summary>
        /// <param name="method">HTTP 方法</param>
        /// <param name="url">URL</param>
        /// <param name="content">请求体（可选）</param>
        /// <param name="headers">额外请求头</param>
        /// <param name="cancellationToken">取消令牌</param>
        public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url,
            HttpContent content = null, Dictionary<string, string> headers = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (HttpRequestMessage request = CreateRequest(method, url, headers, content))
            {
                return await _client.SendAsync(request, cancellationToken);
            }
        }

        #endregion

        #region 内部辅助方法

        private HttpRequestMessage CreateRequest(HttpMethod method, string url,
            Dictionary<string, string> headers, HttpContent content = null)
        {
            HttpRequestMessage request = new HttpRequestMessage(method, url);
            if (content != null)
            {
                request.Content = content;
            }
            if (headers != null)
            {
                foreach (var kv in headers)
                {
                    request.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                }
            }
            return request;
        }

        private async Task EnsureSuccessStatusCodeAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            string errorContent = string.Empty;
            try
            {
                errorContent = await response.Content.ReadAsStringAsync();
            }
            catch
            {
                // 忽略读取异常
            }

            throw new HttpRequestException(string.Format(
                "HTTP {0} {1}: {2}",
                (int)response.StatusCode,
                response.StatusCode,
                errorContent));
        }

        #endregion

        #region 静态便捷方法

        /// <summary>
        /// 静态 GET 请求（每次创建新实例，适合简单一次性调用）
        /// </summary>
        public static async Task<string> Get(string url,
            Dictionary<string, string> headers = null, TimeSpan? timeout = null)
        {
            using (var helper = new HttpRestApiHelper(null, timeout))
            {
                return await helper.GetAsync(url, headers);
            }
        }

        /// <summary>
        /// 静态 GET 请求（反序列化为对象）
        /// </summary>
        public static async Task<T> Get<T>(string url,
            Dictionary<string, string> headers = null, TimeSpan? timeout = null)
        {
            using (var helper = new HttpRestApiHelper(null, timeout))
            {
                return await helper.GetAsync<T>(url, headers);
            }
        }

        /// <summary>
        /// 静态 POST 请求（发送 JSON）
        /// </summary>
        public static async Task<string> Post<TBody>(string url, TBody body,
            Dictionary<string, string> headers = null, TimeSpan? timeout = null)
        {
            using (var helper = new HttpRestApiHelper(null, timeout))
            {
                return await helper.PostAsync(url, body, headers);
            }
        }

        /// <summary>
        /// 静态 POST 请求（反序列化响应）
        /// </summary>
        public static async Task<TResponse> Post<TBody, TResponse>(string url, TBody body,
            Dictionary<string, string> headers = null, TimeSpan? timeout = null)
        {
            using (var helper = new HttpRestApiHelper(null, timeout))
            {
                return await helper.PostAsync<TBody, TResponse>(url, body, headers);
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (!_disposed)
            {
                _client.Dispose();
                _disposed = true;
            }
        }

        #endregion
    }
}
