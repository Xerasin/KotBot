using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotBot.Steam
{
    public class KSteamUser : User
    {
        public SteamKit2.SteamID steamid;
        public KSteamUser(SteamKit2.SteamID user)
        {
            this.steamid = user;
        }

        public SteamKit2.SteamID GetSteamID()
        {
            return steamid;
        }
        public override string GetName()
        {
            return SteamManager.steamFriends.GetFriendPersonaName(steamid);
        }

        public override void Message(string message)
        {
            SteamManager.steamFriends.SendChatMessage(steamid, SteamKit2.EChatEntryType.ChatMsg, message);
            Scripting.LuaHook.Call("MessageSent", new Message(new Steam.SteamPMClient(steamid, SteamManager.steamUser.SteamID), new Steam.KSteamUser(SteamManager.steamUser.SteamID), message));
        }
        public override string GetUserID()
        {
            return "|Steam|" + steamid.ToString();
        }
    }
}
