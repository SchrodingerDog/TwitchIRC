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
        List<XElement> pendingChanges = new List<XElement>(112);
        private ChatWindow chatWindow;
        //List<XElement> pendingChanges = new List<XElement>();
        /// <summary>
        /// Konstruktor Api Twitcha, dostaje konfiguracje IRCa
        /// </summary>
        /// <param name="config">Konfiguracja IRCa</param>
        public TwitchAPI(IRCConfig config)
        {
            this.config = config;
        }

        public TwitchAPI(IRCConfig config, ChatWindow chatWindow)
        {
            // TODO: Complete member initialization
            this.config = config;
            this.chatWindow = chatWindow;
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
        public void SaveImage(Image img, string name, bool subscriberonly, string regex, string channel = "")
        {
            string path = "emoticons/";
            string subPath = path + channel.Substring(1) + "/";
            if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
            if (!File.Exists(path + "eConfig.xml")) { XDocument.Parse("<emotes></emotes>").Save(path + "eConfig.xml", SaveOptions.None); }
            XElement toAdd = null;
            try
            {
                StringBuilder b = new StringBuilder();
                b.Append("<emote>")
                    .Append("<src>").Append(path + name).Append("</src>")
                    .Append("<subonly>").Append(subscriberonly.ToString()).Append("</subonly>")
                    .Append("<channel>").Append(subscriberonly?channel:"").Append("</channel>")
                    .Append("<today>").Append(DateTime.Now).Append("</today>")
                    .Append("<regex>").Append(regex.Replace("\\", "")).Append("</regex>")
                .Append("</emote>");
                toAdd = XElement.Parse(b.ToString());
                
                //toAdd = XElement.Parse(String.Format("<emote> <src> {0} </src> <subonly> {1} </subonly> <channel> {2} </channel> <today> {3} </today> <regex> {4} </regex> </emote>", name, subscriberonly, subscriberonly ? channel : "", DateTime.Now, regex));

            }
            catch (XmlException ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            var xml = XElement.Load("emoticons/eConfig.xml");
            if (!CheckExistance(toAdd, xml))
            {
                pendingChanges.Add(toAdd);
            }

            if (subscriberonly)
            {
                if (!Directory.Exists(subPath))
                {
                    Directory.CreateDirectory(subPath);
                    if (CheckNeeding(toAdd) || !File.Exists(subPath + name))
                    {
                        img.Save(subPath + name, ImageFormat.Png);
                    }
                }
                else
                {
                    if (CheckNeeding(toAdd) || !File.Exists(subPath + name))
                    {
                        img.Save(subPath + name, ImageFormat.Png);
                    }
                }

            }
            else
            {
                if (CheckNeeding(toAdd) || !File.Exists(path + name))
                {
                    img.Save(path + name, ImageFormat.Png);
                }
            }


        }

        private bool CheckNeeding(XElement toAdd)
        {
            bool sub = bool.Parse(toAdd.Element("subonly").Value);
            var date = toAdd.Element("today").Value;
            var date_date = DateTime.Parse(date);
            if (sub)
            {
                if (DateTime.Now >= date_date.AddDays(int.Parse(config.Data["sub"])))
                {
                    return true;
                }
            }
            else
            {
                if (DateTime.Now >= date_date.AddDays(int.Parse(config.Data["global"])))
                {
                    return true;
                }
            }
            return false;
        }

        private bool CheckExistance(XElement toAdd, XElement src)
        {
            var element = from child in src.Elements() where child.Element("src").Value == toAdd.Element("src").Value select child;
            if (element.Count() > 0) return true;
            return false;
        }
        /// <summary>
        /// Połączenie GetImageFromUrlAsync i SaveImage
        /// </summary>
        public async Task GetAndSaveEmotes()
        {
            var result = await GetEmotesAsync(config.Data["channel"].Remove(0, 1));
            var emotes = result["emoticons"];
            var eCount = emotes.Count();
            PrepareProgressBar(eCount);
            foreach (var item in emotes)
            {
                string name = item["url"].ToString().Substring(item["url"].ToString().LastIndexOf("/") + 1);
                chatWindow.InvokeIfRequired(text =>
                {
                    //chatWindow.textBox2.Text += (text + "\n");
                    chatWindow.ProgressBar.Value++;
                }, name);
                await GetImageFromUrlAsync(item["url"].ToString()).ContinueWith((t) =>
                {
                    SaveImage(t.Result, name, bool.Parse(item["subscriber_only"].ToString()), item["regex"].ToString(), config.Data["channel"]);
                });

            }

            try
            {
                MakeChanges(pendingChanges);
                chatWindow.InvokeIfRequired(t =>
                {
                    //chatWindow.textBox2.Text += (text + "\n");
                    chatWindow.ProgressBar.Visible = false;
                }, 0);
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

        }

        private void PrepareProgressBar(int eCount)
        {
            chatWindow.InvokeIfRequired(c =>
            {
                var pb = chatWindow.ProgressBar;
                pb.Maximum = c;
                pb.Visible = true;
                pb.Value = 0;
            }, eCount);
        }

        private void MakeChanges(List<XElement> pendingChanges)
        {
            XElement xml = XElement.Load("emoticons/eConfig.xml", LoadOptions.None);
            foreach (var item in pendingChanges)
            {
                xml.AddFirst(item);
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
