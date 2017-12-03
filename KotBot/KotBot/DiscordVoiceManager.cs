using System;
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
using Discord.Net;
using System.Timers;
using NAudio.Wave;
using NAudio.Vorbis;
using System.Diagnostics;
using System.Web;
using Discord.Audio;
namespace KotBot.DiscordBot
{
    public static class DiscordVoiceManager
    {
        public static AudioService service;
        public static Dictionary<ulong, IAudioClient> vClients = new Dictionary<ulong, IAudioClient>();
        public static Dictionary<ulong, CancellationTokenSource> audioThreads = new Dictionary<ulong, CancellationTokenSource>();
        [RegisterLuaFunction("Discord.JoinVoiceChannel")]
        public async static void JoinVoiceChannel(Discord.Channel channel)
        {
            var client = DiscordManager.GetClient();
            if (service == null)
            {
                service = client.GetService<AudioService>();
            }
            var _vClient = await service.Join(channel);
            vClients[channel.Server.Id] = _vClient;
        }

        [RegisterLuaFunction("Discord.DisconnectVoiceChannel")]
        public static void DisconnectVoiceChannel(Discord.Server server)
        {
            if (service == null)
            {
                return;
            }
            var client = DiscordManager.GetClient();
            service.Leave(server);
            vClients.Remove(server.Id);
            audioThreads.Remove(server.Id);
        }
        static System.Timers.Timer timer = new System.Timers.Timer(20);
        public static bool isSpeaking = false;
        [RegisterLuaFunction("Discord.PlayAudio")]
        public static void PlayAudio(Discord.Server server, string pathOrUrl)
        {
            if (service == null || !vClients.ContainsKey(server.Id))
            {
                return;
            }
            var tokenSource2 = new CancellationTokenSource();
            CancellationToken ct = tokenSource2.Token;

            var audioThread = Task.Factory.StartNew(() =>
            {
                var _vClient = vClients[server.Id];

                if (pathOrUrl.EndsWith(".wav"))
                {
                    var channelCount = service.Config.Channels; // Get the number of AudioChannels our AudioService has been configured to use.
                    var OutFormat = new WaveFormat(48000, 16, channelCount); // Create a new Output Format, using the spec that Discord will accept, and with the number of channels that our client supports.
                    using (var waveReader = new WaveFileReader(File.Open(pathOrUrl, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        using (var resampler = new MediaFoundationResampler(waveReader, OutFormat)) // Create a Disposable Resampler, which will convert the read MP3 data to PCM, using our Output Format
                        {
                            resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
                            int blockSize = OutFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
                            byte[] buffer = new byte[blockSize];
                            int byteCount;

                            while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0) // Read audio into our buffer, and keep a loop open while data is present
                            {
                                if (ct.IsCancellationRequested)
                                {
                                    return;
                                }
                                if (byteCount < blockSize)
                                {
                                    // Incomplete Frame
                                    for (int i = byteCount; i < blockSize; i++)
                                        buffer[i] = 0;
                                }
                                _vClient.Send(buffer, 0, blockSize); // Send the buffer to Discord
                            }
                            waveReader.Close();
                            resampler.Dispose();
                        }
                    }
                }
                else if (pathOrUrl.EndsWith(".mp3"))
                {
                    var channelCount = service.Config.Channels; // Get the number of AudioChannels our AudioService has been configured to use.
                    var OutFormat = new WaveFormat(48000, 16, channelCount); // Create a new Output Format, using the spec that Discord will accept, and with the number of channels that our client supports.
                    using (var mp3Reader = new Mp3FileReader(File.Open(pathOrUrl, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        using (var resampler = new MediaFoundationResampler(mp3Reader, OutFormat)) // Create a Disposable Resampler, which will convert the read MP3 data to PCM, using our Output Format
                        {
                            resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
                            int blockSize = OutFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
                            byte[] buffer = new byte[blockSize];
                            int byteCount;

                            while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0) // Read audio into our buffer, and keep a loop open while data is present
                            {
                                if (ct.IsCancellationRequested)
                                {
                                    return;
                                }
                                if (byteCount < blockSize)
                                {
                                    // Incomplete Frame
                                    for (int i = byteCount; i < blockSize; i++)
                                        buffer[i] = 0;
                                }
                                _vClient.Send(buffer, 0, blockSize); // Send the buffer to Discord
                            }
                            mp3Reader.Close();
                            resampler.Dispose();
                        }
                    }
                }
                else if (pathOrUrl.EndsWith(".ogg"))
                {
                    var channelCount = service.Config.Channels; // Get the number of AudioChannels our AudioService has been configured to use.
                    var OutFormat = new WaveFormat(48000, 16, channelCount); // Create a new Output Format, using the spec that Discord will accept, and with the number of channels that our client supports.
                    using (var mp3Reader = new VorbisWaveReader(File.Open(pathOrUrl, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        using (var resampler = new MediaFoundationResampler(mp3Reader, OutFormat)) // Create a Disposable Resampler, which will convert the read MP3 data to PCM, using our Output Format
                        {
                            resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
                            int blockSize = OutFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
                            byte[] buffer = new byte[blockSize];
                            int byteCount;

                            while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0) // Read audio into our buffer, and keep a loop open while data is present
                            {
                                if (ct.IsCancellationRequested)
                                {
                                    return;
                                }
                                if (byteCount < blockSize)
                                {
                                    // Incomplete Frame
                                    for (int i = byteCount; i < blockSize; i++)
                                        buffer[i] = 0;
                                }
                                _vClient.Send(buffer, 0, blockSize); // Send the buffer to Discord
                            }
                            mp3Reader.Close();
                            resampler.Dispose();
                        }
                    }
                }
            }, tokenSource2.Token);
            audioThreads[server.Id] = tokenSource2;
        }
        [RegisterLuaFunction("Discord.PlayTTS")]
        public static void PlayTTS(Discord.Server server, string message)
        {
            var client = DiscordManager.GetClient();
            if (service == null || !vClients.ContainsKey(server.Id))
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
        public static VideoLibrary.YouTubeVideo PlayVideo(Discord.Server server, string videoURL)
        {
            try
            {
                if (service == null || !vClients.ContainsKey(server.Id))
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
        public static void PlayAudioURL(Discord.Server server, string audioURL)
        {
            if (service == null || !vClients.ContainsKey(server.Id))
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
        public static void StopAudio(Discord.Server server)
        {
            if (service != null && vClients.ContainsKey(server.Id))
            {
                IAudioClient cclient = vClients[server.Id];
                cclient.Clear();
                cclient.Wait();
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
    }
}
