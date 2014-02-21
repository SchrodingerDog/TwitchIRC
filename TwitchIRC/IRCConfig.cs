using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TwitchIRC
{
    internal class IRCConfig
    {
        public Dictionary<string, string> data = new Dictionary<string, string>
    {
        { "server", null },
        { "port", null },
        { "nick", null },
        { "name", null },
        { "oauth", null },
        { "channel", null }
    };
        public static IRCConfig ReadConfig(string url)
        {
            XElement root = XElement.Load(url);
            IRCConfig config = new IRCConfig();
            IEnumerable<XElement> elements = from el in root.Elements("config") where (string)el.Attribute("type") != "" select el;
            foreach (XElement el in elements)
            {
                config.data[el.Attribute("type").Value] = el.Value;
            }
            return config;
        }
    }
}
