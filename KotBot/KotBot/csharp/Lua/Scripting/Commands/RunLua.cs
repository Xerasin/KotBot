using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KotBot.Modules;

namespace KotBot.Scripting
{
    [CommandInfo("l", "Run Lua!", "Global", "")]
    public class RunLua : Modules.ModuleCommand
    {
        public override bool OnCall(List<string> args, MessageArgs originalMessage, string completeText)
        {
            object returns;
            bool success = KotBot.Scripting.MainLua.DoString($"{completeText}", out returns, "message", originalMessage.message);
            if(success)
            {
                if(returns == null)
                {
                    //originalMessage.message.Reply("nil");
                }
                else
                {
                    originalMessage.message.Reply(returns.ToString());
                }
            }
            else
            {
                originalMessage.message.Reply($"Lua failed! {returns.ToString()}");
            }
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

    [CommandInfo("print", "Run Lua (Print)!", "Global", "")]
    public class PrintLua : Modules.ModuleCommand
    {
        public override bool OnCall(List<string> args, MessageArgs originalMessage, string completeText)
        {
            object returns;
            bool success = KotBot.Scripting.MainLua.DoString($"return {completeText}", out returns, "message", originalMessage.message);
            if(success)
            {
                if(returns == null)
                {
                    originalMessage.message.Reply("nil");
                }
                else
                {
                    originalMessage.message.Reply(returns.ToString());
                }
            }
            else
            {
                originalMessage.message.Reply($"Lua failed! {returns.ToString()}");
            }
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
