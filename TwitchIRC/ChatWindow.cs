using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TwitchIRC
{
    public partial class ChatWindow : Form
    {
        IRCConfig config;
        IRCBot bot;
        TwitchAPI api;
        Dictionary<string, Action<string, string>> gFunc = new Dictionary<string, Action<string, string>>();
        public ChatWindow()
        {
            api = new TwitchAPI();
            gFunc.Add("add", AddTextToTextBox);
            config = IRCConfig.ReadConfig(@"config.xml");
            bot = new IRCBot(config, gFunc);
            InitializeComponent();
            
            bot.Connect();
        }

        public void AppendWithColor(RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }

        delegate void TextToTextBox(string message, string name);

        private void AddTextToTextBox(string message, string name)
        {
            if (this.textBox1.InvokeRequired)
            {
                try
                {
                    TextToTextBox d = new TextToTextBox(AddTextToTextBox);
                    this.Invoke(d, new object[] { message, name });
                }
                catch (ObjectDisposedException) { }

            }
            else
            {
                if (name != "")
                {
                    AppendWithColor(textBox1, name, Color.LimeGreen);
                    AppendWithColor(textBox1, ": " + message + "\n", Color.Blue);

                    this.textBox1.SelectionStart = this.textBox1.Text.Length;
                    this.textBox1.ScrollToCaret();
                    this.textBox1.Refresh();
                }
                else
                {
                    AppendWithColor(textBox1, message + "\n", Color.Magenta);
                }
            }
        }
        delegate void TextInLabel(string text);

        private void SetTextInLabel(string text)
        {
            if (this.textBox1.InvokeRequired)
            {
                try
                {
                    TextInLabel d = new TextInLabel(SetTextInLabel);
                    this.Invoke(d, new object[] { text });
                }
                catch (ObjectDisposedException) { }

            }
            else
            {
                label1.Text = text;
            }
        }



        private void textBox1_MouseDown(object sender, MouseEventArgs e)
        {
            HideCaret(textBox1.Handle);
        }

        [DllImport("user32.dll")]
        static extern bool HideCaret(IntPtr h);

        private async void button1_Click(object sender, EventArgs e)
        {
            var response = await api.GetStreamsInfoJSONAsync(1);
            var r = (response["streams"][0]);
            //var text = r.ToString();
            var text = new StringBuilder().AppendFormat("Najpopularniejszy stream: \n{0} - {1} \n ({2})", r["channel"]["name"].ToString(), r["channel"]["status"].ToString(), r["viewers"]).ToString();
            //var text = "Najpopularniejszy stream: \n{0} - {1} ({2})", r["name"].ToString(), r["status"].ToString(), response["viewers"];
            SetTextInLabel(text);
        }
    }
}
