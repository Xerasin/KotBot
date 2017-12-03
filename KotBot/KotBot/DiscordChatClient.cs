using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
namespace KotBot.DiscordBot
{
    public class DiscordChatClient : Client
    {
        public Discord.Channel channel;
        public Discord.User member;
        public DiscordChatClient(Discord.User user, Discord.Channel client)
        {
            this.channel = client;
            this.member = user;
        }
        public override void Message(string message)
        {
            message = this.ProcessMessage(message);
            channel.SendMessage(message);
            Scripting.LuaHook.Call("MessageSent", new Message(this, new DiscordUser(channel.GetUser(DiscordManager.client.CurrentUser.Id)), message));
        }
        public override string GetName()
        {
            return channel.Server.Id + "|" + channel.Id;
        }
        public override string GetLocationString()
        {
            return "|Discord|" + channel.Server.Name + "|" + channel.Name;
        }
        public override string GetIP()
        {
            return "";
        }
        public override string GetDomain()
        {
            return "Discord";
        }
        public DiscordClient GetClient()
        {
            return DiscordManager.client;
        }
        public Discord.Channel GetChannel()
        {
            return this.channel;
        }
        public Discord.Channel FindChannelByName(string name)
        {
            if (name == null)
                return null;
            foreach (Discord.Channel user in channel.Server.TextChannels)
            {
                if (user.Name.ToLower().Contains(name.ToLower()))
                {
                    return user;
                }
            }
            return null;
        }
        public Discord.Channel FindVoiceChannelByName(string name)
        {
            if (name == null)
                return null;
            foreach (Discord.Channel user in channel.Server.VoiceChannels)
            {
                if (user.Name.ToLower().Contains(name.ToLower()))
                {
                    return user;
                }
            }
            return null;
        }
        public override string ProcessMessage(string message)
        {
            message = message.Replace("{aname}", string.Format("<@{0}>", this.member.Id));
            message = message.Replace("{name}", this.member.Name);
            return message;
        }
        public override User FindUserByName(string name)
        {
            if (name == null)
                return null;
            foreach (Discord.User user in channel.Users)
            {
                if (user.Name.ToLower().Contains(name.ToLower()))
                {
                    return new DiscordUser(user);
                }
            }
            return null;
        }
    }
}
