using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotBot.IRC
{
    public class IRCPMClient : Client
    {
        private IrcDotNet.IrcLocalUser client;
        private IrcDotNet.IrcUser user;
        public IRCPMClient(IrcDotNet.IrcLocalUser client, IrcDotNet.IrcUser user)
        {
            this.client = client;
            this.user = user;
        }
        public override void Message(string message)
        {
            if (message.Length > 500)
            {
                message = message.Substring(0, 500);
            }
            client.SendMessage(user, message);
        }
        public override string GetName()
        {
            return client.NickName;
        }
        public override User FindUserByName(string name)
        {
            foreach (IrcDotNet.IrcChannelUser user in client.GetChannelUsers())
            {
                if (user.User.NickName.ToLower().Contains(name.ToLower()))
                {
                    return new IRCUser(client, user.User);
                }
            }   
            return null;
        }
    }
}
