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

        public override void Message(string message)
        {
            localuser.SendMessage(user, message);
        }
    }
}
