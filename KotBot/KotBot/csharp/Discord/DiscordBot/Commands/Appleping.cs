using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KotBot.Modules;

namespace KotBot.DiscordBot
{
    [CommandInfo("apple", "Apple Ping!", "Global", "Apples are great!")]
    public class ApplePing : Modules.ModuleCommand
    {
        public override bool OnCall(List<string> args, MessageArgs originalMessage, string completeText)
        {
            originalMessage.message.Reply("Apples are great! *giggles*");
            return true;
        }

        public override bool ShouldCall(string source)
        {
            if(source == "Discord")
            {
                return true;
            }
            return false;
        }
    }
}
