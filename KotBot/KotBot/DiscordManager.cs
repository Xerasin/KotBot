using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KotBot.Scripting;
using System.Runtime.InteropServices;
using SteamKit2;
using SteamKit2.Unified.Internal;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Net;
using Discord.Net;
using System.Timers;
using System.Diagnostics;
using System.Web;
using Discord.Audio;
namespace KotBot.DiscordBot
{
    public static class DiscordManager
    {
        public static Discord.DiscordClient client;
        [RegisterLuaFunction("Discord.Start")]
        public static void Start(string token)
        {
            if (client != null)
            {
                client.Dispose();
            }

            client = new Discord.DiscordClient();

            client.MessageReceived += Client_MessageReceived;
            client.UsingAudio(x => // Opens an AudioConfigBuilder so we can configure our AudioService
            {
                x.Mode = AudioMode.Outgoing; // Tells the AudioService that we will only be sending audio
            });

            client.ExecuteAndWait(async () => {
                await client.Connect(token);
                Console.WriteLine($"Discord Connected, User: {client.CurrentUser.Name}");
             });

            
        }

        private static void Client_MessageReceived(object sender, Discord.MessageEventArgs e)
        {
            if (e.Message.IsAuthor)
            {
                return;
            }
            if(e.Channel.IsPrivate)
            {
                return;
            }
            Message chatMessage = new Message(new DiscordChatClient(e.User, e.Channel), new DiscordUser(e.User), e.Message.Text);
            object[] ShouldCallHook = Scripting.LuaHook.Call("ShouldCallMessage", chatMessage);
            try
            {
                if (ShouldCallHook.Length != 0)
                {
                    object[] returns = (object[])ShouldCallHook[0];
                    if (returns != null && returns.Length != 0)
                    {
                        bool? x = returns[0] as bool?;
                        if (x != null)
                        {
                            if (x.Value == false)
                            {
                                Scripting.LuaHook.Call("MessageRecieved", chatMessage);
                            }
                        }
                    }

                }
                else
                {
                    Scripting.LuaHook.Call("MessageRecieved", chatMessage);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        /*private static void Client_SocketClosed(object sender, DiscordSocketClosedEventArgs e)
        {
            Scripting.LuaHook.Call("DiscordSocketClosed", e.Reason, e.Code);
        }*/

        [RegisterLuaFunction("Discord.SetAvatar")]
        public static void SetAvatar(string url)
        {
            WebClient wc = new WebClient();
            wc.Proxy = null;
            File.Delete("temp.png");
            byte[] file = wc.DownloadData(url);
            using (BinaryWriter writer = new BinaryWriter(File.OpenWrite("temp.png")))
            {
                writer.Write(file);
                writer.Flush();
                writer.Close();
            }
            client.CurrentUser.Edit(avatar: File.Open("temp.png", FileMode.OpenOrCreate));
        }
        [RegisterLuaFunction("Discord.GetMessageHistory")]
        public static NLua.LuaTable GetMessageHistory(Discord.Channel channel, Discord.User author, double count)
        {
            NLua.LuaTable table = MainLua.GetNewTable();
            int i = 1;
            foreach (Discord.Message message in channel.Messages)
            {
                if(message.User == author && (i - 1) < count)
                {
                    table[i] = message;
                    i++;
                }
                
            }
            return table;
        }
        [RegisterLuaFunction("Discord.GetClient")]
        public static Discord.DiscordClient GetClient()
        {
            return client;
        }
        [RegisterLuaFunction("Discord.SetGameName")]
        public static void SetGameName(string gameName)
        {
            client.SetGame(gameName);
        }
        
        /*private static void Client_PrivateMessageReceived(object sender, DiscordPrivateMessageEventArgs e)
        {
            if(e.Author.Username == "Websocket.Close")
            {

            }
            if (e.Author == client.Me)
            {
                return;
            }
            Message chatMessage = new Message(new DiscordPMClient(client.Me, e.Channel.Recipient), new DiscordUser(e.Channel.Recipient), e.Message);
            object[] ShouldCallHook = Scripting.LuaHook.Call("ShouldCallMessage", chatMessage, true);
            try
            {
                if (ShouldCallHook.Length != 0)
                {
                    object[] returns = (object[])ShouldCallHook[0];
                    if (returns != null && returns.Length != 0)
                    {
                        bool? x = returns[0] as bool?;
                        if (x != null)
                        {
                            if (x.Value == false)
                            {
                                Scripting.LuaHook.Call("MessageRecieved", chatMessage);
                            }
                        }
                    }

                }
                else
                {
                    Scripting.LuaHook.Call("MessageRecieved", chatMessage);
                }
            }
            catch(Exception)
            {

            }
            
        }*/
    }
}
