﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TwitchIRC
{
    public class ConfigBase
    {
        public virtual Dictionary<string, string> Data { get; set; }
        public static ConfigBase ReadConfig(string path, ConfigBase c)
        {
            XElement root = XElement.Load(path);
            //IRCConfig config = new IRCConfig();
            IEnumerable<XElement> elements = from el in root.Elements("config") where (string)el.Attribute("type") != "" select el;
            foreach (XElement el in elements)
            {
                c.Data[el.Attribute("type").Value] = el.Value;
            }
            return c;
        }
    }

    public class IRCConfig : ConfigBase
    {
        private Dictionary<string, string> _d = new Dictionary<string, string>{
        { "server", null },
        { "port", null },
        { "nick", null },
        { "name", null },
        { "oauth", null },
        { "channel", null }};
        public override Dictionary<string, string> Data
        {
            get
            {
                return _d;
            }
            set { _d = value; }
        }
        //public override Dictionary<string, string> Data = 
    }
}
