using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KotBot.Modules
{

    public class CommandInfo : Attribute
    {
        public string Call;
        public string Name;
        public string Domain;
        public string Description;
        public CommandInfo(string Call, string Name, string Domain, string Description)
        {
            this.Call = Call;
            this.Name = Name;
            this.Domain = Domain;
            this.Description = Description;
        }
    }

    public class CommandInformation
    {
        public string Call { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Domain { get; set; } = "Global";
        public string Module { get; set; }
    }

    public abstract class ModuleCommand
    {
        public abstract bool ShouldCall(string source);
        public abstract bool OnCall(List<string> args, MessageArgs originalMessage);
        public virtual bool OnHelp(MessageArgs args)
        {
            args.message.Reply($"No help for command {Info.Name}");
            return false;
        }

        public CommandInformation Info;
    }

    public static class ModuleCommands
    {
        private static bool IsSetup = false;
        public static string commandStart { get; set; } = "!";
        public static Dictionary<string, ModuleCommand> commands = new Dictionary<string, ModuleCommand>();
        public static void Setup()
        {
            ModuleCommunications.MessageReceived += ModuleCommunications_MessageReceived;
            IsSetup = true;
            RegisterAssembliesComamnds("Global", Assembly.GetExecutingAssembly());
        }
        public static void RegisterAssembliesComamnds(string module, Assembly assembly)
        {
            if (!IsSetup)
            {
                Setup();
            }
            List<string> toRemove = new List<string>();
            foreach(KeyValuePair<string, ModuleCommand> command in commands)
            {
                if(command.Value.Info.Module == module)
                {
                    toRemove.Add(command.Key);
                }
            }
            foreach(string oldCmd in toRemove)
            {
                commands.Remove(oldCmd);
            }
            foreach (var type in assembly.GetTypes())
            {
                foreach(var attr in Attribute.GetCustomAttributes(type))
                {
                    if(typeof(CommandInfo) == attr.GetType())
                    {
                        CommandInfo cmdInfo = (CommandInfo)attr;
                        object instance = Activator.CreateInstance(type);
                        if(instance != null)
                        {
                            ModuleCommand newCmd = (ModuleCommand)instance;
                            newCmd.Info = new CommandInformation()
                            {
                                Call = cmdInfo.Call,
                                Name = cmdInfo.Name,
                                Description = cmdInfo.Description,
                                Domain = cmdInfo.Domain,
                                Module = module     
                            };

                            commands.Add(newCmd.Info.Call, (ModuleCommand)instance);
                        }
                    }
                }
            }
        }
        private static bool ModuleCommunications_MessageReceived(MessageArgs args)
        {
            if(!string.IsNullOrWhiteSpace(args.text) && args.text.StartsWith(commandStart))
            {
                List<string> words = new List<string>(args.text.Split(' '));
                if(words.Count > 0 && words[0].Length > commandStart.Length)
                {
                    string commandName = words[0].Substring(commandStart.Length);
                    words.RemoveAt(0);
                    if(commands.ContainsKey(commandName))
                    {
                        if(commands[commandName].ShouldCall(args.module))
                        {
                            commands[commandName].OnCall(words, args);
                        }
                    }
                }
            }
            return true;
        }
    }
}
