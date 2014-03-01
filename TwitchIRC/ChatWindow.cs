using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TwitchIRC
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ChatWindow : Form
    {
        IRCBot bot;
        TwitchAPI api;
        Dictionary<string, Action<string, string>> gFunc = new Dictionary<string, Action<string, string>>();
        /// <summary>
        /// 
        /// </summary>
        public RichTextBox textBox1 = new RichTextBox();
        IRCConfig config = null;
        /// <summary>
        /// 
        /// </summary>
        public ChatWindow()
        {
            gFunc.Add("add", AddTextToTextBox);
            config = ConfigBase.ReadConfig(@"config.xml", new IRCConfig()) as IRCConfig;
            bot = new IRCBot(config, gFunc);
            api = new TwitchAPI(config);
            InitializeComponent();

            bot.Connect();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="box"></param>
        /// <param name="text"></param>
        /// <param name="color"></param>
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

        private void SetInLabel(string text) {
            textBox2.Text = text;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("user32.dll")]
        static extern bool HideCaret(IntPtr h);

        private async void button1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            await api.GetAndSaveEmotes();
            watch.Stop();
            System.Windows.Forms.MessageBox.Show(string.Format("Czas wykonania operacji = {0}ms", watch.ElapsedMilliseconds));

            try
            {
                
                //typeof(ChatWindow).GetMethod("SetInLabel").Invoke(this, new [] {"Done"});
                SetTextInLabel("Done");
            }
            catch (TargetInvocationException) { Console.WriteLine("Złapałem ten głupi wyjątek"); }
        }
    }
}
