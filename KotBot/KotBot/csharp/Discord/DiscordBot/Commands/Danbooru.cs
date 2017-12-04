using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KotBot.Modules;
using System.Net;
using Newtonsoft.Json.Linq;
namespace KotBot.DiscordBot
{
    public enum ProcessFilter {SFW, SNSFW, NSFW, NONE}; 
    public static class TagProcessor
    {
        public static string ProcessTags(string tags, ProcessFilter filter)
        {
            if(filter == ProcessFilter.NONE) return tags;
            if(filter == ProcessFilter.SFW)
            {
                if(!tags.Contains("rating:safe"))
                    tags = tags + " rating:safe";
                tags = tags.Replace("rating:explicit", "rating:safe");
                tags = tags.Replace("rating:questionable", "rating:safe");
                tags = tags.Replace("-rating:safe", "rating:safe");
            }
            else if(filter == ProcessFilter.SNSFW)
            {
                if(!tags.Contains("-rating:explicit"))
                    tags = tags + " -rating:explicit";
            }
            return tags;
        }
    }
    [Modules.CommandInfo(new string[] {"db", "danbooru", "hentai"}, "danbooru", "Discord", "Danbooru!")]
    public class Danbooru : Modules.ModuleCommand
    {
        public override bool OnCall(List<string> args, MessageArgs originalMessage, string completeText)
        {
            WebClient client = new WebClient();
            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            client.Proxy = null;
            ProcessFilter filter = ProcessFilter.NSFW;
            if(originalMessage.message.GetClient().GetType() == typeof(KotBot.DiscordBot.DiscordChatClient))
            {
                Discord.WebSocket.Socket​Text​Channel channel = ((KotBot.DiscordBot.DiscordChatClient)originalMessage.message.GetClient()).channel;
                /*if(!channel.IsNsfw)
                {
                    filter = ProcessFilter.SFW;
                }*/
            }
            string output = TagProcessor.ProcessTags(completeText, filter);
            try
            {
                client.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs e) =>
                {
                    if(e.Error != null)
                    {
                        originalMessage.message.Reply($"Failed to find danbooru image for {output}");
                        return;
                    }
                    JToken data = JToken.Parse(e.Result);
                    if (data.GetType() == typeof(JArray))
                    {
                        JArray arrayData = (JArray)data;
                        if (arrayData.Count == 0)
                        {
                            originalMessage.message.Reply($"Failed to find danbooru image for {output}");
                            return;
                        }
                        if (arrayData[0] != null && arrayData[0].GetType() == typeof(JObject))
                        {
                            JObject firstImage = (JObject)arrayData[0];
                            originalMessage.message.Reply($"Random danbooru! https://danbooru.donmai.us/posts/{firstImage["id"]}");
                        }
                        else
                        {
                            originalMessage.message.Reply($"Failed to find danbooru image for {output}");
                        }
                    }
                };
                client.DownloadStringAsync(new Uri($"https://danbooru.donmai.us/posts.json?limit=1&random=true&tags={output}"));
            }
            catch(WebException e)
            {
                originalMessage.message.Reply($"Failed to find danbooru image for {output}");
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
    [Modules.CommandInfo("gelbooru", "gelbooru", "Discord", "Gelbooru!")]
    public class Gelbooru : Modules.ModuleCommand
    {
        public override bool OnCall(List<string> args, MessageArgs originalMessage, string completeText)
        {
            originalMessage.message.Reply("Gelbooru has their API disabled! :(");
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
