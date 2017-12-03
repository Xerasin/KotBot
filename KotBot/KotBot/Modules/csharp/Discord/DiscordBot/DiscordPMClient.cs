using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DWebSocket = Discord.WebSocket;
using KotBot;
using KotBot.BotManager;
using Discord;

namespace KotBot.DiscordBot
{
    [Serializable]
    public class DiscordPMClient : Client
    {
        public DWebSocket.SocketUser me;
        public DWebSocket.SocketUser other = null;
        public DWebSocket.DiscordSocketClient client;

        public DiscordPMClient(DWebSocket.SocketUser user, DWebSocket.SocketUser other, DWebSocket.DiscordSocketClient client)
        {
            this.me = user;
            this.other = other;
            this.client = client;
        }

        public override object Message(string message)
        {
            if (this.client.LoginState != LoginState.LoggedIn) return null;
            if (other == null) return null;
            DiscordUser user = new DiscordUser(other, client);
            return user.Message(message);
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
            return "|Discord|PM|" + other.Username + "#" + other.Discriminator;
        }
        public override string ProcessMessage(string message)
        {
            message = message.Replace("{aname}", string.Format("<@{0}>", this.other.Id));
            message = message.Replace("{name}", this.other.Username);
            return message;
        }
        public override string GetIP()
        {
            return "";
        }
        public override User FindUserByName(string name)
        {
            DiscordUser user = new DiscordUser(other, client);
            string str = user.GetUserID();
            return user;
        }
    }
}
