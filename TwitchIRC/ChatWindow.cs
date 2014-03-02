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
            config = ConfigBase.ReadConfig(@"config.xml", new IRCConfig()) as IRCConfig;
            bot = new IRCBot(config);
            api = new TwitchAPI(config, this);
            InitializeComponent();
            //this.pictureBox1.Draggable(true);
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
            try
            {
                this.InvokeIfRequired(text => { TextBox.Text = text; }, "Done");
            }
            catch (TargetInvocationException) { Console.WriteLine("Złapałem ten głupi wyjątek"); }
            this.InvokeIfRequired(img =>
            {
                this.PictureBox.Image = img;
            }, Image.FromFile(@"images.jpg"));
            System.Windows.Forms.MessageBox.Show(string.Format("Czas wykonania operacji = {0}ms", watch.ElapsedMilliseconds));


        }

    }
}
