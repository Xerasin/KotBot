using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KotBot.Modules;
using System.Net;
using Newtonsoft.Json.Linq;
using Discord;
using Discord.WebSocket;
using Discord.Rest;
using System.Drawing;
using System.Text.RegularExpressions;

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
    public class BooruData
    {
        public string Title {get; set;} = null;
        public string Footer {get; set;} = null;
        public string Tags {get; set;} = null;
        public string Artist {get; set;} = null;
        public string Score {get; set;} = null;
        public string Image {get; set;} = null;
        public string Url {get; set;} = null;
    }
    public delegate void ProcessingFinished(BooruSender value);
    public class BooruSender
    {
        public event ProcessingFinished Ready;
        internal List<BooruData> data = new List<BooruData>();
        public int CurrentData {get; set;} = 0;
        public BooruSender()
        {

        }
        public EmbedBuilder BuildEmbed(MessageArgs originalMessage, BooruData data)
        {
            try
            {
                var builder = new EmbedBuilder();
                if(!String.IsNullOrWhiteSpace(data.Title))
                    builder.WithTitle(data.Title);
                if(!String.IsNullOrWhiteSpace(data.Footer))
                    builder.WithFooter(data.Footer);
                if(!String.IsNullOrWhiteSpace(data.Tags) && data.Tags.Length <= 1024)
                    builder.AddField("Tags", data.Tags, true);
                if(!String.IsNullOrWhiteSpace(data.Artist))
                    builder.AddField("Artist", data.Artist, true);
                if(!String.IsNullOrWhiteSpace(data.Score))
                    builder.AddField("Score", data.Score, true);
                if(!String.IsNullOrWhiteSpace(data.Image))
                    builder.WithImageUrl(data.Image);
                if(!String.IsNullOrWhiteSpace(data.Url))
                    builder.WithUrl(data.Url);
                builder.WithColor(Color.Red);
                return builder;
            }
            catch(Exception e)
            {
                Log.Error(e.ToString());
            }
            return null;
        }
        public void GoNext()
        {
            
        }
        public void GoPrevious()
        {
            
        }
        internal void OnReady()
        {
            if(Ready == null) return;
            Ready(this);
        }
        public RestUserMessage Send(MessageArgs originalMessage)
        {
            try
            {
                if(data.Count == 0) return null;
                if(data.Count < CurrentData) return null;
                var builder = this.BuildEmbed(originalMessage, data[CurrentData]);
                if(builder == null) return null;
                
                RestUserMessage message = null;
                if(originalMessage.message.GetClient().GetType() == typeof(KotBot.DiscordBot.DiscordChatClient))
                {
                    var client = ((KotBot.DiscordBot.DiscordChatClient)originalMessage.message.GetClient());
                    Discord.WebSocket.SocketGuildUser user = client.member;
                    if(user != null)
                    {
                        var authorBuilder = new EmbedAuthorBuilder();
                        authorBuilder.WithIconUrl(user.GetAvatarUrl());
                        authorBuilder.WithName((!String.IsNullOrWhiteSpace(user.Nickname)) ? user.Nickname : user.Username);
                        builder.WithAuthor(authorBuilder);
                    }

                    Discord.WebSocket.Socket​Text​Channel channel = client.channel;
                    Embed embed = builder.Build();
                    message = (RestUserMessage)client.Message("", embed);
                }
                else if(originalMessage.message.GetClient().GetType() == typeof(KotBot.DiscordBot.DiscordPMClient))
                {
                    
                    KotBot.DiscordBot.DiscordPMClient client = ((KotBot.DiscordBot.DiscordPMClient)originalMessage.message.GetClient());
                    if(client != null)
                    {
                        Discord.WebSocket.SocketUser member = client.other;
                        if(member != null)
                        {
                            var authorBuilder = new EmbedAuthorBuilder();
                            authorBuilder.WithIconUrl(member.GetAvatarUrl());
                            authorBuilder.WithName(member.Username);
                            builder.WithAuthor(authorBuilder);
                        }
                        Embed embed = builder.Build();
                        message = (RestUserMessage)client.Message("", embed);
                    }
                }
                else
                {
                    message = (RestUserMessage)originalMessage.message.Reply(data[CurrentData].Url);
                }
                return message;
            }
            catch(Exception sendFailed)
            {
                Log.Error(sendFailed.ToString());
                originalMessage.message.Reply("Failure?!?!?");;
                return null;
            }
        }
        public virtual bool Setup(ProcessFilter filter, string output)
        {
            return false;
        }
    }
    public class DanbooruSender : BooruSender
    {
        private const int MAX_LOOK = 75;
        public DanbooruSender() : base()
        {
            
        }
        public override bool Setup(ProcessFilter filter, string output)
        {
            try
            {
                WebClient client = new WebClient();
                client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                client.Proxy = null;
                client.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs e) =>
                {
                    if(e.Error != null)
                    {
                        this.OnReady();
                        return;
                    }
                    JToken jsonData = JToken.Parse(e.Result);
                    if (jsonData.GetType() == typeof(JArray))
                    {
                        JArray arrayData = (JArray)jsonData;
                        if (arrayData == null) return;
                        if (arrayData.Count == 0)
                        {
                            this.OnReady();
                            return;
                        }
                        for(int I = 0; I < arrayData.Count; I++)
                        {
                            int matchImage = I;
                            if (arrayData[matchImage] != null && arrayData[matchImage].GetType() == typeof(JObject))
                            {
                                JObject firstImage = (JObject)arrayData[matchImage];
                                string rating = null;
                                if(firstImage["rating"] != null)
                                    rating = firstImage["rating"].ToString();
                                
                                if(rating == null) continue;
                                if(filter == ProcessFilter.SFW && (rating == "e" || rating == "q")) continue;
                                if(filter == ProcessFilter.SNSFW && rating == "e") continue;

                                string tags = "abc";
                                string id = "";
                                string url = "https://danbooru.donmai.us/";
                                string file_url = "https://cdn0.iconfinder.com/data/icons/shift-free/32/Error-128.png";
                                string artist = "";
                                string score = "???";
                                
                                if(firstImage["tag_string"] != null)
                                    tags = firstImage["tag_string"].ToString();

                                if(firstImage["id"] != null)
                                    id = firstImage["id"].ToString();

                                if(!String.IsNullOrWhiteSpace(id))
                                    url = $"https://danbooru.donmai.us/posts/{id}";

                                if(firstImage["large_file_url"] != null)
                                    file_url = $"https://danbooru.donmai.us{firstImage["large_file_url"].ToString()}";
                                if(file_url == "https://danbooru.donmai.us")
                                {
                                    if(firstImage["file_url"] != null)
                                        file_url = $"https://danbooru.donmai.us{firstImage["file_url"].ToString()}";
                                    else
                                        file_url = "https://cdn0.iconfinder.com/data/icons/shift-free/32/Error-128.png";
                                }

                                if(firstImage["tag_string_artist"] != null)
                                    artist = firstImage["tag_string_artist"].ToString();

                                if(firstImage["score"] != null)
                                    score = firstImage["score"].ToString();

                                data.Add(new BooruData()
                                {
                                    Title = "Random Danbooru!",
                                    Footer = $"\"{output}\" - {id} ({rating})",
                                    Tags = tags,
                                    Image = file_url,
                                    Artist = artist,
                                    Score = score,
                                    Url = url
                                });
                            }
                        }
                    }
                    this.OnReady();
                };
                client.DownloadStringAsync(new Uri($"https://danbooru.donmai.us/posts.json?limit={MAX_LOOK}&random=true&tags={output}"));
            }
            catch(Exception danbooruFailed)
            {
                Log.Error(danbooruFailed.ToString());
                return false;
            }
            return true;
        }
    }

    public class GelbooruSender : BooruSender
    {
        public Random random = new Random();
        private const int MAX_LOOK = 75;
        public GelbooruSender() : base()
        {
            
        }
        public override bool Setup(ProcessFilter filter, string output)
        {
            try
            {
                output = TagProcessor.ProcessTags(output, filter);
                string cookies = KotBot.Modules.ModuleConfig.GetString("Discord", "GelbooruCookies", "");
                if (cookies == "")
                {
                    this.OnReady();
                    return false;
                }
                WebClient client = new WebClient();
                client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                client.Proxy = null;
                client.Headers.Add(HttpRequestHeader.Cookie, cookies);
                client.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs e) =>
                {
                    if(e.Error != null)
                    {
                        this.OnReady();
                        return;
                    }
                    
                    Regex rgx = new Regex("count=\"([\\d]+)\"", RegexOptions.IgnoreCase);
                    MatchCollection matches = rgx.Matches(e.Result);
                    if (matches.Count == 0 || matches[0].Groups.Count == 0)
                    {
                        this.OnReady();
                        return;
                    }
                    int count = 0;
                    try
                    {
                        count = Convert.ToInt32(matches[0].Groups[1].Value);
                    }
                    catch(Exception whatEven)
                    {
                        this.OnReady();
                        Log.Error(whatEven.ToString());
                        return;
                    }
                    if (count == 0)
                    {
                        this.OnReady();
                        return;
                    }
                    int picked = random.Next(0, count - 1);
                    WebClient client2 = new WebClient();
                    client2.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                    client2.Proxy = null;
                    client2.Headers.Add(HttpRequestHeader.Cookie, cookies);
                    client2.DownloadStringCompleted += (object sender2, DownloadStringCompletedEventArgs e2) =>
                    {
                        try
                        {
                            if(e2.Error != null)
                            {
                                return;
                            }
                            JToken jsonData = JToken.Parse(e2.Result);
                            if (jsonData.GetType() == typeof(JArray))
                            {
                                JArray arrayData = (JArray)jsonData;
                                if (arrayData == null) return;
                                if (arrayData.Count == 0)
                                {
                                    return;
                                }
                                for(int I = 0; I < arrayData.Count; I++)
                                {
                                    int matchImage = I;
                                    if (arrayData[matchImage] != null && arrayData[matchImage].GetType() == typeof(JObject))
                                    {
                                        JObject firstImage = (JObject)arrayData[matchImage];
                                        string rating = null;
                                        string tags = "abc";
                                        string id = "";
                                        string url = "https://gelbooru.com/";
                                        string file_url = "https://cdn0.iconfinder.com/data/icons/shift-free/32/Error-128.png";
                                        string artist = "";
                                        string score = "???";
                                        

                                        if(firstImage["rating"] != null)
                                            rating = firstImage["rating"].ToString();
                                            
                                        if(firstImage["tags"] != null)
                                            tags = firstImage["tags"].ToString();

                                        if(firstImage["id"] != null)
                                            id = firstImage["id"].ToString();

                                        if(!String.IsNullOrWhiteSpace(id))
                                            url = $"http://gelbooru.com/index.php?page=post&s=view&id={id}";

                                        if(firstImage["file_url"] != null)
                                            file_url = firstImage["file_url"].ToString();

                                        if(firstImage["score"] != null)
                                            score = firstImage["score"].ToString();

                                        data.Add(new BooruData()
                                        {
                                            Title = "Random Gelbooru!",
                                            Footer = $"\"{output}\" - {id} ({rating}) (picked #{picked} of {count})",
                                            Tags = tags,
                                            Image = file_url,
                                            Score = score,
                                            Url = url
                                        });
                                    }
                                }
                            }
                            this.OnReady();
                        }
                        catch(Exception gelbooruFailed)
                        {
                            Log.Error(gelbooruFailed.ToString());
                        }
                    };
                    client2.DownloadStringAsync(new Uri($"http://gelbooru.com/index.php?page=dapi&s=post&q=index&tags={output}&json=1&limit=1&pid={picked}"));
                };
                client.DownloadStringAsync(new Uri($"http://gelbooru.com/index.php?page=dapi&s=post&q=index&tags={output}&limit=1"));
            }
            catch(Exception gelboooruFailed)
            {
                Log.Error(gelboooruFailed.ToString());
                return false;
            }
            return true;
        }
    }


    [Modules.CommandInfo(new string[] {"db", "danbooru", "hentai"}, "danbooru", "Discord", "Danbooru!")]
    public class Danbooru : Modules.ModuleCommand
    {
        
        public override bool OnCall(List<string> args, MessageArgs originalMessage, string completeText)
        {
            ProcessFilter filter = ProcessFilter.NSFW;
            if(originalMessage.message.GetClient().GetType() == typeof(KotBot.DiscordBot.DiscordChatClient))
            {
                Socket​Text​Channel channel = ((KotBot.DiscordBot.DiscordChatClient)originalMessage.message.GetClient()).channel;
                if(!channel.IsNsfw)
                {
                    filter = ProcessFilter.SFW;
                }
            }
            string output = TagProcessor.ProcessTags(completeText, ProcessFilter.NSFW);
            try
            {
                DanbooruSender dSender = new DanbooruSender();
                if(!dSender.Setup(filter, output))
                {
                    originalMessage.message.Reply($"Failed to initialize danbooru!");
                }
                dSender.Ready += (BooruSender sender) =>
                {
                    RestUserMessage message = dSender.Send(originalMessage);
                    if(message == null)
                        originalMessage.message.Reply($"Failed to find danbooru image for \"{output}\"");
                };
            }
            catch(WebException e)
            {
                originalMessage.message.Reply($"Failed to find danbooru image for \"{output}\"");
            }
            catch(Exception e)
            {
                originalMessage.message.Reply($"Failed to find danbooru image for \"{output}\": {e.ToString()}");
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
    [Modules.CommandInfo(new string[] {"gb", "gelbooru"}, "gelbooru", "Discord", "Gelbooru!")]
    public class Gelbooru : Modules.ModuleCommand
    {
        public override bool OnCall(List<string> args, MessageArgs originalMessage, string completeText)
        {
            ProcessFilter filter = ProcessFilter.NSFW;
            if(originalMessage.message.GetClient().GetType() == typeof(KotBot.DiscordBot.DiscordChatClient))
            {
                Socket​Text​Channel channel = ((KotBot.DiscordBot.DiscordChatClient)originalMessage.message.GetClient()).channel;
                if(!channel.IsNsfw)
                {
                    filter = ProcessFilter.SFW;
                }
            }
            string output = TagProcessor.ProcessTags(completeText, ProcessFilter.NSFW);
            try
            {
                GelbooruSender dSender = new GelbooruSender();
                if(!dSender.Setup(filter, output))
                {
                    originalMessage.message.Reply($"Failed to initialize gelbooru!");
                }
                dSender.Ready += (BooruSender sender) =>
                {
                    RestUserMessage message = dSender.Send(originalMessage);
                    if(message == null)
                        originalMessage.message.Reply($"Failed to find gelbooru image for \"{output}\"");
                };
            }
            catch(WebException e)
            {
                originalMessage.message.Reply($"Failed to find gelbooru image for \"{output}\"");
            }
            catch(Exception e)
            {
                originalMessage.message.Reply($"Failed to find gelbooru image for \"{output}\": {e.ToString()}");
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
