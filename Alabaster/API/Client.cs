using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;

namespace Alabaster
{
    public static class Client
    {
        private static HttpClient client = new HttpClient();   

        public static async Task<string> Get(string url, HTTPScheme scheme = HTTPScheme.HTTP) => await Request("GET", url, "", scheme);
        public static async Task<string> Delete(string url, HTTPScheme scheme = HTTPScheme.HTTP) => await Request("DELETE", url, "", scheme);
        public static async Task<string> Post(string url, string body, HTTPScheme scheme = HTTPScheme.HTTP) => await Request("POST", url, body, scheme);
        public static async Task<string> Patch(string url, string body, HTTPScheme scheme = HTTPScheme.HTTP) => await Request("PATCH", url, body, scheme);
        public static async Task<string> Put(string url, string body, HTTPScheme scheme = HTTPScheme.HTTP) => await Request("PUT", url, body, scheme);

        public static async Task<string> Request(string method, string url, string body, HTTPScheme scheme = HTTPScheme.HTTP)
        {
            if (scheme != HTTPScheme.HTTP && scheme != HTTPScheme.HTTPS) { throw new ArgumentException("HTTP scheme must be HTTP or HTTPS."); }
            if (url.Substring(0, 4).ToUpper() == "HTTP") { throw new ArgumentException("HTTP scheme must not be defined in the URL."); }            
            string fullURL = String.Join(null, scheme.ToString().ToLower() , "://" , url);
            using (HttpRequestMessage msg = new HttpRequestMessage(new HttpMethod(method), fullURL))
            {
                msg.Content = new StringContent(body ?? "");
                try
                {
                    using (HttpResponseMessage res = await client.SendAsync(msg, HttpCompletionOption.ResponseContentRead))
                    {
                        return await res.Content.ReadAsStringAsync();
                    }
                }
                catch (HttpRequestException)
                {
                    return null;
                }
            }
        }

    }
}
