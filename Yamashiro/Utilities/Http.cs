using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Yamashiro.Logging;
using System.Net.Http;
using System.Text;
using System;

namespace Yamashiro.Utilities
{
    // TODO: Add more methods
    public enum HttpMethod
    {
        Get
    }

    public class HttpRequest
    {
        private readonly Dictionary<string, string> Parameters;
        private readonly Dictionary<string, string> Headers;
        private readonly HttpClient http;
        private readonly Logger logger;

        public HttpRequest()
        {
            Parameters = new Dictionary<string, string>();
            Headers = new Dictionary<string, string>();
            logger = new Logger("HttpRequest");
            http = new HttpClient();
        }

        public HttpRequest AddHeader(string key, string value)
        {
            Headers.Add(key, value);
            return this;
        }

        public HttpRequest AddQuery(string key, string value)
        {
            Parameters.Add(key, value);
            return this;
        }

        public async Task<(bool, JObject)> GetAsync(string url) => await RequestAsync(HttpMethod.Get, url);

        private string BuildUri(Uri url)
        {
            if (Parameters.Count == 0) return url.ToString();

            var sb = new StringBuilder('?');
            var first = true;

            foreach (KeyValuePair<string, string> item in Parameters)
            {
                if (!first) sb.Append("&");
                sb.AppendFormat("{0}={1}", Uri.EscapeDataString(item.Key), Uri.EscapeDataString(item.Value));
                first = false;
            }

            return url.ToString() + sb.ToString();
        }

        private async Task<(bool, JObject)> RequestAsync(HttpMethod method, string url)
        {
            logger.Info($"Now making an attempt to make a request to '{url}'");
            http.DefaultRequestHeaders.Accept.Clear();
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            Uri uri = null;
            try
            {
                uri = new Uri(url);
            } catch
            {
                throw new Exception("URL was not a valid url.");
            }

            switch (method)
            {
                case HttpMethod.Get:
                    {
                        var request = await http.GetAsync(uri!);
                        request.EnsureSuccessStatusCode();

                        if (request.IsSuccessStatusCode)
                        {
                            var content = await request.Content.ReadAsStringAsync();
                            return (true, JObject.Parse(content));
                        }
                        else
                        {
                            return (false, null);
                        }
                    }

                default: throw new Exception("Method is not supported at the moment.");
            }
        }
    }
}