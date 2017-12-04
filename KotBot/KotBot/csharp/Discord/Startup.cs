using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KotBot;
using KotBot.Modules;
using KotBot.DiscordBot;
using Newtonsoft.Json.Linq;
namespace Plugin
{
    public class Plugin
    {
        [ModuleInfo("Discord KotBot", "0.0.5", "Katherine Loveland", "Discord connectivity to KotBot")]
        public static void Main(string[] args)
        {
            string token = KotBot.Modules.ModuleConfig.GetString("Discord", "BotToken", "");
            if (!string.IsNullOrWhiteSpace(token))
            {
                Task<bool> startupCheck = DiscordManager.Start(token);
                startupCheck.Wait();
                if(!startupCheck.Result)
                {
                    Log.Error("Discord failed to connect!");
                    return;
                }
            } 

            ModuleCommunications.ModuleLoaded += ModuleCommunications_ModuleLoaded;
        }

        private static bool ModuleCommunications_ModuleLoaded(ModuleArgs args)
        {
            if(args.Module == "Lua") 
            {
                KotBot.DiscordBot.LuaDiscord.LoadAll(args.ModuleWrap.Assembly);
            }
            return true;
        }
        public static void Close()
        {
            DiscordManager.Close();
            ModuleCommunications.ModuleLoaded -= ModuleCommunications_ModuleLoaded;
        }

        static bool IsActive()
        {
            return true;
        }
    }
}
