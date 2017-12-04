using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using KotBot;
using KotBot.BotManager;
using DWebSocket = Discord.WebSocket;
using KotBot.Modules;
using KotBot.Scripting;

namespace KotBot.DiscordBot
{
    [Serializable]
    public class DiscordMessageArgs : EventArgs
    {
        public DWebSocket.SocketMessage Message { get; set; }
        public DWebSocket.DiscordSocketClient Client { get; set; }
        public DWebSocket.SocketGuildUser User { get; set; }
        public DWebSocket.SocketTextChannel Channel { get; set; }
        public Message GenericMessage {get; set; }
    }
    [Serializable]
    public delegate bool DiscordMessageEvent(DiscordMessageArgs args);

    [Serializable]
    public static class DiscordManager
    {
        public static event DiscordMessageEvent DiscordMessage;
        public static DWebSocket.DiscordSocketClient client;
        public async static Task<bool> Start(string token)
        {   
            try
            {
                if (client != null)
                {
                    client.Dispose();
                }

                client = new DWebSocket.DiscordSocketClient();

                client.MessageReceived += Client_MessageReceived;
                await client.LoginAsync(Discord.TokenType.Bot, token);
                client.Connected += Client_Connected;
                await client.StartAsync();
                return true;
            }
            catch (System.Exception discordFailed)
            {
                client = null;
                return false;
            }
        }

        public async static void Close()
        {
            if(client != null)
            {
                await client.LogoutAsync();
            }
        }

        private static Task Client_Connected()
        {
            
            Log.Print($"Discord Connected, User: {client.CurrentUser.Username}");
            MiscDiscord.Init();
            return null;
        }

        public static DWebSocket.SocketGuildUser GetSelf(DWebSocket.SocketGuild server)
        {
            return server.GetUser(client.CurrentUser.Id);
        }

        private static Task Client_MessageReceived(DWebSocket.SocketMessage arg)
        {
            if (arg.Author.Id == client.CurrentUser.Id)
            {
                return null;
            }
            if(typeof(DWebSocket.SocketDMChannel) == arg.Channel.GetType())
            {

                Message chatMessage2 = new Message(new DiscordPMClient(arg.Author, ((DWebSocket.SocketDMChannel)arg.Channel).Recipient, client), new DiscordUser(arg.Author, client), arg.Content);
                ModuleCommunications.OnMessageReceived("Discord", chatMessage2);
                return null;
            }
            if (typeof(DWebSocket.SocketTextChannel) == arg.Channel.GetType())
            {
                DWebSocket.SocketTextChannel channel = (DWebSocket.SocketTextChannel)arg.Channel;
                Message chatMessage = new Message(new DiscordChatClient(client, channel.GetUser(arg.Author.Id), channel), new DiscordUser(channel.GetUser(arg.Author.Id), client), arg.Content);
                if (ModuleCommunications.OnShouldProcessMessage("Discord", chatMessage))
                {
                    ModuleCommunications.OnMessageReceived("Discord", chatMessage);

                    if (DiscordMessage != null)
                    {
                        DiscordMessage(new DiscordMessageArgs()
                        {
                            Message = arg,
                            Channel = channel,
                            Client = client,
                            User = channel.GetUser(arg.Author.Id),
                            GenericMessage = chatMessage
                        });
                    }
                }  
            }
            return null;
        }
        
        [RegisterLuaFunction("discord.SetAvatar", "", "")]
        public static void SetAvatar(string url)
        {
            System.Net.WebClient wc = new WebClient();
            wc.Proxy = null;
            File.Delete("temp.png");
            byte[] file = wc.DownloadData(url);
            using (BinaryWriter writer = new BinaryWriter(File.OpenWrite("temp.png")))
            {
                writer.Write(file);
                writer.Flush();
                writer.Close();
            }
            System.Action<Discord.SelfUserProperties> newUserProperties = outputAction => { outputAction.Avatar = new Discord.Image(File.Open("temp.png", FileMode.OpenOrCreate)); };
            client.CurrentUser.ModifyAsync(newUserProperties);
        }

        /*public static NLua.LuaTable GetMessageHistory(DWebSocket.SocketTextChannel channel, DWebSocket.SocketUser author, double count)
        {
            NLua.LuaTable table = MainLua.GetNewTable();
            int i = 1;
            foreach (DWebSocket.SocketMessage message in channel.CachedMessages)
            {
                if(message.Author == author && (i - 1) < count)
                {
                    table[i] = message;
                    i++;
                }
                
            }
            return table;
        }*/

        public static DWebSocket.DiscordSocketClient GetClient()
        {
            return client;
        }

        public static void SetGameName(string gameName)
        {
            client.SetGameAsync(gameName);
        }
    }
}
