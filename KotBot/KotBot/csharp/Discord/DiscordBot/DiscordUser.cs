using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DWebSocket = Discord.WebSocket;
using KotBot;
using KotBot.BotManager;
using KotBot.Modules;
using Discord;
namespace KotBot.DiscordBot
{
    [Serializable]
    public class DiscordUser : User
    {
        public DWebSocket.SocketUser member;
        public DWebSocket.DiscordSocketClient client;
        public DiscordUser(DWebSocket.SocketGuildUser user, DWebSocket.DiscordSocketClient client)
        {
            this.member = user;
            this.client = client;
        }

        public DiscordUser(DWebSocket.SocketUser user, DWebSocket.DiscordSocketClient client)
        {
            this.member = user;
            this.client = client;
        }
        public DWebSocket.SocketUser GetMember()
        {
            return member;
        }
        public override string GetName()
        {
            if (typeof(DWebSocket.SocketGuildUser) == this.member.GetType())
            {
                if(((DWebSocket.SocketGuildUser)this.member).Nickname != null)
                {
                    return ((DWebSocket.SocketGuildUser)this.member).Nickname;
                }
            }
            return member.Username;
        }
        public string GetID()
        {
            return member.Id.ToString();
        }
        public override string ProcessMessage(string message)
        {
            message = message.Replace("{name}", this.GetName());
            message = message.Replace("{aname}", this.member.Mention);
            return message;
        }
        public object Message(string message, Embed embed)
        {
            message = this.ProcessMessage(message);
            Task<Discord.IDMChannel> tChannel = member.GetOrCreateDMChannelAsync();
            tChannel.Wait();
            var channel = (DWebSocket.SocketDMChannel)client.GetChannel(tChannel.Result.Id);
            Task<Discord.Rest.RestUserMessage> dMessage = channel.SendMessageAsync(message, false, embed);
            dMessage.Wait();
            ModuleCommunications.OnMessageSent("Discord", new Message(new DiscordPMClient(client.CurrentUser, this.member, client), new DiscordUser(client.GetUser(client.CurrentUser.Id), client), message));
            return dMessage.Result;
        }

        public override object Message(string message)
        {
            return Message(message, null);
        }
        public override string GetUserID()
        {
            return "|Discord|" + member.Id;
        }
    }
}
