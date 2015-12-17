using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SyncLinks.Models
{
    public class HttpTool
    {
        private static HttpClient _client = GetHttpClient();

        private static HttpClient GetHttpClient()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://getpocket.com/");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
            client.DefaultRequestHeaders.Add("X-Accept", "application/json");
            client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows Phone 8.0; Trident/6.0; ARM; Touch; IEMobile/10.0;)");
            return client;
        }

        /// <summary>
        /// 开始一个异步任务来获取GET请求内容。
        /// </summary>
        /// <param name="url">请求链接</param>
        /// <returns>一个异步任务对象，任务完成后可通过其 Result 属性获取返回的流内容</returns>
        public static async Task<Stream> GetStreamAsync(string url, Preference preference = null)
        {
            Uri uri = new Uri(url);
            HttpClient client;
            if (preference != null)
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.Credentials = new NetworkCredential(preference.Username, preference.Password);
                client = new HttpClient(handler);
            }
            else client = _client;
            HttpResponseMessage response = await client.GetAsync(uri);
            Debug.WriteLine("code in repsonse message: {0}, url: {1}", response.StatusCode, url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync();
        }

        /// <summary>
        /// 开始一个异步任务来获取POST请求内容
        /// </summary>
        /// <param name="url">请求链接</param>
        /// <param name="data">发送内容</param>
        /// <returns>一个异步任务对象，任务完成后可通过其 Result 属性获取返回的流内容</returns>
        public static async Task<Stream> PostStreamAsync(string url, string data, Preference preference = null)
        {
            HttpClient client;
            if (preference != null)
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.Credentials = new NetworkCredential(preference.Username, preference.Password);
                client = new HttpClient(handler);
            }
            else client = _client;
            HttpContent content = new StringContent(data, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(url, content);
            Debug.WriteLine("code in repsonse message: {0}, url: {1}", response.StatusCode, url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync();
        }

        /// <summary>
        /// 开始一个异步任务来获取POST请求内容
        /// </summary>
        /// <param name="url">请求链接</param>
        /// <param name="data">发送内容</param>
        /// <returns>一个异步任务对象，任务完成后可通过其 Result 属性获取返回的流内容</returns>
        public static async Task<String> PostStringAsync(string url, string data, Preference preference = null)
        {
            HttpClient client;
            if (preference != null)
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.Credentials = new NetworkCredential(preference.Username, preference.Password);
                client = new HttpClient(handler);
            }
            else client = _client;
            HttpContent content = new StringContent(data, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(url, content);
            Debug.WriteLine("code in repsonse message: {0}, url: {1}", response.StatusCode, url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// 开始一个异步任务来获取POST请求内容
        /// </summary>
        /// <param name="url">请求链接</param>
        /// <param name="data">发送内容</param>
        /// <returns>一个异步任务对象，任务完成后可通过其 Result 属性获取返回的流内容</returns>
        public static async Task<Stream> PostStreamAsync(string url, Stream data, Preference preference = null)
        {
            HttpClient client;
            if (preference != null)
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.Credentials = new NetworkCredential(preference.Username, preference.Password);
                client = new HttpClient(handler);
            }
            else client = _client;
            HttpContent content = new StreamContent(data);
            HttpResponseMessage response = await client.PostAsync(url, content);
            Debug.WriteLine("code in repsonse message: {0}, url: {1}", response.StatusCode, url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync();
        }

        /// <summary>
        /// 开始一个异步任务来获取POST请求内容
        /// </summary>
        /// <param name="url">请求链接</param>
        /// <param name="data">发送内容</param>
        /// <returns>一个异步任务对象，任务完成后可通过其 Result 属性获取返回的流内容</returns>
        public static async Task<String> PostStringAsync(string url, Stream data, Preference preference = null)
        {
            HttpClient client;
            if (preference != null)
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.Credentials = new NetworkCredential(preference.Username, preference.Password);
                client = new HttpClient(handler);
            }
            else client = _client;
            HttpContent content = new StreamContent(data);
            HttpResponseMessage response = await client.PostAsync(url, content);
            Debug.WriteLine("code in repsonse message: {0}, url: {1}", response.StatusCode, url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
