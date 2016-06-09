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
using DiscordSharp;

namespace KotBot.Discord
{
    public static class DiscordManager
    {
        public static DiscordClient client;
        [RegisterLuaFunction("Discord.Start")]
        public static void Start(string token)
        {
            client = new DiscordClient(token, true);

            client.Connected += (sender, e) =>
            {
                //Console.WriteLine($"Connected! User: {e.User.Username}");
            };
            client.MessageReceived += Client_MessageReceived;
            client.PrivateMessageReceived += Client_PrivateMessageReceived;
            client.SendLoginRequest();
            Thread t = new Thread(client.Connect);
            t.Start();
        }

        private static void Client_PrivateMessageReceived(object sender, DiscordPrivateMessageEventArgs e)
        {
            if (e.Author == client.Me)
            {
                return;
            }
            Message chatMessage = new Message(new DiscordPMClient(client.Me, e.Author), new DiscordUser(e.Author), e.Message);
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
            catch(Exception ex)
            {

            }
            
        }

        private static void Client_MessageReceived(object sender, DiscordSharp.Events.DiscordMessageEventArgs e)
        {
            if(e.Author == client.Me)
            {
                return;
            }
            Message chatMessage = new Message(new DiscordChatClient(e.Author, e.Channel), new DiscordUser(e.Author), e.MessageText);
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

            }
        }
    }
}
