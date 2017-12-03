using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotBot.IRC
{
    public class IRCUser : User
    {
        public IrcDotNet.IrcLocalUser localuser;
        public IrcDotNet.IrcUser user;
        public IRCUser(IrcDotNet.IrcLocalUser localuser, IrcDotNet.IrcUser user)
        {
            this.user = user;
            this.localuser = localuser;
        }

        public override string GetName()
        {
            return this.user.NickName;
        }

        public override object Message(string message)
        {
            message = message.Replace("\r\n", "");
            localuser.SendMessage(user, message);
            ModuleCommunications.MessageSent(new MessageArgs()
            {
                message = new Message(new IRCPMClient(localuser, user), new IRC.IRCUser(localuser, localuser), message)
            });
            return null;
        }
        public override string GetUserID()
        {
            return localuser.Client.ServerName + "|IRC|" + user.NickName;
        }
    }
}
