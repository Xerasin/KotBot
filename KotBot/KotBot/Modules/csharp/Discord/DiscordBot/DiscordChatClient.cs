using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using DWebSocket = Discord.WebSocket;
using KotBot;
using KotBot.Modules;
using KotBot.BotManager;
namespace KotBot.DiscordBot
{
    [Serializable]
    public class DiscordChatClient : Client
    {
        public DWebSocket.SocketTextChannel channel;
        public DWebSocket.SocketGuildUser member;
        public DWebSocket.DiscordSocketClient client;
        public DiscordChatClient(DWebSocket.DiscordSocketClient client, DWebSocket.SocketGuildUser user, DWebSocket.SocketTextChannel channel)
        {
            this.channel = channel;
            this.member = user;
            this.client = client;
        }
        public override object Message(string message)
        {
            message = this.ProcessMessage(message);
            Task<Discord.Rest.RestUserMessage> dMessage = channel.SendMessageAsync(message);
            ModuleCommunications.OnMessageSent(new Message(this, new DiscordUser(client.CurrentUser, client), message));
            dMessage.Wait();
            return dMessage.Result;
        }
        public override string GetName()
        {
            return channel.Guild.Name + "|" + channel.Id;
        }
        public override string GetLocationString()
        {
            try
            {
                return "|Discord|" + channel.Guild.Name + "|" + channel.Name;
            }
            catch (Exception) { return ""; }
        }
        public override string GetIP()
        {
            return "";
        }
        public override string GetDomain()
        {
            return "Discord";
        }
        public DWebSocket.DiscordSocketClient GetClient()
        {
            return client;
        }
        public DWebSocket.SocketTextChannel GetChannel()
        {
            return this.channel;
        }
        public DWebSocket.SocketTextChannel FindChannelByName(string name)
        {
            if (name == null)
                return null;
            foreach (DWebSocket.SocketTextChannel user in channel.Guild.TextChannels)
            {
                if (user.Name.ToLower().Contains(name.ToLower()))
                {
                    return user;
                }
            }
            return null;
        }
        public DWebSocket.SocketVoiceChannel FindVoiceChannelByName(string name)
        {
            if (name == null)
                return null;
            foreach (DWebSocket.SocketVoiceChannel user in channel.Guild.VoiceChannels)
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
            message = message.Replace("{name}", this.member.Nickname);
            return message;
        }
        public override User FindUserByName(string name)
        {
            if (name == null)
                return null;
            foreach (DWebSocket.SocketGuildUser user in channel.Users)
            {
                if (user.Nickname.ToLower().Contains(name.ToLower()))
                {
                    return new DiscordUser(user, client);
                }
            }
            return null;
        }
    }
}
