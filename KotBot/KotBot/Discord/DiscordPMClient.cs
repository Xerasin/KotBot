using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordSharp;
using DiscordSharp.Objects;
namespace KotBot.Discord
{
    public class DiscordPMClient : Client
    {
        public DiscordMember me;
        public DiscordMember other;
        public DiscordPMClient(DiscordMember user, DiscordMember client)
        {
            this.me = user;
            this.other = client;
        }
        public override void Message(string message)
        {
            DiscordManager.client.SendMessageToUser(message, other);
            Scripting.LuaHook.Call("MessageSent", new Message(this, new DiscordUser(me), message));
        }
        public override string GetName()
        {
            return "";
        }
        public override string GetLocationString()
        {
            return "|Discord|PM|" + other.Username + "#" + other.Discriminator;
        }
        public override string GetIP()
        {
            return "";
        }
        public override User FindUserByName(string name)
        {
            return new DiscordUser(other);
        }
    }
}
