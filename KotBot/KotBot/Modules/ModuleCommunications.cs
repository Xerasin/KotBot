using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KotBot;
using KotBot.BotManager;
using System.Reflection;
using System.Runtime.InteropServices;

namespace KotBot.Modules
{
    [Serializable]
    public class MessageArgs : EventArgs
    {
        public Message message { get; set; }
        public User user { get; set; }
        public string text { get; set; }
        public string module { get; set; }
    }

    [Serializable]
    public delegate bool MessageEvent(MessageArgs args);

    [Serializable]
    public class ModuleArgs : EventArgs
    {
        public string Module { get; set; }
        public ModuleWrap ModuleWrap { get; set; }
    }
    [Serializable]
    public delegate bool ModuleEvent(ModuleArgs args);

    [Serializable]
    public static class ModuleCommunications
    {
        public static event MessageEvent MessageSent;
        public static event MessageEvent MessageReceived;
        public static event MessageEvent ShouldProcessMessage;
        public static event ModuleEvent ModuleLoaded;
        
        public static bool OnModuleLoaded(string module, ModuleWrap inputModule)
        {
            if (ModuleLoaded == null)
                return true;
            return ModuleLoaded(new ModuleArgs()
            {
                ModuleWrap = inputModule,
                Module = module
            });
        }

        public static bool OnMessageReceived(string module, Message inputMessage)
        {
            if (MessageReceived == null)
                return true;
            return MessageReceived(new MessageArgs()
            {
                message = inputMessage,
                user = inputMessage.GetSender(),
                text = inputMessage.GetMessage(),
                module = module
            });
        }

        public static bool OnMessageSent(string module, Message inputMessage)
        {
            if (MessageSent == null)
                return true;
            return MessageSent(new MessageArgs()
            {
                message = inputMessage,
                user = inputMessage.GetSender(),
                text = inputMessage.GetMessage(),
                module = module
            });
        }

        public static bool OnShouldProcessMessage(string module, Message inputMessage)
        {
            if (ShouldProcessMessage == null)
                return true;
            return ShouldProcessMessage(new MessageArgs()
            {
                message = inputMessage,
                user = inputMessage.GetSender(),
                text = inputMessage.GetMessage(),
                module = module
            });
        }
    }
}
