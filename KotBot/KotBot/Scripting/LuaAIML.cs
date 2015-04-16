/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIMLbot;
namespace KotBot.Scripting
{
    public static class LuaAIML
    {
        private static AIMLbot.Bot bot = new Bot();
        private static bool Initialized = false;
        [RegisterLuaFunction("aiml.init", "AIML Init", "")]
        public static void Init()
        {
            if (Initialized)
            {
                return;
            }
            bot.loadSettings("config/Settings.xml");
            bot.loadAIMLFromFiles();
            try
            {
                bot.loadFromBinaryFile("aiml.dat");
            }
            catch(Exception GoAway)
            {

            }
            Initialized = true;
        }
        [RegisterLuaFunction("aiml.reload", "AIML Reload", "")]
        public static void Reload()
        {
            Save();
            bot.loadSettings("config/Settings.xml");
            bot.loadAIMLFromFiles();
            bot.loadFromBinaryFile("aiml.dat");
        }

        [RegisterLuaFunction("aiml.save")]
        public static void Save()
        {
            try
            {
                bot.saveToBinaryFile("aiml.dat");
            }
            catch(Exception Ex)
            {

            }
        }
        [RegisterLuaFunction("aiml.respond", "AIML Respond", "")]
        public static string GetResponse(string input, User user)
        {
            Init();
            try
            {

                AIMLbot.User aimluser = new AIMLbot.User(user.GetName(), bot);
                Request request = new Request(input, aimluser, bot);
                Result result = bot.Chat(request);
                return result.Output;
            }
            catch(Exception ass)
            {
                return "I don't understand.";
            }
        }
    }
}
*/