using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TwitchIRC
{
    public partial class ChatWindow : Form
    {
        IRCBot bot;
        TwitchAPI api;
        Dictionary<string, Action<string, string>> gFunc = new Dictionary<string, Action<string, string>>();
        public RichTextBox textBox1 = new RichTextBox();
        IRCConfig config = null;
        public ChatWindow()
        {
            gFunc.Add("add", AddTextToTextBox);
            config = ConfigBase.ReadConfig(@"config.xml", new IRCConfig()) as IRCConfig;
            bot = new IRCBot(config, gFunc);
            api = new TwitchAPI(config);
            InitializeComponent();

            bot.Connect();
        }

        public void AppendWithColor(RichTextBox box, string text, Color color)
        {
            if (box.Lines.Length > 100)
            {
                Console.WriteLine("Ok, dotarłem do 100 i mam więcej");
                box.Select(0, box.GetFirstCharIndexFromLine(1)); // select the first line
                box.SelectedText = "";
                Console.WriteLine("teraz ilość linii to: " + box.Lines.Length);
            }
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
                textBox2.Text = text;
            }
        }

        delegate void ImageInPB(Image img);

        private void SetImageInPB(Image img)
        {
            if (this.textBox1.InvokeRequired)
            {
                try
                {
                    ImageInPB d = new ImageInPB(SetImageInPB);
                    this.Invoke(d, new object[] { img });
                }
                catch (ObjectDisposedException) { }

            }
            else
            {
                pictureBox1.Image = img;
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
            //await api.GetEmotesAsync(config.Data["channel"].Remove(0, 1)).ContinueWith(t =>
            //{
            //    foreach (var item in t.Result["emoticons"])
            //    {
            //        new System.Threading.Thread(() =>
            //        {
            //            string name = item["url"].ToString().Substring(item["url"].ToString().LastIndexOf("/"));
            //            api.GetImageFromUrlAsync(item["url"].ToString(),false).ContinueWith((r=>{
            //                    api.SaveImage(r.Result, name);
            //                }));

            //        }).Start();
            //    }

            //});
            api.GetAndSaveEmotes();
        }
    }
}
