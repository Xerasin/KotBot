using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordSharp;
using DiscordSharp.Objects;
namespace KotBot.Discord
{
    public class DiscordChatClient : Client
    {
        public DiscordChannel channel;
        public DiscordMember member;
        public DiscordChatClient(DiscordMember user, DiscordChannel client)
        {
            this.channel = client;
            this.member = user;
        }
        public override void Message(string message)
        {
            DiscordManager.client.SendMessageToChannel(message, channel);
            Scripting.LuaHook.Call("MessageSent", new Message(this, new DiscordUser(member), message));
        }
        public override string GetName()
        {
            return channel.Parent.Name + "|" + channel.Name;
        }
        public override string GetLocationString()
        {
            return "|Discord|" + channel.Parent.Name + "|" + channel.Name;
        }
        public override string GetIP()
        {
            return "";
        }
        public override User FindUserByName(string name)
        {
            if (name == null)
                return null;
            foreach (KeyValuePair<string, DiscordMember> user in channel.Parent.Members)
            {
                if (user.Value.Username.ToLower().Contains(name.ToLower()))
                {
                    return new DiscordUser(user.Value);
                }
            }
            return null;
        }
    }
}
