using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace TwitchIRC
{
    class MainProgram
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            try
            {
                Application.Run(new ChatWindow());
            }
            catch (InvalidOperationException ex) { Console.WriteLine(ex.StackTrace); }

            catch (TargetInvocationException ex) { Console.WriteLine(ex.StackTrace); }
        }
    }
}
