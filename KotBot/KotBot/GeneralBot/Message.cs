using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotBot.BotManager
{
    [Serializable]
    public class Message
    {
        Client client;
        User user;
        string message;
        public Message(Client client, User user, string Message)
        {
            this.client = client;
            this.user = user;
            this.message = Message;
        }
        
        public virtual object Reply(string message)
        {
            return this.client.Message(message);
        }
        public virtual Client GetClient()                                                                                       
        {
            return this.client;
        }
        public virtual string GetMessage()
        {
            return this.message;
        }
        public virtual User GetSender()
        {
            return this.user;
        }
    }
}
