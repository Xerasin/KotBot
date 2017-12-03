using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotBot.Modules.DefaultCommands
{
    [CommandInfo("reload", "Reload modules", "Global", "Reloads all modules or only one")]
    public class Reload : Modules.ModuleCommand
    {
        private static DateTime lastReload = DateTime.Now.AddSeconds(-20);
        private static bool currentlyRunning = false;
        public override bool OnCall(List<string> args, MessageArgs originalMessage, string fullText)
        {
            if (currentlyRunning) return false;
            try
            {
                currentlyRunning = true;
                lastReload = DateTime.Now;
                if (fullText.Length > 0)
                {
                    if (!Modules.ModuleLoader.Load(fullText))
                        originalMessage.message.Reply($"Failed to load module {fullText}! *runs crying*");
                    else
                        originalMessage.message.Reply($"Success module {fullText} reloaded! hehehe <3");
                }
                else
                {
                    if (!Modules.ModuleLoader.LoadAllModules())
                        originalMessage.message.Reply($"Failed to load modules! *runs crying*");
                    else
                        originalMessage.message.Reply($"Success modules reloaded! hehehe <3");
                }
                currentlyRunning = false;
            }
            catch(Exception)
            {
                currentlyRunning = false;
            }
            return true;
        }

        public override bool ShouldCall(string source)
        {
            return (DateTime.Now - lastReload).Seconds > 1;
        }
    }
}
