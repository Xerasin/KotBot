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
            client.SendMessage(channel, message);
        }
    }
}
