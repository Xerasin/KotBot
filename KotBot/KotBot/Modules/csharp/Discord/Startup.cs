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
                DiscordManager.Start(token);
            }
            
        }
        public static void Close()
        {
            DiscordManager.Close();
        }

        static bool IsActive()
        {
            return true;
        }
    }
}
