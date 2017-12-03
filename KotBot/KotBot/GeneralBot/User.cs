using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotBot.BotManager
{
    [Serializable]
    public class User
    {
        public virtual object Message(string message)
        {
            return null;
        }
        public virtual string ProcessMessage(string message)
        {
            return message;
        }
        public virtual string GetName()
        {
            return "";
        }
        public virtual string GetUserID()
        {
            return "";
        }
    }
}
