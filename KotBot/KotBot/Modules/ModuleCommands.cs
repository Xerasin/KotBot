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
        public List<string> Aliases;
        public CommandInfo(object Call, string Name, string Domain, string Description)
        {
            string newCall = "";
            List<string> aliases = null;
            if (typeof(string[]) == Call.GetType())
            {
                aliases = new List<string>((string[])Call);
                newCall = aliases[0];
                aliases.RemoveAt(0);
            }
            else if (typeof(string) == Call.GetType())
            {
                newCall = (string)Call;
            }
            else
            {
                newCall = Call.ToString();
            }
            this.Call = newCall;
            this.Name = Name;
            this.Domain = Domain;
            this.Description = Description;
            if(aliases != null)
            {
                this.Aliases = new List<string>(aliases);
            }
            
        }
    }

    public class CommandInformation
    {
        public string Call { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Domain { get; set; } = "Global";
        public string Module { get; set; }
        public List<string> Aliases { get; set; }
    }

    public abstract class ModuleCommand
    {
        public abstract bool ShouldCall(string source);
        public abstract bool OnCall(List<string> args, MessageArgs originalMessage, string fullText);
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
        public static char seperator { get; set; } = ' ';
        public static Dictionary<string, ModuleCommand> commands = new Dictionary<string, ModuleCommand>();
        public static Dictionary<string, string> aliases = new Dictionary<string, string>();
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
                if(aliases.ContainsValue(oldCmd))
                {
                    List<string> aliasesToRemove = new List<string>();
                    foreach (var alias in aliases)
                    {
                        aliasesToRemove.Add(alias.Key);
                    }

                    foreach(string alias in aliasesToRemove)
                    {
                        aliases.Remove(alias);
                    }
                }
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
                                Aliases = cmdInfo.Aliases,
                                Module = module
                            };
                            if(!commands.ContainsKey(newCmd.Info.Call))
                            {
                                commands.Add(newCmd.Info.Call, (ModuleCommand)instance);
                                if (newCmd.Info.Aliases != null)
                                {
                                    foreach (var alias in newCmd.Info.Aliases)
                                    {
                                        if(!aliases.ContainsKey(alias))
                                        {
                                            aliases.Add(alias, newCmd.Info.Call);
                                        }
                                    }
                                }
                            }
                            
                        }
                    }
                }
            }
        }
        private static bool ModuleCommunications_MessageReceived(MessageArgs args)
        {
            if(!string.IsNullOrWhiteSpace(args.text) && args.text.StartsWith(commandStart))
            {
                List<string> words = new List<string>(args.text.Split(seperator));
                if(words.Count > 0 && words[0].Length > commandStart.Length)
                {
                    string commandName = words[0].Substring(commandStart.Length);
                    string restOfText = "";
                    if(args.text.Length > words[0].Length + 1)
                    {
                        restOfText = args.text.Substring(words[0].Length + 1);
                    }
                    words.RemoveAt(0);
                    if(!commands.ContainsKey(commandName))
                    {
                        if(aliases.ContainsKey(commandName))
                        {
                            commandName = aliases[commandName];
                        }
                    }
                    if(commands.ContainsKey(commandName))
                    {
                        try
                        {
                            if (commands[commandName].ShouldCall(args.module))
                            {
                                commands[commandName].OnCall(words, args, restOfText);
                            }
                        }
                        catch(Exception commandFailure)
                        {
                            args.message.Reply($"Command {commandName} failed \"{commandFailure.Message}\" \n {commandFailure.StackTrace}");
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
