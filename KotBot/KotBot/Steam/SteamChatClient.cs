using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace KotBot.Steam
{
    public class SteamChatClient : Client
    {
        public SteamKit2.SteamID user;
        public SteamKit2.SteamID client;
        public SteamChatClient(SteamKit2.SteamID user, SteamKit2.SteamID client)
        {
            this.user = user;
            this.client = client;
        }
        public override void Message(string message)
        {
            SteamManager.steamFriends.SendChatRoomMessage(user, SteamKit2.EChatEntryType.ChatMsg, message);
            Scripting.LuaHook.Call("MessageSent", new Message(this, new Steam.KSteamUser(client), message));
        }
        public override string GetName()
        {
            return SteamManager.steamFriends.GetFriendPersonaName(client);
        }
        public override string GetLocationString()
        {
            return "|Steam|" + user.ToString();
        }
        public override string GetIP()
        {
            return "";
        }
        public override User FindUserByName(string name)
        {
            if (name == null) return null;
            for (int I = 0; I <= SteamManager.steamFriends.GetFriendCount(); I++ )
            {
                SteamKit2.SteamID steamid = SteamManager.steamFriends.GetFriendByIndex(I);
                if (SteamManager.steamFriends.GetFriendPersonaName(steamid).ToLower().Contains(name.ToLower()))
                {
                    return new KSteamUser(steamid);
                }
            }
            return null;
        }
    }
}
