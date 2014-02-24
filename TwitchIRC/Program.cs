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
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            try
            {
                Application.Run(new ChatWindow());
            }
            catch (InvalidOperationException) { }

            catch (TargetInvocationException) { }
        }
    }
}
