using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotBot.IRC
{
    public class IRCClient : Client
    {
        private IrcDotNet.IrcLocalUser client;
        private IrcDotNet.IrcChannel channel;
        public IRCClient(IrcDotNet.IrcLocalUser client, IrcDotNet.IrcChannel channel)
        {
            this.client = client;
            this.channel = channel;
        }
        public override void Message(string message)
        {
            if (message.Length > 500)
            {
                message = message.Substring(0, 500);
            }
            client.SendMessage(channel, message);
        }
        public override string GetName()
        {
            return client.NickName;
        }
        public override User FindUserByName(string name)
        {
            foreach(IrcDotNet.IrcChannelUser user in channel.Users)
            {
                if(user.User.NickName.ToLower().Contains(name.ToLower()))
                {
                    return new IRCUser(client, user.User);
                }
            }
            return null;
        }
    }
}
