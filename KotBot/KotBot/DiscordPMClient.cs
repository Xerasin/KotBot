using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
namespace KotBot.DiscordBot
{
    public class DiscordPMClient : Client
    {
        public Discord.User me;
        public Discord.User other;
        public DiscordPMClient(Discord.User user, Discord.User client)
        {
            this.me = user;
            this.other = client;
        }

        public async override void Message(string message)
        {
            message = this.ProcessMessage(message);
            Discord.Channel channel = await DiscordManager.client.CreatePrivateChannel(other.Id);
            await channel.SendMessage(message);
            Scripting.LuaHook.Call("MessageSent", new Message(this, new DiscordUser(me), message));
        }
        public override string GetName()
        {
            return "";
        }
        public override string GetDomain()
        {
            return "Discord";
        }
        public override string GetLocationString()
        {
            return "|Discord|PM|" + other.Name + "#" + other.Discriminator;
        }
        public override string ProcessMessage(string message)
        {
            message = message.Replace("{aname}", string.Format("<@{0}>", this.other.Id));
            message = message.Replace("{name}", this.other.Name);
            return message;
        }
        public override string GetIP()
        {
            return "";
        }
        public override User FindUserByName(string name)
        {
            DiscordUser user = new DiscordUser(other);
            string str = user.GetUserID();
            return user;
        }
    }
}
