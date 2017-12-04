using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KotBot.Modules;

namespace KotBot.Modules.DefaultCommands
{
    [CommandInfo(new string[] {"help", "how"}, "Help Command", "Global", "Get loaded modules, commands from a module, or command help")]
    public class HelpCommand : Modules.ModuleCommand
    {
        public override bool OnCall(List<string> args, MessageArgs originalMessage, string fullText)
        {
            string moduleOrCommandName = null;
            string restOfText = "";
            if(args.Count > 0) 
            {
                moduleOrCommandName = args[0];
                if(fullText.Length > moduleOrCommandName.Length + 1)
                {
                    restOfText = fullText.Substring(moduleOrCommandName.Length + 1);
                }
            }
            if(!Modules.ModuleCommands.GetHelpForModuleCommand(moduleOrCommandName, originalMessage, restOfText))
            {
                originalMessage.message.Reply($"Failed to find help for {fullText}");
            }
            return true;
        }

        public override bool ShouldCall(string source)
        {
            return true;
        }
    }
}
