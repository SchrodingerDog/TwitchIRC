using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TwitchIRC
{
    class TwitchAPI
    {
        public async Task<JObject> GetJSONAsync(string link)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.twitchtv.v3+json"));
                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, "relativeAddress");
                Task<string> response = client.GetStringAsync(link);
                string responseBody = await response;
                return JObject.Parse(responseBody);
            }
        }

    }
}

