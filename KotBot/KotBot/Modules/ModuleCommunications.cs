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
    }

    [Serializable]
    public class CommunicationMethod
    {

        public string type;
        public string method;
        public object[] variables = null;

        public void Load()
        {
            var app = AppDomain.CurrentDomain;
            foreach (var assembly in app.GetAssemblies())
            {
                if (assembly.GetName().Name == "KotBot")
                {
                    var pluginType = assembly.GetType(type);
                    if (pluginType != null)
                    {
                        var methods = pluginType.GetMethods();
                        var methodInfo = pluginType.GetMethod(method, BindingFlags.Static | BindingFlags.Public);
                        var outMain = methodInfo.Invoke(null, variables);
                    }
                }


            }
        }

        public static AppDomain Execute(string type, string method, params object[] parameters)
        {
            CommunicationMethod cMethod = new CommunicationMethod { type = type, method = method, variables = parameters };
            var app = AsmLoad.LoadedInstance.Domain;
            app.DoCallBack(new CrossAppDomainDelegate(cMethod.Load));
            return app;
        }
    }

    [Serializable]
    public delegate bool MessageEvent(MessageArgs args);
    [Serializable]
    public static class ModuleCommunications
    {
        public static event MessageEvent MessageSent;
        public static event MessageEvent MessageReceived;
        public static event MessageEvent ShouldProcessMessage;
        public static bool OnMessageReceived(Message inputMessage)
        {
            if (AsmLoad.LoadedInstance != null)
            {
                CommunicationMethod.Execute("KotBot.Modules.ModuleCommunications", "OnMessageReceived", inputMessage);
                return true;
            }
            if (MessageReceived == null)
                return true;
            return MessageReceived(new MessageArgs()
            {
                message = inputMessage,
                user = inputMessage.GetSender(),
                text = inputMessage.GetMessage()
            });
        }

        public static bool OnMessageSent(Message inputMessage)
        {
            if (AsmLoad.LoadedInstance != null)
            {
                CommunicationMethod.Execute("KotBot.Modules.ModuleCommunications", "OnMessageSent", inputMessage);
                return true;
            }
            if (MessageSent == null)
                return true;
            return MessageSent(new MessageArgs()
            {
                message = inputMessage,
                user = inputMessage.GetSender(),
                text = inputMessage.GetMessage()
            });
        }

        public static bool OnShouldProcessMessage(Message inputMessage)
        {
            AppDomain domain = AppDomain.CurrentDomain;
            if (AsmLoad.LoadedInstance != null)
            {
                CommunicationMethod.Execute("KotBot.Modules.ModuleCommunications", "OnShouldProcessMessage", inputMessage);
                return true;
            }
            if (ShouldProcessMessage == null)
                return true;
            return ShouldProcessMessage(new MessageArgs()
            {
                message = inputMessage,
                user = inputMessage.GetSender(),
                text = inputMessage.GetMessage()
            });
        }
    }
}
