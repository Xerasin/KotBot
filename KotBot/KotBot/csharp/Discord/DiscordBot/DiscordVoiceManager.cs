/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KotBot.Scripting;
using System.Runtime.InteropServices;
using SteamKit2;
using SteamKit2.Unified.Internal;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Net;
using System.Timers;
using NAudio.Wave;
using NAudio.Vorbis;
using System.Diagnostics;
using System.Web;
using Discord.Net;
using Discord;
using DWebSocket = Discord.WebSocket;
using Discord.Audio;
namespace KotBot.DiscordBot
{
    using DAudio = Discord.Audio;
    public static class DiscordVoiceManager
    {
        public static Dictionary<ulong, IAudioClient> vClients = new Dictionary<ulong, IAudioClient>();
        public static Dictionary<ulong, AudioOutStream> createdStreams = new Dictionary<ulong, AudioOutStream>();
        public static Dictionary<ulong, CancellationTokenSource> audioThreads = new Dictionary<ulong, CancellationTokenSource>();
        [RegisterLuaFunction("Discord.JoinVoiceChannel")]
        public async static void JoinVoiceChannel(DWebSocket.SocketVoiceChannel channel)
        {
            IAudioClient client = await channel.ConnectAsync();
            vClients[channel.Guild.Id] = client;
        }

        [RegisterLuaFunction("Discord.DisconnectVoiceChannel")]
        public async static void DisconnectVoiceChannel(DWebSocket.SocketGuild server)
        {
            var client = DiscordManager.GetClient();
            if (vClients.ContainsKey(server.Id))
            {
                await vClients[server.Id].StopAsync();
                vClients[server.Id].Dispose();
            }

            vClients.Remove(server.Id);
            audioThreads.Remove(server.Id);
            createdStreams.Remove(server.Id);
        }
        static System.Timers.Timer timer = new System.Timers.Timer(20);
        public static bool isSpeaking = false;
        [RegisterLuaFunction("Discord.PlayAudio")]
        public async static void PlayAudio(DWebSocket.SocketGuild server, string pathOrUrl)
        {
            if (!vClients.ContainsKey(server.Id))
            {
                return;
            }

            var _vClient = vClients[server.Id];
            if(createdStreams.ContainsKey(server.Id))
            {
                await createdStreams[server.Id].FlushAsync();
                await createdStreams[server.Id].ClearAsync(new CancellationToken { });
            }
            using (var ffmpeg = CreateStream(pathOrUrl))
            {
                if (ffmpeg != null)
                {
                    using (var output = ffmpeg.StandardOutput.BaseStream)
                    {

                        using (var stream = _vClient.CreatePCMStream(AudioApplication.Mixed))
                        {
                            try { await output.CopyToAsync(stream); }
                            catch (Exception)
                            {
                                stream.Dispose();
                                return;
                            }
                            try { await stream.FlushAsync(); }
                            catch (Exception)
                            {
                                stream.Dispose();
                                return;
                            }

                            createdStreams[server.Id] = stream;
                        }
                    }
                    
                }
            }
        }
        [RegisterLuaFunction("Discord.PlayTTS")]
        public static void PlayTTS(DWebSocket.SocketGuild server, string message)
        {
            var client = DiscordManager.GetClient();
            if (!vClients.ContainsKey(server.Id))
            {
                return;
            }
            Task audioDownloadThread = Task.Factory.StartNew(() =>
            {
                try
                {
                    string outputFile = "audiocache\\" + message.GetHashCode() + ".wav";
                    if (!Directory.Exists("audiocache"))
                    {
                        Directory.CreateDirectory("audiocache");
                    }
                    if (File.Exists(outputFile))
                    {
                        PlayAudio(server, outputFile);
                        return;
                    }
                    WebClient webClient = new WebClient();
                    webClient.Proxy = null;
                    webClient.DownloadFile("http://tts.xerasin.com/?text=" + HttpUtility.UrlEncode(message), outputFile);
                    if (File.Exists(outputFile))
                    {
                        PlayAudio(server, outputFile);
                        return;
                    }
                }
                catch (Exception)
                {

                }
            });
        }
        [RegisterLuaFunction("Youtube.GetVideo")]
        public static VideoLibrary.YouTubeVideo GetVideoInfo(string videoURL)
        {
            var youtube = VideoLibrary.YouTube.Default;
            var video = youtube.GetVideo(videoURL);
            return video;
        }

        [RegisterLuaFunction("Discord.PlayVideo")]
        public static VideoLibrary.YouTubeVideo PlayVideo(DWebSocket.SocketGuild server, string videoURL)
        {
            try
            {
                if (!vClients.ContainsKey(server.Id))
                {
                    return null;
                }
                var youtube = VideoLibrary.YouTube.Default;
                List<VideoLibrary.YouTubeVideo> videos = new List<VideoLibrary.YouTubeVideo>(youtube.GetAllVideos(videoURL));
                VideoLibrary.YouTubeVideo video = null;
                foreach (VideoLibrary.YouTubeVideo loopVideo in videos)
                {
                    if (loopVideo.AdaptiveKind == VideoLibrary.AdaptiveKind.Audio)
                    {
                        video = loopVideo;
                    }
                }
                if (video == null)
                {
                    int lowest = int.MaxValue;
                    foreach (VideoLibrary.YouTubeVideo loopVideo in videos)
                    {
                        if (loopVideo.Resolution < lowest)
                        {
                            video = loopVideo;
                            lowest = loopVideo.Resolution;
                        }
                    }
                }
                if (video == null)
                {
                    return null;
                }
                Task videoDownloadThread = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        string outputFile = "videocache\\" + videoURL.GetHashCode() + ".mp3";

                        if (!Directory.Exists("videocache"))
                        {
                            Directory.CreateDirectory("videocache");
                        }
                        if (!Directory.Exists("tmp"))
                        {
                            Directory.CreateDirectory("tmp");
                        }
                        if (File.Exists(outputFile))
                        {
                            PlayAudio(server, outputFile);
                            return;
                        }

                        string fileName = "tmp\\" + videoURL.GetHashCode() + video.FileExtension;

                        if (!File.Exists(fileName))
                        {
                            File.WriteAllBytes(fileName, video.GetBytes());
                        }

                        ProcessStartInfo start = new ProcessStartInfo();
                        // Enter in the command line arguments, everything you would enter after the executable name itself
                        start.Arguments = "-i \"" + Directory.GetCurrentDirectory() + "\\" + fileName + "\" -ar 44100 -ab 320k -ac 2 \"" + Directory.GetCurrentDirectory() + "\\" + outputFile + "\"";
                        // Enter the executable to run, including the complete path
                        start.FileName = Directory.GetCurrentDirectory() + "\\" + "ffmpeg\\ffmpeg.exe";
                        // Do you want to show a console window?
                        start.WindowStyle = ProcessWindowStyle.Hidden;
                        start.CreateNoWindow = true;
                        int exitCode;
                        using (Process proc = Process.Start(start))
                        {
                            proc.WaitForExit();

                            // Retrieve the app's exit code
                            exitCode = proc.ExitCode;
                        }
                        if (File.Exists(outputFile))
                        {
                            if (File.Exists(fileName))
                            {
                                File.Delete(fileName);
                            }
                            PlayAudio(server, outputFile);
                            return;
                        }
                    }
                    catch (Exception)
                    {

                    }
                });
                return video;
            }
            catch (Exception)
            {

            }
            return null;
        }
        [RegisterLuaFunction("Discord.PlayAudioURL")]
        public static void PlayAudioURL(DWebSocket.SocketGuild server, string audioURL)
        {
            if (!vClients.ContainsKey(server.Id))
            {
                return;
            }
            Task audioDownloadThread = Task.Factory.StartNew(() =>
            {
                try
                {
                    string extenstion = Path.GetExtension(audioURL);
                    string outputFile = "audiocache\\" + audioURL.GetHashCode() + extenstion;
                    if (!Directory.Exists("audiocache"))
                    {
                        Directory.CreateDirectory("audiocache");
                    }
                    if (File.Exists(outputFile))
                    {
                        PlayAudio(server, outputFile);
                        return;
                    }
                    WebClient client = new WebClient();
                    client.Proxy = null;
                    client.DownloadFile(audioURL, outputFile);
                    if (File.Exists(outputFile))
                    {
                        PlayAudio(server, outputFile);
                        return;
                    }
                }
                catch (Exception)
                {

                }
            });
        }

        [RegisterLuaFunction("Discord.StopAudio")]
        public static void StopAudio(DWebSocket.SocketGuild server)
        {
            if ( vClients.ContainsKey(server.Id))
            {
                IAudioClient cclient = vClients[server.Id];
                if(audioThreads.ContainsKey(server.Id))
                {
                    CancellationTokenSource audioThread = audioThreads[server.Id];
                    if (audioThread != null)
                    {
                        audioThread.Cancel();
                        audioThread.Dispose();
                    }
                    audioThreads.Remove(server.Id);
                }
            } 
        }

        private static Process CreateStream(string path)
        {

            if(File.Exists(Directory.GetCurrentDirectory() + "\\" + "ffmpeg\\ffmpeg.exe"))
            {
                string arguments = $"-hide_banner -loglevel panic -i \"{Directory.GetCurrentDirectory() + "\\" + path.Replace("/", "\\")}\" -ac 2 -f s16le -ar 48000 pipe:1";
                Process process = Process.Start(new ProcessStartInfo
                {
                    FileName = Directory.GetCurrentDirectory() + "\\" + "ffmpeg\\ffmpeg.exe",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = false,
                    CreateNoWindow = true
                });

                return process;
            }
            return null;
        }
    }
}
*/