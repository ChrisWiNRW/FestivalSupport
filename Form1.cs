using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using File = System.IO.File;
using System.Windows.Forms;
using System.Drawing;
using Newtonsoft.Json;

namespace Festival_Support_GUI
{
    public partial class Form1 : Form
    {
        delegate void AddItemCallback(ListViewItem item);
        delegate void ClearItemCallback();
        delegate void AddUserMessageCallback(string message, string color);

        public Form1()
        {
            InitializeComponent();
        }

        public static string settingsPath;
        public static FestivalSettings fs;
        public static string Token;
        public static string aId = "";
        public static bool keyInput = false;
        public static Api Bot;
        public static string Pattern = "\\/m ([0-9]+) ([\\s\\S]+)$";
        public static List<Session> sessions = new List<Session>();

        public static string RandomString(int length)
        {
            fs = JsonConvert.DeserializeObject<FestivalSettings>(System.IO.File.ReadAllText(settingsPath));
            string chars = fs.tokenChars;
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string ReplaceVars(string input, Session ses = null)
        {
            if (ses == null)
            {
                ses = new Session();
            }
            return input.Replace("$token", ses.Token).Replace("$user", ses.User).Replace("$date", DateTime.Now.ToShortDateString()).Replace("$time", DateTime.Now.ToShortTimeString().Replace("$nl", "\n"));
        }

        static void RunApi()
        {
            System.Threading.ThreadStart InputStart = new System.Threading.ThreadStart(ConsoleInput);
            System.Threading.Thread InputProvider = new System.Threading.Thread(InputStart);
            InputProvider.Start();
            Run().Wait();
        }

        static void ConsoleInput()
        {
            AsyncConsoleInput().Wait();
        }

        static async Task AsyncConsoleInput()
        {
            while (true)
            {
                string Input = Console.ReadLine();
                if (Input == "/live")
                {
                    Console.Write("Id: ");
                    string id = Console.ReadLine();
                    foreach (Session s in sessions)
                    {
                        if (s.Id == id)
                        {
                            //Typing...
                            if (fs.showTyping)
                            {
                                await Bot.SendChatAction(s.ChatId, ChatAction.Typing);
                                await Task.Delay(1000 * fs.typingDuration);
                            }

                            //Send message
                            s.Live = true;
                            Console.Write("Message: ");
                            var t = await Bot.SendTextMessage(s.ChatId, Console.ReadLine().Replace("\\n", "\n"), false, 0, s.Keyboard);
                        }
                    }
                }
                else if (Input == "/d")
                {
                    Console.Write("Id: ");
                    string id = Console.ReadLine();
                    foreach (Session s in sessions)
                    {
                        if (s.Id == id)
                        {
                            //Typing...
                            if (fs.showTyping)
                            {
                                await Bot.SendChatAction(s.ChatId, ChatAction.Typing);
                                await Task.Delay(1000 * fs.typingDuration);
                            }

                            ///start-keyboard
                            var to = new ReplyKeyboardMarkup();
                            to.Keyboard = new string[][] {
                                    new string[] { "/start" }
                            };
                            to.OneTimeKeyboard = true;
                            to.ResizeKeyboard = true;

                            //Send message
                            s.Live = false;
                            var t = await Bot.SendTextMessage(s.ChatId, "Die Verbindung zum Supporter wurde getrennt.\nDir steht nun wieder der Bot zur Verfügung.\n\nBenutze für eine erneute Anfrage: /start", false, 0, to);
                        }
                    }
                }
                else if (Input == "/m")
                {
                    Console.Write("Id: ");
                    string id = Console.ReadLine();
                    foreach (Session s in sessions)
                    {
                        if (s.Id == id)
                        {
                            //Typing...
                            if (fs.showTyping)
                            {
                                await Bot.SendChatAction(s.ChatId, ChatAction.Typing);
                                await Task.Delay(1000 * fs.typingDuration);
                            }

                            //Send message
                            Console.Write("Message: ");
                            var t = await Bot.SendTextMessage(s.ChatId, Console.ReadLine().Replace("\\n", "\n"), false, 0, s.Keyboard);
                        }
                    }
                }
                else if (Input == "/key")
                {
                    Console.Write("Id: ");
                    string id = Console.ReadLine();
                    foreach (Session s in sessions)
                    {
                        if (s.Id == id)
                        {
                            Console.Write("Lines: ");
                            int Lines = int.Parse(Console.ReadLine());
                            List<List<string>> toKeyboard = new List<List<string>>();
                            for (int i = 0; i < Lines; i++)
                            {
                                Console.Write("Line " + i.ToString() + ": ");
                                toKeyboard.Add(new List<string>());
                                toKeyboard[i].AddRange(Console.ReadLine().Split('|'));
                            }
                            List<string[]> kb = new List<string[]>();
                            foreach (List<string> line in toKeyboard)
                            {
                                kb.Add(line.ToArray());
                            }
                            s.Keyboard = new ReplyKeyboardMarkup();
                            s.Keyboard.Keyboard = kb.ToArray();
                            s.Keyboard.OneTimeKeyboard = true;
                            s.Keyboard.ResizeKeyboard = true;
                        }
                    }
                }
                else if (Input == "/dkey")
                {
                    Console.Write("Id: ");
                    string id = Console.ReadLine();
                    foreach (Session s in sessions)
                    {
                        if (s.Id == id)
                        {
                            s.Keyboard = null;
                        }
                    }
                }
            }
        }

        static async Task Run()
        {
            fs = JsonConvert.DeserializeObject<FestivalSettings>(System.IO.File.ReadAllText(settingsPath));
            Bot = new Api(fs.token);
            var me = await Bot.GetMe();
            Console.WriteLine("Connection to account \"{0}\" established.", me.Username);
            var offset = 0;

            while (true)
            {
                fs = JsonConvert.DeserializeObject<FestivalSettings>(System.IO.File.ReadAllText(settingsPath));
                var updates = await Bot.GetUpdates(offset);

                foreach (var update in updates)
                {
                    if (update.Message.Type == MessageType.TextMessage)
                    {
                        Session ps = new Session();
                        foreach (Session ses in sessions)
                        {
                            if (ses.User == update.Message.Chat.Username)
                            {
                                ps = ses;
                            }
                        }

                        if (ps.Live)
                        {
                            //Forward users message to console
                            var f = (Form1)Application.OpenForms[0];
                            f.AddUserMessage(update.Message.Chat.Username + ": " + update.Message.Text, "#1ED760");
                        }
                        else
                        {
                            if (update.Message.Text == "/start")
                            {
                                //Typing...
                                if (fs.showTyping)
                                {
                                    await Bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                                    await Task.Delay(1000 * fs.typingDuration);
                                }

                                //Generate token
                                Token = RandomString(fs.tokenLength);

                                //Generate and add session
                                Session s = new Session(update.Message.Chat.Username, Token, sessions.Count.ToString(), update.Message.Chat.Id);
                                sessions.Add(s);

                                //Send message
                                var t = await Bot.SendTextMessage(update.Message.Chat.Id, ReplaceVars(fs.startMessage, s));
                            }
                            else if (update.Message.Text == "/help")
                            {
                                //Typing...
                                if (fs.showTyping)
                                {
                                    await Bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                                    await Task.Delay(1000 * fs.typingDuration);
                                }

                                //Send message
                                var t = await Bot.SendTextMessage(update.Message.Chat.Id, ReplaceVars(fs.helpMessage));
                            }
                            else if (update.Message.Text == "/topic")
                            {
                                //Typing...
                                if (fs.showTyping)
                                {
                                    await Bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                                    await Task.Delay(1000 * fs.typingDuration);
                                }

                                //Token-keyboard
                                var to = new ReplyKeyboardMarkup();
                                to.Keyboard = new string[][] {
                                    new string[] { Token }
                                };
                                to.OneTimeKeyboard = true;
                                to.ResizeKeyboard = true;

                                //Send token-question
                                var tokenans = await Bot.SendTextMessage(update.Message.Chat.Id, "Gib bitte dein Anfrage-Token ein.", false, 0, to);

                                //Ignore latest
                                offset = update.Id + 1;

                                //Wait for answer
                                var tanswer = await Bot.GetUpdates(offset);
                                while (tanswer.Length < 1)
                                {
                                    tanswer = await Bot.GetUpdates(offset);
                                }

                                //Check for session
                                Session s = new Session();
                                foreach (Session ses in sessions)
                                {
                                    if (ses.Token == tanswer[0].Message.Text && ses.User == update.Message.Chat.Username)
                                    {
                                        s = ses;
                                    }
                                }

                                //Typing...
                                if (fs.showTyping)
                                {
                                    await Bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                                    await Task.Delay(1000 * fs.typingDuration);
                                }

                                //Topic-keyboard
                                //    var r = new ReplyKeyboardMarkup();
                                //    r.Keyboard = new string[][] {
                                //    new string[] { "SkrivaPrisma", "Polar" },
                                //    new string[] { "Floe", "LockWatch" },
                                //    new string[] { "Repo", "Website"}
                                //};
                                //    r.OneTimeKeyboard = true;
                                //    r.ResizeKeyboard = true;
                                var r = FestivalSettingsHelper.ParseKeyboard(fs.topicKeyboard);

                                //Send topic-question
                                var t = await Bot.SendTextMessage(update.Message.Chat.Id, "Bitte wähle ein Problemthema.", false, 0, r);

                                //Ignore latest
                                offset = update.Id + 2;

                                //Wait for answer
                                var answer = await Bot.GetUpdates(offset);
                                while (answer.Length < 1)
                                {
                                    answer = await Bot.GetUpdates(offset);
                                }

                                //Assign result to session
                                s.SetTopic(answer[0].Message.Text);

                                //Typing...
                                if (fs.showTyping)
                                {
                                    await Bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                                    await Task.Delay(1000 * fs.typingDuration);
                                }

                                //Echo
                                var t2 = await Bot.SendTextMessage(answer[0].Message.Chat.Id, String.Format("Du hast \"{0}\" als Thema gewählt.", answer[0].Message.Text));
                            }
                            else if (update.Message.Text == "/message")
                            {
                                //Typing...
                                if (fs.showTyping)
                                {
                                    await Bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                                    await Task.Delay(1000 * fs.typingDuration);
                                }

                                //Token-keyboard
                                var to = new ReplyKeyboardMarkup();
                                to.Keyboard = new string[][] {
                                new string[] { Token }
                            };
                                to.OneTimeKeyboard = true;
                                to.ResizeKeyboard = true;

                                //Send token-question
                                var tokenans = await Bot.SendTextMessage(update.Message.Chat.Id, "Gib bitte dein Anfrage-Token ein.", false, 0, to);

                                //Ignore latest
                                offset = update.Id + 1;

                                //Wait for answer
                                var tanswer = await Bot.GetUpdates(offset);
                                while (tanswer.Length < 1)
                                {
                                    tanswer = await Bot.GetUpdates(offset);
                                }

                                //Check for session
                                Session s = new Session();
                                foreach (Session ses in sessions)
                                {
                                    if (ses.Token == tanswer[0].Message.Text && ses.User == update.Message.Chat.Username)
                                    {
                                        s = ses;
                                    }
                                }

                                //Typing...
                                if (fs.showTyping)
                                {
                                    await Bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                                    await Task.Delay(1000 * fs.typingDuration);
                                }

                                //Send message-question
                                var t = await Bot.SendTextMessage(update.Message.Chat.Id, "Bitte gib deine Nachricht ein.");

                                //Ignore latest
                                offset = update.Id + 2;

                                //Wait for answer
                                var answer = await Bot.GetUpdates(offset);
                                while (answer.Length < 1)
                                {
                                    answer = await Bot.GetUpdates(offset);
                                }

                                //Assign result to session
                                s.SetMessage(answer[0].Message.Text);

                                //Typing...
                                if (fs.showTyping)
                                {
                                    await Bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                                    await Task.Delay(1000 * fs.typingDuration);
                                }

                                //Echo
                                var t2 = await Bot.SendTextMessage(answer[answer.Length - 1].Message.Chat.Id, String.Format("Du hast uns \"{0}\" geschrieben.", answer[answer.Length - 1].Message.Text));
                            }
                            else if (update.Message.Text == "/support")
                            {
                                //Typing...
                                if (fs.showTyping)
                                {
                                    await Bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                                    await Task.Delay(1000 * fs.typingDuration);
                                }

                                //Token-keyboard
                                var to = new ReplyKeyboardMarkup();
                                to.Keyboard = new string[][] {
                                new string[] { Token }
                            };
                                to.OneTimeKeyboard = true;
                                to.ResizeKeyboard = true;

                                //Send token-question
                                var tokenans = await Bot.SendTextMessage(update.Message.Chat.Id, "Gib bitte dein Anfrage-Token ein.", false, 0, to);

                                //Ignore latest
                                offset = update.Id + 1;

                                //Wait for answer
                                var tanswer = await Bot.GetUpdates(offset);
                                while (tanswer.Length < 1)
                                {
                                    tanswer = await Bot.GetUpdates(offset);
                                }

                                //Check for session
                                Session s = new Session();
                                foreach (Session ses in sessions)
                                {
                                    if (ses.Token == tanswer[0].Message.Text && ses.User == update.Message.Chat.Username)
                                    {
                                        s = ses;
                                    }
                                }

                                //Submit session
                                Console.WriteLine("====================");
                                Console.WriteLine("Id: " + s.Id);
                                Console.WriteLine("Date: " + update.Message.Date.ToShortDateString() + " Time: " + update.Message.Date.ToShortTimeString());
                                Console.WriteLine("Token: " + s.Token);
                                Console.WriteLine("User: " + s.User);
                                Console.WriteLine("Topic: " + s.Topic);
                                Console.WriteLine("Message: " + s.Message);
                                Console.WriteLine("====================");

                                //Set support
                                s.Support = true;

                                //Refresh ListView
                                var f = (Form1)Application.OpenForms[0];
                                System.Threading.ThreadStart startInfo = new System.Threading.ThreadStart(f.refreshSessions);
                                System.Threading.Thread refresh = new System.Threading.Thread(startInfo);
                                refresh.Start();

                                //Typing...
                                if (fs.showTyping)
                                {
                                    await Bot.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                                    await Task.Delay(1000 * fs.typingDuration);
                                }

                                //Send message
                                var t = await Bot.SendTextMessage(update.Message.Chat.Id, "Deine Support-Anfrage wurde uns übermittelt.\nBitte benutze für eine neue Anfrage zuerst den /start Befehl.");
                            }
                            else if (false)
                            {
                                var r = new ReplyKeyboardMarkup();
                                r.Keyboard = new string[][] {
                                new string[] { "Ja", "Nein" }
                            };
                                r.OneTimeKeyboard = true;
                                r.ResizeKeyboard = true;
                                var t = await Bot.SendTextMessage(update.Message.Chat.Id, "Hilfe erforderlich?", false, 0, r);
                            }
                        }
                    }

                    if (update.Message.Type == MessageType.PhotoMessage)
                    {
                        var file = await Bot.GetFile(update.Message.Photo.LastOrDefault()?.FileId);

                        Console.WriteLine("Received Photo: {0}", file.FilePath);

                        var filename = file.FileId + "." + file.FilePath.Split('.').Last();

                        using (var profileImageStream = File.Open(filename, FileMode.Create))
                        {
                            await file.FileStream.CopyToAsync(profileImageStream);
                        }
                    }

                    offset = update.Id + 1;
                }

                await Task.Delay(1000);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            System.Threading.ThreadStart InputStart = new System.Threading.ThreadStart(RunApi);
            System.Threading.Thread InputProvider = new System.Threading.Thread(InputStart);
            InputProvider.Start();
            MessageBox.Show("Der Supportservice wurde gestartet.", "Festival Support", MessageBoxButtons.OK, MessageBoxIcon.Information);
            button1.Enabled = false;
        }

        public void refreshSessions()
        {
            ClearItems();
            foreach (Session s in sessions)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = s.Id;
                lvi.SubItems.Add(s.Token);
                lvi.SubItems.Add(s.User);
                lvi.SubItems.Add(s.Topic);
                lvi.SubItems.Add(s.Message);
                lvi.SubItems.Add("");
                AddItem(lvi);
            }
        }
        private void AddItem(ListViewItem item)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.listView1.InvokeRequired)
            {
                AddItemCallback d = new AddItemCallback(AddItem);
                this.Invoke(d, new object[] { item });
            }
            else
            {
                this.listView1.Items.Add(item);
            }
        }

        private void ClearItems()
        {
            if (this.listView1.InvokeRequired)
            {
                ClearItemCallback c = new ClearItemCallback(ClearItems);
                this.Invoke(c, new object[] { });
            }
            else
            {
                this.listView1.Items.Clear();
            }
        }

        private void supportÜbernehmenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Session s in sessions)
            {
                if (s.Id == listView1.SelectedItems[0].Text)
                {
                    s.Live = true;
                    aId = s.Id;
                }
            }
        }

        private void AddUserMessage(string message, string color)
        {
            if (richTextBox1.InvokeRequired)
            {
                AddUserMessageCallback a = new AddUserMessageCallback(AddUserMessage);
                this.Invoke(a, new object[] { message, color });
            }
            else
            {
                richTextBox1.SelectionColor = ColorTranslator.FromHtml(color);
                richTextBox1.AppendText(message + "\n");
                richTextBox1.SelectionColor = Color.Black;
            }
        }

        private async Task SendMessage(long chatid, string message, ReplyKeyboardMarkup r = null)
        {
            if (fs.showTyping)
            {
                await Bot.SendChatAction(chatid, ChatAction.Typing);
                await Task.Delay(1000 * fs.typingDuration);
            }
            await Bot.SendTextMessage(chatid, message, false, 0, r);
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                foreach (Session s in sessions)
                {
                    if (s.Id == aId)
                    {
                        if (!keyInput)
                        {
                            AddUserMessage("Festival Support: " + textBox1.Text.Replace("\\n", "\n"), "#FC3539");
                            SendMessage(s.ChatId, textBox1.Text, s.Keyboard).Wait(1000);
                            textBox1.Clear();
                        }
                        else
                        {
                            s.Keyboard = new ReplyKeyboardMarkup();
                            string tKeyboardCode = textBox1.Text;
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
                            s.Keyboard.Keyboard = kb.ToArray();
                            s.Keyboard.OneTimeKeyboard = true;
                            s.Keyboard.ResizeKeyboard = true;
                            keyInput = false;
                            textBox1.Clear();
                        }
                    }
                }
            }
        }

        private void supportBeendenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Session s in sessions)
            {
                if (s.Id == listView1.SelectedItems[0].Text)
                {
                    s.Live = false;
                    richTextBox1.Clear();
                    var startkey = new ReplyKeyboardMarkup();
                    startkey.Keyboard = new string[][] {
                                    new string[] { "/start" }
                            };
                    startkey.OneTimeKeyboard = true;
                    startkey.ResizeKeyboard = true;
                    SendMessage(s.ChatId, "Die Verbindung zum Supporter wurde getrennt.\nDir steht nun wieder der Bot zur Verfügung.\n\nBenutze für eine erneute Anfrage: /start", startkey).Wait(1000);
                    aId = "";
                }
            }
        }

        private void tastaturZurücksetzenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Session s in sessions)
            {
                if (s.Id == aId)
                {
                    s.Keyboard = null;
                }
            }
        }

        private void tastaturBearbeitenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddUserMessage("Gib nun die neue Tastatur ein.", "#2D46B9");
            keyInput = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                foreach (Session s in sessions)
                {
                    if (s.Id == listView1.SelectedItems[0].Text)
                    {
                        s.Live = true;
                        aId = s.Id;
                        AddUserMessage("Sie haben den Support übernommen.\n", "#2D46B9");
                        if (fs.showSupportMessage)
                        {
                            SendMessage(s.ChatId, ReplaceVars(fs.supportMessage, s)).Wait(1000);
                        }
                        textBox1.Enabled = true;
                        button2.Enabled = false;
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                foreach (Session s in sessions)
                {
                    if (s.Id == listView1.SelectedItems[0].Text)
                    {
                        s.Live = false;
                        richTextBox1.Clear();
                        var startkey = new ReplyKeyboardMarkup();
                        startkey.Keyboard = new string[][] {
                                    new string[] { "/start" }
                            };
                        startkey.OneTimeKeyboard = true;
                        startkey.ResizeKeyboard = true;
                        if (fs.showSupportEndMessage)
                        {
                            SendMessage(s.ChatId, ReplaceVars(fs.supportEndMessage, s), startkey).Wait(1000);
                        }
                        aId = "";
                        textBox1.Enabled = false;
                        button2.Enabled = true;
                    }
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            foreach (Session s in sessions)
            {
                if (s.Id == aId)
                {
                    KeyboardInput ki = new KeyboardInput();
                    if (ki.ShowDialog() == DialogResult.OK)
                    {
                        s.Keyboard = new ReplyKeyboardMarkup();
                        string tKeyboardCode = ki.Keyboard;
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
                        s.Keyboard.Keyboard = kb.ToArray();
                        s.Keyboard.OneTimeKeyboard = true;
                        s.Keyboard.ResizeKeyboard = true;
                        MessageBox.Show("Tastatur geändert.", "Festival Support", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            foreach (Session s in sessions)
            {
                if (s.Id == aId)
                {
                    s.Keyboard = null;
                    MessageBox.Show("Tastatur zurückgesetzt.", "Festival Support", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            SettingsForm sf = new SettingsForm();
            if (!string.IsNullOrEmpty(fs.token))
            {
                sf.TokenChangeAllowed = button1.Enabled;
            }
            sf.ShowDialog();
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                if (aId == "")
                {
                    button2.Enabled = true;
                }
                button3.Enabled = true;
                button4.Enabled = true;
                button5.Enabled = true;
                button6.Enabled = true;
            }
            else
            {
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;
                button6.Enabled = false;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Einstellungen (*.json)|*.json";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                settingsPath = ofd.FileName;
                fs = JsonConvert.DeserializeObject<FestivalSettings>(System.IO.File.ReadAllText(settingsPath));
                if (string.IsNullOrEmpty(fs.token))
                {
                    button1.Enabled = false;
                }
                this.BringToFront();
            }
            else
            {
                if (MessageBox.Show("Wollen sie eine leere Datei generieren und laden?", "Festival Support", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Filter = "Default-Einstellungen (*.json)|*.json";
                    sfd.FileName = "default";
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        fs = new FestivalSettings();
                        System.IO.File.WriteAllText(sfd.FileName, JsonConvert.SerializeObject(fs));
                        button1.Enabled = false;
                    }
                    else
                    {
                        Application.Exit();
                    }
                }
                else
                {
                    Application.Exit();
                }
            }
        }
    }
}
