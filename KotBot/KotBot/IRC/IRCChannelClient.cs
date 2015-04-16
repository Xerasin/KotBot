using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotBot.IRC
{
    public class IRCChannelClient : Client
    {
        private IrcDotNet.IrcLocalUser client;
        private IrcDotNet.IrcChannel channel;
        public IRCChannelClient(IrcDotNet.IrcLocalUser client, IrcDotNet.IrcChannel channel)
        {
            this.client = client;
            this.channel = channel;
        }

        public IrcDotNet.IrcChannel GetChannel()
        {
            return channel;
        }

        public override void Message(string message)
        {
            message = message.Replace("\r\n", "");
            if (message.Length > 500)
            {
                message = message.Substring(0, 500);
            }
            client.SendMessage(channel, message);
            Scripting.LuaHook.Call("MessageSent", new Message(this, new IRC.IRCUser(client, client), message));
        }
        public override string GetLocationString()
        {
            return client.Client.ServerName + "|" + channel.Name;
        }
        public void JoinChannel(string channel)
        {
            client.Client.Channels.Join(channel);
        }
        public void LeaveChannel()
        {
            channel.Leave("I was told too... *cries* :(");
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
