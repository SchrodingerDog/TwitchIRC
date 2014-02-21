using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TwitchIRC
{
    class IRCBot : IDisposable
    {
        TcpClient IRCConnection = null;
        bool Joined = false;
        bool Debug = false;
        public bool JSONWindow = true;
        IRCConfig config;
        NetworkStream ns = null;
        StreamReader sr = null;
        StreamWriter sw = null;
        Thread WorkThread;
        private Dictionary<string, Action<string, string>> gFunc;
        //public IRCBot(IRCConfig config, ref Dictionary<string, Control> components)
        public IRCBot(IRCConfig config)
        {
            this.config = config;
            //this.Components = components;
        }

        public IRCBot(IRCConfig config, Dictionary<string, Action<string, string>> gFunc)
        {
            this.config = config;
            this.gFunc = gFunc;
        }

        public void Connect()
        {
            try
            {
                IRCConnection = new TcpClient(config.data["server"], int.Parse(config.data["port"]));
            }
            catch (SocketException)
            {
                Console.WriteLine("Connection Error");
            }

            try
            {
                ns = IRCConnection.GetStream();
                sr = new StreamReader(ns);
                sw = new StreamWriter(ns);
                SendData("PASS", config.data["oauth"]);
                SendData("USER", config.data["nick"] + config.data["name"]);
                SendData("NICK", config.data["nick"]);
            }
            catch (Exception)
            {
                Console.WriteLine("Communication error");
            }
            JoinChannel(config.data["channel"]);
            WorkThread = new Thread(() => { IRCWork(); });
            WorkThread.Start();
        }

        private void JoinChannel(string p)
        {
            SendData("JOIN", config.data["channel"]);
            Joined = true;
            Console.WriteLine("Joined");
        }

        private void IRCWork()
        {
            while (!Joined) { }
            //TwitchAPI api = new TwitchAPI();
            //var response = await api.GetFromLink("https://api.twitch.tv/kraken");
            //Console.WriteLine(response["_links"]);
            bool running = true;
            string data;
            while (running)
            {
                data = sr.ReadLine();
                if (Debug) Console.WriteLine(data);
                HandlePingPong(data);
                HandleCommand(data);
                HandleMessage(data);
            }
        }

        private void HandlePingPong(string data)
        {
            var ex = data.Split(' ');
            if (ex[0] == "PING")
            {
                SendData("PONG", ex[1]);
            }
        }

        private void HandleMessage(string data)
        {
            var ex = data.Split(' ');
            if (ex.Length >= 4) //is the command received long enough to be a bot command?
            {
                string command = ex[3]; //grab the command sent

                switch (command)
                {
                    case ":!wow":
                        SendData("PRIVMSG", "" + config.data["channel"] + " :Kappa wow wow, much dog such doge Kappa");
                        break;
                    case ":!hi":
                        SendData("PRIVMSG", "" + config.data["channel"] + " :HI! I'm bot Doge");
                        break;
                    case ":!lolking":
                        SendData("PRIVMSG", "" + config.data["channel"] + " :Oh, i see you written !lolking Kappa");
                        break;
                    case ":!runes":
                        SendData("PRIVMSG", "" + config.data["channel"] + " :Oh, i see you written !runes Kappa");
                        break;
                }
            }
        }

        private void HandleCommand(string data)
        {
            string cmd = data.Split(' ')[1];
            switch (cmd)
            {
                case "PRIVMSG":
                    if (data.IndexOf("!") > 0)
                    {
                        string name = data.Substring(1, data.IndexOf("!") - 1);
                        string message = data.Substring(data.IndexOf(':', 1) + 1);
                        if (name == "twitchnotify")
                        {
                            WriteMessage(message);
                            break;
                        }
                        else if (name == "jtv")
                        {
                            if (Debug)
                            {
                                WriteMessage(message, name);
                            }
                            break;
                        }
                        WriteMessage(message, name);
                    }
                    break;
                //case "PART": break;
                //case "JOIN": break;
                default:
                    try
                    {
                        int code;
                        int.TryParse(cmd, out code);
                        if (Debug&&code!=353)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.WriteLine("Wywołano kod {0} do cmd: {1}", code, cmd);
                            Console.ResetColor();
                        }
                        break;
                    }
                    catch (ArgumentException)
                    {
                        if (Debug)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(cmd);
                            Console.ResetColor();
                            break;
                        }
                    }
                    break;
            }
        }
        
        private void WriteMessage(string message, string name="")
        {
            gFunc["add"](message, name);
        }

        private void sendKonamiCode(string channel)
        {
            string[] konami = { "start", "up", "up", "down", "down", "left", "right", "left", "right", "b", "a" };
            SendData("PRIVMSG", "" + channel + " Trying irc konami code Kappa");
            foreach (string s in konami)
            {
                SendData("PRIVMSG", "" + channel + " " + s);
                //System.Threading.Thread.Sleep(1500);
            }
        }

        private void SendData(string cmd, string param = "")
        {
            if (Debug) Console.WriteLine("{0} {1}", cmd, param);
            if (param == "")
            {
                sw.WriteLine(cmd);
                sw.Flush();
            }
            else
            {
                sw.WriteLine(cmd + " " + param);
                sw.Flush();
            }
        }


        public void Dispose()
        {
            if(WorkThread != null)WorkThread.Abort();
            if (ns != null) ns.Close();
            if (sr != null) sr.Close();
            if (sw != null) sw.Close();
            if (IRCConnection != null) IRCConnection.Close();
        }
    }
}
