using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordSharp;
using DiscordSharp.Objects;
namespace KotBot.Discord
{
    public class DiscordUser : User
    {
        public DiscordMember member;
        public DiscordUser(DiscordMember user)
        {
            this.member = user;
        }

        public DiscordMember GetMember()
        {
            return member;
        }
        public override string GetName()
        {
            return member.Username;
        }

        public override void Message(string message)
        {
            DiscordManager.client.SendMessageToUser(message, this.member);
        }
        public override string GetUserID()
        {
            return "|Discord|" + member.Username + "#" + member.Discriminator;
        }
    }
}
