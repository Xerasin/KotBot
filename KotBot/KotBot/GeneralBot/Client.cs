using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotBot.BotManager
{
    [Serializable]
    public class Client
    {
        public virtual object Message(string message)
        {
            return null;
        }
        public virtual string ProcessMessage(string message)
        {
            return message;
        }
        public virtual string GetIP()
        {
            return "";
        }
        public virtual string GetName()
        {
            return "";
        }
        public virtual string GetDomain()
        {
            return "";
        }
        public virtual User FindUserByName(string name)
        {
            return null;
        }
        public virtual string GetLocationString()
        {
            return "";
        }
    }
}
