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
using System.Xml;
using System.Xml.Linq;

namespace TwitchIRC
{
    class TwitchAPI
    {
        IRCConfig config = null;
        List<XElement> pendingChanges = new List<XElement>();
        /// <summary>
        /// Konstruktor Api Twitcha, dostaje konfiguracje IRCa
        /// </summary>
        /// <param name="config">Konfiguracja IRCa</param>
        public TwitchAPI(IRCConfig config)
        {
            this.config = config;
        }
        /// <summary>
        /// Prywatna funkcja, pobiera ona JSON z strony i zwraca go jako obiekt JObject
        /// </summary>
        /// <param name="link">Adres URL JSONa</param>
        /// <returns>JObject zwraca</returns>
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
        /// <summary>
        /// Wrapper funkcji GetJSONAsync wymuszający użycia bazowego URL TwitchApi
        /// </summary>
        /// <returns></returns>
        public async Task<JObject> GetBaseTwitchJSONAsync()
        {
            return await GetJSONAsync("https://api.twitch.tv/kraken/");
        }
        /// <summary>
        /// Wrapper dla GetJSONAsync, wykonany z powodu braku w _links linku do channels
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public async Task<JObject> GetChannelsInfoJSONAsync(string channel)
        {
            return await GetJSONAsync("https://api.twitch.tv/kraken/channels/" + channel);
        }
        /// <summary>
        /// Pobiera informacje o określonej liczbie streamów
        /// </summary>
        /// <param name="limit">Ilość streamów do przesłania, max 100</param>
        /// <param name="offset">Przesunięcie od początku listy streamów</param>
        /// <returns></returns>
        public async Task<JObject> GetStreamsInfoJSONAsync(int limit, int offset = 0)
        {
            var json = await GetBaseTwitchJSONAsync();
            var streams = json["_links"]["streams"].ToString();
            return await GetJSONAsync(String.Format("{0}?{1}&{2}", streams, limit, offset));
        }
        /// <summary>
        /// Pobranie JSONa z emotikonami danego channela Twitcha
        /// </summary>
        /// <param name="channel">Kanał z którego mają być pobrane emotikony</param>
        /// <returns></returns>
        public async Task<JObject> GetEmotesAsync(string channel)
        {
            return await GetChannelsInfoJSONAsync(channel).ContinueWith(t =>
            {
                return GetJSONAsync(t.Result["_links"]["chat"].ToString());
            }).ContinueWith(t =>
            {
                return GetJSONAsync(t.Unwrap().Result["_links"]["emoticons"].ToString());
            }).Unwrap();
        }
        /// <summary>
        /// Pobiera obraz i zapamiętuje go w obiekcie typu Image
        /// </summary>
        /// <param name="url">URL z którego ma być pobrany obraz</param>
        /// <returns></returns>
        public async Task<Image> GetImageFromUrlAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                using (Stream responseBody = await client.GetStreamAsync(url))
                {
                    Image img = Image.FromStream(responseBody);
                    return img;

                }
            }
        }
        /// <summary>
        /// Zapisanie obrazu na dysk
        /// </summary>
        /// <param name="img">Obraz do zapisania</param>
        /// <param name="name">nazwa pod którą ma być obraz zapisany</param>
        /// <param name="subscriberonly">Określa czy emotikon jest tylko dla subskrybentów i ma być umieszczony w folderze kanału</param>
        /// <param name="channel">Kanał dla którego ten emotikon istnieje</param>
        public void SaveImage(Image img, string name, bool subscriberonly, string channel = "")
        {
            string path = "emoticons/";
            if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
            if (!File.Exists(path + "eConfig.xml")) { XDocument.Parse("<emotes></emotes>").Save(path + "eConfig.xml", SaveOptions.None); }
            XElement toAdd = null;
            try
            {
                toAdd = XElement.Parse(String.Format("<emote> <src> {0} </src> <subonly> {1} </subonly> <channel> {2} </channel> <today> {3} </today> </emote>", path + name, subscriberonly, channel, DateTime.Now));

            }
            catch (XmlException ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            if (!CheckExistance(toAdd))
            {
                pendingChanges.Add(toAdd);
            }
            if (CheckNeeding(toAdd) || !File.Exists(path+name))
            {
                img.Save(path + name, ImageFormat.Png);
            }
        }

        private bool CheckNeeding(XElement toAdd)
        {
            bool sub = bool.Parse(toAdd.Element("subonly").Value);
            var date = toAdd.Element("today").Value;
            var date_date = DateTime.Parse(date);
            if (sub)
            {
                if (DateTime.Now >= date_date.AddDays(30))
                {
                    return true;
                }
            }
            else
            {
                if (DateTime.Now >= date_date.AddDays(60))
                {
                    return true;
                } 
            }
            return false;
        }

        private bool CheckExistance(XElement toAdd)
        {
            var xml = XElement.Load("emoticons/eConfig.xml");
            var element = from child in xml.Elements() where child.Element("src").Value == toAdd.Element("src").Value select child;
            if (element.Count() > 0) return true;
            return false;
        }
        /// <summary>
        /// Połączenie GetImageFromUrlAsync i SaveImage
        /// </summary>
        public void GetAndSaveEmotes()
        {
            GetEmotesAsync(config.Data["channel"].Remove(0, 1)).ContinueWith(t =>
            {
                foreach (var item in t.Result["emoticons"])
                {
                    new System.Threading.Thread(() =>
                    {
                        string name = item["url"].ToString().Substring(item["url"].ToString().LastIndexOf("/") + 1);
                        GetImageFromUrlAsync(item["url"].ToString()).ContinueWith((r =>
                        {
                            SaveImage(r.Result, name, bool.Parse(item["subscriber_only"].ToString()), config.Data["channel"]);
                        }));

                    }).Start();
                }
            }).ContinueWith(r =>
            {
                try
                {
                    MakeChanges(pendingChanges);
                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex.StackTrace);
                }
            });
        }

        private void MakeChanges(List<XElement> pendingChanges)
        {
            XElement xml = XElement.Load("emoticons/eConfig.xml", LoadOptions.None);
            foreach (var item in pendingChanges)
            {
                xml.Add(item);
            }
            xml.Save("emoticons/eConfig.xml", SaveOptions.None);
        }
    }
}
//TODO
//1.XML dla pobierania emotków, by miały jakiś okres w którym są ważne, długi dla global, krótszy dla dub-only
//2.Jakieś fajne UI
//3.Wybór channela
//4.Okno konfiguracji
//5.Może jakaś elastyczna funkcja do tych invoke'ów
