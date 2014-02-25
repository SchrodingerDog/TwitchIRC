using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TwitchIRC
{
    class TwitchAPI
    {
        private async Task<JObject> GetJSONAsync(string link)
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

        public async Task<JObject> GetBaseTwitchJSONAsync()
        {
            return await GetJSONAsync("https://api.twitch.tv/kraken/");
        }

        public async Task<JObject> GetChannelsInfoJSONAsync(string channel)
        {
            return await GetJSONAsync("https://api.twitch.tv/kraken/channels/" + channel);
        }

        public async Task<JObject> GetStreamsInfoJSONAsync(int limit, int offset = 0)
        {
            var json = await GetBaseTwitchJSONAsync();
            var streams = json["_links"]["streams"].ToString();
            return await GetJSONAsync(String.Format("{0}?{1}&{2}", streams, limit, offset));
        }
        public async Task<JObject> GetEmotesAsync(string channel)
        {
            //JObject result;
            //var t1 = GetChannelsInfoJSONAsync(channel);
            //var t2 = t1.ContinueWith(t => { return GetJSONAsync(t.Result["_links"]["chat"].ToString()); });
            //var t3 = t2.ContinueWith(t => { return GetJSONAsync(t.Unwrap().Result["_links"]["emoticons"].ToString()); });
            //return await t3.Unwrap();
            return await GetChannelsInfoJSONAsync(channel).ContinueWith(t =>
            {
                return GetJSONAsync(t.Result["_links"]["chat"].ToString());
            }).ContinueWith(t =>
            {
                return GetJSONAsync(t.Unwrap().Result["_links"]["emoticons"].ToString());
            }).Unwrap();
        }

        public async Task<Image> GetImageFromUrlAsync(string url, bool subscriberonly, string channel = "")
        {
            using (HttpClient client = new HttpClient())
            {
                using (Stream responseBody = await client.GetStreamAsync(url))
                {
                    Image img = Image.FromStream(responseBody);
                    return img;

                }
            }

            //HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);

            //using (HttpWebResponse httpWebReponse = (HttpWebResponse)httpWebRequest.GetResponse())
            //{
            //    using (Stream stream = httpWebReponse.GetResponseStream())
            //    {
            //        return Image.FromStream(stream);
            //    }
            //}
        }
        public void SaveImage(Image img, string name) {
            string path = "emoticons/";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            img.Save(path + name, ImageFormat.Jpeg);
        }
    }
}

