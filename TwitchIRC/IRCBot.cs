﻿using System;
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
        bool Running = true;
        bool Joined = false;
        bool Debug = false;
        bool IsDisposed = false;
        public bool JSONWindow = true;
        IRCConfig config;
        NetworkStream ns = null;
        StreamReader sr = null;
        StreamWriter sw = null;
        Thread WorkThread;
        private Dictionary<string, Action<string, string>> gFunc;
        public IRCBot(IRCConfig config)
        {
            this.config = config;
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
                IRCConnection = new TcpClient(config.Data["server"], int.Parse(config.Data["port"]));
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
                SendData("PASS", config.Data["oauth"]);
                SendData("USER", config.Data["nick"] + config.Data["name"]);
                SendData("NICK", config.Data["nick"]);
            }
            catch (Exception)
            {
                Console.WriteLine("Communication error");
            }
            JoinChannel(config.Data["channel"]);
            WorkThread = new Thread(() => { IRCWork(); });
            WorkThread.Start();
        }

        private void JoinChannel(string p)
        {
            SendData("JOIN", config.Data["channel"]);
            Joined = true;
            Console.WriteLine("Joined");
        }

        private void IRCWork()
        {
            while (!Joined) ;
            SendData("PRIVMSG", "" + config.Data["channel"] + " :Siemanko wszystkim, DogeIRCBot wchodzi na czat!");
            string data;
            while (Running)
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
                        SendData("PRIVMSG", "" + config.Data["channel"] + " :Kappa wow wow, much dog such doge Kappa");
                        break;
                    case ":!hi":
                        SendData("PRIVMSG", "" + config.Data["channel"] + " :HI! I'm bot Doge");
                        break;
                    case ":!lolking":
                        SendData("PRIVMSG", "" + config.Data["channel"] + " :Oh, i see you written !lolking Kappa");
                        break;
                    case ":!runes":
                        SendData("PRIVMSG", "" + config.Data["channel"] + " :Oh, i see you written !runes Kappa");
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
                        if (Debug && code != 353)
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

        private void WriteMessage(string message, string name = "")
        {
            //gFunc["add"](message, name);
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
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    Running = false;
                }
                if (WorkThread != null) WorkThread.Abort();
                if (ns != null) ns.Close();
                if (sr != null) sr.Close();
                if (sw != null) sw.Close();
                if (IRCConnection != null) IRCConnection.Close();
            }
            IsDisposed = true;
        }
        ~IRCBot()
        {
            Dispose(false);
        }
    }
}
