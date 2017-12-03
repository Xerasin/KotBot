using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IrcDotNet;
namespace KotBot
{
    public struct ClientInfo
    {
        public IrcDotNet.IrcUserRegistrationInfo registrationInfo;
        public IrcClient client;
        public string IP;
    }
    public static class IRCBot
    {
        private static Dictionary<string, ClientInfo> allClients = new Dictionary<string, ClientInfo>();
        [Scripting.RegisterLuaFunction("IRC.Connect")]
        public static IrcClient Connect(string IP, IrcDotNet.IrcUserRegistrationInfo info)
        {
            var client = new StandardIrcClient();
            return ConnectWithClient(client, IP, info);
        }
        public static StandardIrcClient ConnectWithClient(StandardIrcClient client, string IP, IrcDotNet.IrcUserRegistrationInfo info)
        {
            client.FloodPreventer = new IrcStandardFloodPreventer(100, 2000);
            client.Connected += client_Connected;
            client.Disconnected += client_Disconnected;
            client.Registered += client_Registered;
            client.ConnectFailed += client_ConnectFailed;
            client.Connect(System.Net.IPAddress.Parse(IP), false, info);
            ClientInfo cInfo = new ClientInfo();
            cInfo.client = client;
            cInfo.registrationInfo = info;
            cInfo.IP = IP;
            allClients[IP] = cInfo;
            // Add new client to collection.
            return client;
        }

        static void client_ConnectFailed(object sender, IrcErrorEventArgs e)
        {
            throw new NotImplementedException();
        }
        [Scripting.RegisterLuaFunction("IRC.Disconnect")]
        public static void Disconnect(IrcClient client)
        {
            client.Disconnect();
        }
        [Scripting.RegisterLuaFunction("IRC.Reconnect")]
        public static IrcClient Reconnect(IrcClient client)
        {
            ClientInfo info = GetInfo(client);
            if (info.client != null)
            {
                return Connect(info.IP, info.registrationInfo);
            }
            return null;
        }
        [Scripting.RegisterLuaFunction("IRC.IsConnected")]
        public static bool IsConnected(string IP)
        {
            foreach(KeyValuePair<string, ClientInfo> client in allClients)
            {
                if (client.Key == IP && client.Value.client.IsConnected)
                {
                    return true;
                }
            }
            return false;
        }

        [Scripting.RegisterLuaFunction("IRC.GetBlankInfo")]
        public static IrcUserRegistrationInfo GetBlankInfo()
        {
            return new IrcUserRegistrationInfo();
        }

        [Scripting.RegisterLuaFunction("IRC.GetIP")]
        public static string GetIP(IrcClient inClient)
        {
            foreach (KeyValuePair<string, ClientInfo> client in allClients)
            {
                if (client.Value.client == inClient)
                {
                    return client.Key;
                }
            }
            return "";
        }
        [Scripting.RegisterLuaFunction("IRC.GetInfo")]
        public static ClientInfo GetInfo(IrcClient inClient)
        {
            foreach (KeyValuePair<string, ClientInfo> client in allClients)
            {
                if (client.Value.client == inClient)
                {
                    return client.Value;
                }
            }
            return new ClientInfo();
        }
        public static void JoinChannel(IrcClient client, string channel)
        {
            client.Channels.Join(channel);
        }
        private static void IrcClient_LocalUser_JoinedChannel(object sender, IrcChannelEventArgs e)
        {
            var localUser = (IrcLocalUser)sender;

            e.Channel.MessageReceived += Channel_MessageReceived;
        }

        static void Channel_MessageReceived(object sender, IrcMessageEventArgs e)
        {
            IrcChannel channel = (IrcChannel)sender;
            IrcLocalUser user = channel.Client.LocalUser;
            IrcUser messageSender = (IrcUser)e.Source;
            ModuleCommunications.MessageReceived(new MessageArgs()
            {
                message = new Message(new IRC.IRCChannelClient(user, channel), new IRC.IRCUser(user, messageSender), e.Text)
            });
        }
        static void client_Registered(object sender, EventArgs e)
        {
            ((IrcClient)sender).LocalUser.LeftChannel += LocalUser_LeftChannel;
            ((IrcClient)sender).LocalUser.JoinedChannel += IrcClient_LocalUser_JoinedChannel;
            ((IrcClient)sender).LocalUser.MessageReceived += LocalUser_MessageReceived;
            ((IrcClient)sender).LocalUser.NoticeReceived += LocalUser_NoticeReceived;
            ((IrcClient)sender).Disconnected += IRCBot_Disconnected;
        }

        static void LocalUser_NoticeReceived(object sender, IrcMessageEventArgs e)
        {
            IrcLocalUser user = (IrcLocalUser)sender;
            IrcUser messageSender = (IrcUser)e.Source;
            ModuleCommunications.MessageRecieved(new MessageArgs()
            {
                message = new Message(new IRC.IRCPMClient(user, messageSender), new IRC.IRCUser(user, messageSender), e.Text)
            });
        }
        [Scripting.RegisterLuaFunction("IRC.Register")]
        public static void Register(IrcClient client, string password, string email)
        {
            client.LocalUser.SendMessage("NickServ", "register " + password + " " + email);
        }
        static void IRCBot_Disconnected(object sender, EventArgs e)
        {
        }

        static void LocalUser_LeftChannel(object sender, IrcChannelEventArgs e)
        {
            e.Channel.MessageReceived -= Channel_MessageReceived;
        }

        static void LocalUser_MessageReceived(object sender, IrcMessageEventArgs e)
        {
            IrcLocalUser user = (IrcLocalUser)sender;
            IrcUser messageSender = (IrcUser)e.Source;
            ModuleCommunications.MessageRecieved(new MessageArgs()
            {
                message = new Message(new IRC.IRCPMClient(user, messageSender), new IRC.IRCUser(user, messageSender), e.Text)
            });
        }

        static void client_Disconnected(object sender, EventArgs e)
        {
            
        }

        static void client_Connected(object sender, EventArgs e)
        {
        }
    }
}
