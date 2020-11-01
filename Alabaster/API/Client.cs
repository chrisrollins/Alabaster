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

        /// <summary>Sends an HTTP request.</summary>
        public static async Task<string> Get(string url, HTTPScheme scheme = HTTPScheme.HTTP) => await Request(HTTPMethod.GET, url, "", scheme);

        /// <summary>Sends an HTTP request.</summary>
        public static async Task<string> Delete(string url, HTTPScheme scheme = HTTPScheme.HTTP) => await Request(HTTPMethod.DELETE, url, "", scheme);

        /// <summary>Sends an HTTP request.</summary>
        public static async Task<string> Post(string url, string body, HTTPScheme scheme = HTTPScheme.HTTP) => await Request(HTTPMethod.POST, url, body, scheme);

        /// <summary>Sends an HTTP request.</summary>
        public static async Task<string> Patch(string url, string body, HTTPScheme scheme = HTTPScheme.HTTP) => await Request(HTTPMethod.PATCH, url, body, scheme);

        /// <summary>Sends an HTTP request.</summary>
        public static async Task<string> Put(string url, string body, HTTPScheme scheme = HTTPScheme.HTTP) => await Request(HTTPMethod.PUT, url, body, scheme);

        /// <summary>Sends an HTTP request.</summary>
        public static async Task<string> Request(HTTPMethod method, string url, string body, HTTPScheme scheme = HTTPScheme.HTTP) => await Request(method.ToString(), url, body, scheme);

        /// <summary>Sends an HTTP request.</summary>
        public static async Task<string> Request(string method, string url, string body, HTTPScheme scheme = HTTPScheme.HTTP)
        {            
            if (scheme != HTTPScheme.HTTP && scheme != HTTPScheme.HTTPS) { throw new ArgumentException("HTTP scheme must be HTTP or HTTPS."); }
            if (url.Substring(0, 4).ToUpper() == "HTTP") { throw new ArgumentException("HTTP scheme must not be defined in the URL."); }            
            string fullURL = string.Join(null, scheme.ToString().ToLower() , "://" , url);            
            return await InternalExceptionHandler.Try(async () =>
            {
                using HttpRequestMessage msg = new HttpRequestMessage(new HttpMethod(method), fullURL);
                if (!(new string[] { "GET", "HEAD" }).Contains(method.ToUpper())) { msg.Content = new StringContent(body ?? ""); }
                using (HttpResponseMessage res = await client.SendAsync(msg, HttpCompletionOption.ResponseContentRead))
                return await res.Content.ReadAsStringAsync();                    
            });
            
        }

    }
}
