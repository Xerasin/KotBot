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
        public override bool OnCall(List<string> args, MessageArgs originalMessage)
        {
            if (args.Count > 0)
            {
                if (!Modules.ModuleLoader.Load(args[0]))
                    originalMessage.message.Reply($"Failed to load module {args[0]}! *runs crying*");
                else
                    originalMessage.message.Reply($"Success module {args[0]} reloaded! hehehe <3");
            }
            else
            {
                if(!Modules.ModuleLoader.LoadAllModules())
                    originalMessage.message.Reply($"Failed to load modules! *runs crying*");
                else
                    originalMessage.message.Reply($"Success modules reloaded! hehehe <3");
            }
            return true;
        }

        public override bool ShouldCall(string source)
        {
            throw new NotImplementedException();
        }
    }
}
