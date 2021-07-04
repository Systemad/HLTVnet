using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace HLTVnet.Parsing
{
    public static partial class HLTVParser
    {
        private static async Task<T> FetchPage<T>(string url, Func<Task<HttpResponseMessage>, T> continueWith, WebProxy proxy = null)
        {
            var httpClientHandler = new HttpClientHandler();

            if (proxy != null)
            {
                httpClientHandler.UseProxy = true;
                httpClientHandler.Proxy = proxy;
            }

            var client = new HttpClient(httpClientHandler);
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri("https://www.hltv.org/" + url),
                Method = HttpMethod.Get,
            };

            request.Headers.Add("Referer", "https://www.hltv.org/stats");
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:66.0) Gecko/20100101 Firefox/66.0");
            T result = await client.SendAsync(request).ContinueWith((response) => continueWith(response));
            return result;
        }

        // TODO: Remove these time functions and use inbuilt ones
        /*
         *  //Match date
         *  long unixDateMilliseconds = long.Parse(upcomingMatchNode.Attributes["data-zonedgrouping-entry-unix"].Value);
         *  string dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(unixDateMilliseconds).ToString();
         *  model.Date = DateTime.Parse(dateTimeOffset);
         */
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        public static DateTime DateTimeFromUnixTimestampMillis(long millis)
        {
            return UnixEpoch.AddMilliseconds(millis);
        }
    }
}
