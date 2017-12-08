using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KotBot.Modules;
using System.Timers;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Discord;
using System.Drawing;

namespace KotBot.DiscordBot
{
    public delegate bool WordMatch(DiscordMessageArgs args);
    public class WordCatch
    {
        public const string keepMatching =  ".*";
        public List<WordCatch> next = new List<WordCatch>();
        public string Word {get; set;} = null;
        public WordMatch Message {get; set;} = null;
        private Dictionary<string, WordMatch> sentanceMatches = new Dictionary<string, WordMatch>();
        public WordCatch(string word)
        {
            this.Word = word;
        }
        private List<string> BuildRegexes(List<string[]> words)
        {
            List<string> newList = new List<string>();
            if(words.Count == 0) return newList;
            List<string> wordsInThis = new List<string>(words[0]);
            words.RemoveAt(0);
            foreach(string newWord in wordsInThis)
            {
                List<string> nextList = BuildRegexes(new List<string[]>(words));
                if(nextList.Count > 0)
                {
                    foreach(string otherCompounds in nextList)
                    {
                        newList.Add($"{newWord} *{otherCompounds}");
                    }
                }
                else
                {
                    newList.Add(newWord);
                }
                
            }
            return newList;
        }
        public void Add(WordMatch message, List<string[]> words)
        {
            List<string> regExs = BuildRegexes(words);
            foreach(string regEx in regExs)
            {
                sentanceMatches[$"^{regEx}$"] = message;
            }
        }
        public WordMatch Get(string sentance)
        {
            sentance = sentance.Trim();
            foreach(var regEx in sentanceMatches)
            {
                //Log.Debug($"\"{sentance}\" \"{regEx.Key}\"");
                Regex rgx = new Regex(regEx.Key, RegexOptions.IgnoreCase);
                MatchCollection matches = rgx.Matches(sentance);
                
                if (matches.Count != 0)
                {
                    return regEx.Value;
                }
            }
            return null;
        }
        /*public void Add(WordMatch message, List<string[]> words)
        {
            if(words.Count > 0)
            {
                string[] newWords = words[0];
                words.RemoveAt(0);
                for(int I=0; I < newWords.Length; I++)
                {
                    string word = newWords[I];
                    WordCatch newWordCatch = null;
                    foreach(var existingWord in next)
                    {
                        if(existingWord.Word == word)
                        {
                            newWordCatch = existingWord;
                            break;
                        }
                    }
                    if(newWordCatch == null)
                    {
                        newWordCatch = new WordCatch(word);
                        next.Add(newWordCatch);
                    }
                    newWordCatch.Add(message, new List<string[]>(words));
                }
            }
            else
            {
                this.Message = message;
            }
        }
        public WordMatch Get(string sentance)
        {
            List<string> words = sentance.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
            return Get(words);
        }
        public WordMatch Get(List<string> words)
        {
            try
            {
                
                string word = null;
                if(!String.IsNullOrWhiteSpace(Word))
                {   

                    if(Word == keepMatching)
                    {
                        foreach(var existingWord in next)
                        {
                            WordMatch wordCheck = existingWord.Get(new List<string>(words));
                            if(wordCheck != null)
                            {
                                return wordCheck;
                            }
                        }
                    }
                   
                    if(words.Count > 0)
                    {
                         word = words[0];
                         words.RemoveAt(0);
                    }
                    
                    if(Word != keepMatching)
                    {
                        if(String.IsNullOrWhiteSpace(word)) return null;
                        Regex rgx = new Regex(Word, RegexOptions.IgnoreCase);
                        MatchCollection matches = rgx.Matches(word);
                        
                        if (matches.Count == 0)
                        {
                            return null;
                        }
                    }
                }
                foreach(var existingWord in next)
                {
                    WordMatch wordCheck = existingWord.Get(new List<string>(words));
                    if(wordCheck != null)
                    {
                        return wordCheck;
                    }
                    else if(Word == keepMatching)
                    {
                        List<string> newList = new List<string>(words);
                        if(newList.Count > 0)
                        {
                            while(newList.Count > 0)
                            {
                                wordCheck = existingWord.Get(new List<string>(newList));
                                if(wordCheck != null)
                                {
                                    return wordCheck;
                                }
                                if(newList.Count > 0)
                                    newList.RemoveAt(0);
                            }
                        }
                    }
                }
                if(Word == keepMatching) return Message;
                if(words.Count == 0) return Message;
                return null;
            }
            catch(Exception wordFindFailed)
            {
                Log.Error(wordFindFailed.ToString());
            }
            return null;
        }*/
    }
    public static class MiscDiscord
    {
        private static WordCatch wordCatcher = new WordCatch(null);
        public static void AddWordPattern(WordMatch response, bool orderMatters, params string[][] inputMessage)
        {
            if(inputMessage.Length == 0)
            {
                return;
            }
            if (orderMatters)
            {
                List<string[]> parms = new List<string[]>(inputMessage);
                wordCatcher.Add(response, parms);
            }
        }

        public static Random random = new Random();
        private static JObject currentGame = null;
        private static Timer gameNameTimer;
        public static void SendMessageGeneric(DiscordMessageArgs args, string message, Embed embed = null)
        {
            if(args.Channel == null && args.DMUser != null)
            {
                Task<Discord.IDMChannel> channel = args.DMUser.GetOrCreateDMChannelAsync();
                channel.Wait();
                Task<Discord.IUserMessage> dMessage = channel.Result.SendMessageAsync(message, false, embed);
                dMessage.Wait();
            }
            else if(args.Channel != null)
            {
                Task<Discord.Rest.RestUserMessage> dMessage = args.Channel.SendMessageAsync(message, false, embed);
                dMessage.Wait();
            }
        }
        public static void Init()
        {
            AddWordPattern((DiscordMessageArgs args) => {
                SendMessageGeneric(args, "Hiya! You are looking cute today! hehe *giggles*");
                return true;
            }, true, new string[] {"hiya", "hi+", "hello", "heya*"});

            AddWordPattern((DiscordMessageArgs args) => {
                SendMessageGeneric(args, "Hiya! I'm Kotbot c: *hugs*");
                return true;
            }, true, new string[] {"hiya +", "hi+ +", "hello +", "heya* +"}, new string[] {"kotbot"});

            AddWordPattern((DiscordMessageArgs args) => {
                SendMessageGeneric(args, "Pong~~~~ hehe *giggles*");
                return true;
            }, true, new string[] {"ping"});

            AddWordPattern((DiscordMessageArgs args) => {
                if(currentGame == null) 
                {
                    SendMessageGeneric(args, $"Im not playing a game yet silly!");
                    return false;
                }
                try
                {
                    string strPlatforms = "";
                    if(currentGame["platforms"].GetType() == typeof(JArray))
                    {
                        JArray platforms = (JArray)currentGame["platforms"];
                        foreach(var platform in platforms)
                        {
                            JObject objPlatform = (JObject)platform;
                            strPlatforms = $"{strPlatforms}{objPlatform["name"].ToString()}, ";
                        }
                        if(strPlatforms.Length > 0)
                        {
                            strPlatforms = strPlatforms.Substring(0, strPlatforms.Length - 2);
                        }
                    }
                    
                    string coverImage = null;
                    if(currentGame["image"] != null)
                    {
                        JObject images = (JObject)currentGame["image"];
                        if(images["original_url"] != null) 
                        {
                            string imageURL = images["original_url"].ToString();
                            if(!imageURL.Contains("gblogo"))
                            {
                                coverImage = imageURL;
                            }
                        }
                    }   

                    var builder = new EmbedBuilder();

                    builder.AddField("Game Name", currentGame["name"] != null ? currentGame["name"].ToString() : "???", true);
                    builder.AddField("Platforms", strPlatforms, true);
                    if(coverImage != null)
                    {
                        builder.WithImageUrl(coverImage);
                    }
                    builder.WithColor(new Color(145, 236, 255));
                    SendMessageGeneric(args, "", builder.Build());
                }
                catch(Exception whatGameFail)
                {
                    SendMessageGeneric(args, $"Failure to Get Game {whatGameFail.Message}: \n {whatGameFail.StackTrace}");
                }
                return true;
            }, true, new string[] {WordCatch.keepMatching}, new string[] {"wa+t +", "vat+ +", "what+a+ +", "what+ +"}, new string[] {WordCatch.keepMatching}, new string[] {"pla+y"}, new string[] {WordCatch.keepMatching});

            DiscordManager.DiscordMessage += (DiscordMessageArgs args) =>
            {
                string messageContent = args.Message.Content.ToLower();
                bool found = false;
                foreach(var user in args.Message.MentionedUsers)
                {
                    if(user.Id == args.Client.CurrentUser.Id)
                    {
                        found = true;
                    }
                }
                if(!found) return true;
                string newMessage = args.Message.Content.Replace(args.Client.CurrentUser.Mention, "").Replace($"<@{args.Client.CurrentUser.Id}>", "");
                //Log.Warning(newMessage);
                //Log.Error(args.Client.CurrentUser.Mention);
                WordMatch matchFunction = wordCatcher.Get(newMessage);
                if(matchFunction != null)
                    return matchFunction(args);
                return true;
            };

            //gameNameTimer.Destroy();
            gameNameTimer = new Timer(10);
            
            gameNameTimer.Elapsed += new ElapsedEventHandler((object e, ElapsedEventArgs args2) => {
                try
                {  
                    gameNameTimer.Interval = 180 * 1000;
                    SetRandomGameName();
                }
                catch (Exception timerFailed)
                {
                    Log.Error(timerFailed.Message);
                }
            });
            gameNameTimer.Start();
        }
        
        public static void SetRandomGameName()
        {
            WebClient client = new WebClient();
            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            client.Proxy = null;
            try
            {
                client.DownloadStringCompleted += (object sender2, DownloadStringCompletedEventArgs e2) =>
                {
                    if(e2.Error != null)
                    {
                        return;
                    }
                    JToken data = JToken.Parse(e2.Result);
                    if (data.GetType() == typeof(JObject))
                    {
                        int results = (int)data["number_of_total_results"];
                        int gameResult = random.Next(0, results - 1);
                        WebClient client2 = new WebClient();
                        client2.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                        client2.Proxy = null;
                        client2.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs e3) =>
                        {
                            if(e3.Error != null)
                            {
                                return;
                            }
                            JToken data2 = JToken.Parse(e3.Result);
                            if (data2.GetType() == typeof(JObject))
                            {
                                JObject objectTbl = (JObject)data2;
                                if(objectTbl["results"] != null && objectTbl["results"].GetType() == typeof(JArray))
                                {
                                   
                                    JArray gameResults = (JArray)objectTbl["results"];
                                    if(gameResults.Count > 0)
                                    {
                                        JObject gameTable = (JObject)gameResults[0];
                                        KotBot.DiscordBot.DiscordManager.SetGameName(gameTable["name"].ToString());
                                        currentGame = gameTable;
                                    }
                                }

                            }
                        };
                        client2.DownloadStringAsync(new Uri($"http://www.giantbomb.com/api/games/?api_key=2613ba373fa9e74dd5d0706e4d5b8b9c3c1a4ab1&format=json&limit=1&offset={gameResult}"));
                    }
                };
                client.DownloadStringAsync(new Uri($"http://www.giantbomb.com/api/games/?api_key=2613ba373fa9e74dd5d0706e4d5b8b9c3c1a4ab1&format=json&limit=1"));
            }
            catch(WebException)
            {
                Log.Error($"Gamename changed failed");
            }
        }
    }
}
