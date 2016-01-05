using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace Festival_Support_GUI
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        public bool TokenChangeAllowed
        {
            get
            {
                return textBox1.Enabled;
            }
            set
            {
                textBox1.Enabled = value;
            }
        }

        private void textBox4_Click(object sender, EventArgs e)
        {
            KeyboardInput ki = new KeyboardInput();
            ki.Keyboard = textBox4.Text;
            if (ki.ShowDialog() == DialogResult.OK)
            {
                textBox4.Text = ki.Keyboard;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            AboutForm af = new AboutForm();
            af.ShowDialog();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            groupBox2.Enabled = checkBox3.Checked;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox5.Enabled = checkBox1.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            textBox6.Enabled = checkBox2.Checked;
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            RefreshSettings();
        }

        public void RefreshSettings()
        {
            textBox1.Text = Form1.fs.token;
            checkBox3.Checked = Form1.fs.showTyping;
            numericUpDown1.Value = Form1.fs.typingDuration;
            numericUpDown2.Value = Form1.fs.tokenLength;
            textBox8.Text = Form1.fs.tokenChars;
            textBox2.Text = Form1.fs.startMessage;
            textBox3.Text = Form1.fs.helpMessage;
            checkBox1.Checked = Form1.fs.showSupportMessage;
            textBox5.Enabled = Form1.fs.showSupportMessage;
            textBox5.Text = Form1.fs.supportMessage;
            textBox4.Text = Form1.fs.topicKeyboard;
            checkBox2.Checked = Form1.fs.showSupportEndMessage;
            textBox6.Text = Form1.fs.supportEndMessage;
            textBox6.Enabled = Form1.fs.showSupportEndMessage;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FestivalSettings fs = new FestivalSettings();
            fs.token = textBox1.Text;
            fs.showTyping = checkBox3.Checked;
            fs.typingDuration = (int)numericUpDown1.Value;
            fs.tokenLength = (int)numericUpDown2.Value;
            fs.tokenChars = textBox8.Text;
            fs.startMessage = textBox2.Text;
            fs.helpMessage = textBox3.Text;
            fs.showSupportMessage = checkBox1.Checked;
            fs.supportMessage = textBox5.Text;
            fs.topicKeyboard = textBox4.Text;
            fs.showSupportEndMessage = checkBox2.Checked;
            fs.supportEndMessage = textBox6.Text;
            System.IO.File.WriteAllText(Form1.settingsPath, JsonConvert.SerializeObject(fs));
            Form1.fs = JsonConvert.DeserializeObject<FestivalSettings>(System.IO.File.ReadAllText(Form1.settingsPath));
            RefreshSettings();
        }
    }

    public static class FestivalSettingsHelper
    {
        public static ReplyKeyboardMarkup ParseKeyboard(string Code)
        {
            ReplyKeyboardMarkup r = new ReplyKeyboardMarkup();
            string tKeyboardCode = Code;
            List<List<string>> toKeyboard = new List<List<string>>();
            int Lines = tKeyboardCode.Split('§').Length;
            for (int i = 0; i < Lines; i++)
            {
                toKeyboard.Add(new List<string>());
                toKeyboard[i].AddRange(tKeyboardCode.Split('§')[i].Split('|'));
            }
            List<string[]> kb = new List<string[]>();
            foreach (List<string> line in toKeyboard)
            {
                kb.Add(line.ToArray());
            }
            r.Keyboard = kb.ToArray();
            r.OneTimeKeyboard = true;
            r.ResizeKeyboard = true;
            return r;
        }
    }

    public class FestivalSettings
    {
        public string token;
        public bool showTyping;
        public int typingDuration = 1;
        public int tokenLength = 4;
        public string tokenChars;
        public string startMessage;
        public string helpMessage;
        public bool showSupportMessage;
        public string supportMessage;
        public string topicKeyboard;
        public bool showSupportEndMessage;
        public string supportEndMessage;
    }
}
