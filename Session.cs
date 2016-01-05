using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festival_Support_GUI
{
    public class Session
    {
        public string User;
        public string Token;
        public string Topic;
        public string Message;
        public string Id;
        public string Culture;
        public bool Live = false;
        public bool Support = false;
        public long ChatId;
        public Telegram.Bot.Types.ReplyKeyboardMarkup Keyboard = null;

        public Session()
        {

        }

        public Session(string username, string token, string id, long chatid)
        {
            User = username;
            Token = token;
            Id = id;
            ChatId = chatid;
        }

        public void SetCulture(string culture)
        {
            Culture = culture;
        }

        public void SetTopic(string topic)
        {
            Topic = topic;
        }

        public void SetMessage(string message)
        {
            Message = message;
        }
    }
}
