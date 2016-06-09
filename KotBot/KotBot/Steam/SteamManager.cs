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


namespace KotBot.Steam
{
    public static class SteamManager
    {
        static SteamClient steamClient;
        static CallbackManager manager;

        public static SteamUser steamUser;

        static SteamUnifiedMessages steamUnifiedMessages;
        static SteamUnifiedMessages.UnifiedService<IPlayer> playerService;
        public static SteamFriends steamFriends;

        static bool isRunning;

        static string user, pass;
        static string authCode, twoFactorAuth;

        static JobID badgeRequest = JobID.Invalid;
        public static int pipe = 0;
        [RegisterLuaFunction("Steam.Start")]
        public static SteamClient Start(string user, string pass, string auth)
        {
            SteamManager.user = user;
            SteamManager.pass = pass;
            SteamManager.authCode = auth;
            // create our steamclient instance
            steamClient = new SteamClient();
            // create the callback manager which will route callbacks to function calls
            manager = new CallbackManager(steamClient);

            // get the steamuser handler, which is used for logging on after successfully connecting
            steamUser = steamClient.GetHandler<SteamUser>();

            // get the steam friends handler, which is used for interacting with friends on the network after logging on
            steamFriends = steamClient.GetHandler<SteamFriends>();

            // get the steam unified messages handler, which is used for sending and receiving responses from the unified service api
            steamUnifiedMessages = steamClient.GetHandler<SteamUnifiedMessages>();

            // we also want to create our local service interface, which will help us build requests to the unified api
            playerService = steamUnifiedMessages.CreateService<IPlayer>();

            manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);
            
            manager.Subscribe<SteamUser.AccountInfoCallback>(OnAccountInfo);
            manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            manager.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth);
            manager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);
            isRunning = true;

            Log.Print("Connecting to Steam...");

            // initiate the connection
            steamClient.Connect();

            // create our callback handling loop
            while (isRunning)
            {
                // in order for the callbacks to get routed, they need to be handled by the manager
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }
            return steamClient;
        }
        static void OnConnected(SteamClient.ConnectedCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                Console.WriteLine("Unable to connect to Steam: {0}", callback.Result);

                isRunning = false;
                return;
            }

            Console.WriteLine("Connected to Steam! Logging in '{0}'...", user);

            byte[] sentryHash = null;
            if (File.Exists("sentry.bin"))
            {
                // if we have a saved sentry file, read and sha-1 hash it
                byte[] sentryFile = File.ReadAllBytes("sentry.bin");
                sentryHash = CryptoHelper.SHAHash(sentryFile);
            }

            steamUser.LogOn(new SteamUser.LogOnDetails
            {
                Username = user,
                Password = pass,

                // in this sample, we pass in an additional authcode
                // this value will be null (which is the default) for our first logon attempt
                AuthCode = authCode,

                // if the account is using 2-factor auth, we'll provide the two factor code instead
                // this will also be null on our first logon attempt
                TwoFactorCode = twoFactorAuth,

                // our subsequent logons use the hash of the sentry file as proof of ownership of the file
                // this will also be null for our first (no authcode) and second (authcode only) logon attempts
                SentryFileHash = sentryHash,
            });
        }


        static void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                if (callback.Result == EResult.AccountLogonDenied)
                {
                    // if we recieve AccountLogonDenied or one of it's flavors (AccountLogonDeniedNoMailSent, etc)
                    // then the account we're logging into is SteamGuard protected
                    // see sample 5 for how SteamGuard can be handled

                    Log.Print("Unable to logon to Steam: This account is SteamGuard protected.");

                    isRunning = false;
                    return;
                }

                Log.Print("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult);

                isRunning = false;
                return;
            }

            Log.Print("Successfully logged on!");

            // now that we're logged onto Steam, lets query the IPlayer service for our badge levels

            // first, build our request object, these are autogenerated and can normally be found in the SteamKit2.Unified.Internal namespace
            CPlayer_GetGameBadgeLevels_Request req = new CPlayer_GetGameBadgeLevels_Request
            {
                // we want to know our 440 (TF2) badge level
                appid = 440,
            };

            // now lets send the request, this is done by building an expression tree with the IPlayer interface
            badgeRequest = playerService.SendMessage(x => x.GetGameBadgeLevels(req));

            // alternatively, the request can be made using SteamUnifiedMessages directly, but then you must build the service request name manually
            // the name format is in the form of <Service>.<Method>#<Version>
            steamUnifiedMessages.SendMessage("Player.GetGameBadgeLevels#1", req);
        }

        static void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback)
        {
            Console.WriteLine("Updating sentryfile...");

            // write out our sentry file
            // ideally we'd want to write to the filename specified in the callback
            // but then this sample would require more code to find the correct sentry file to read during logon
            // for the sake of simplicity, we'll just use "sentry.bin"

            int fileSize;
            byte[] sentryHash;
            using (var fs = File.Open("sentry.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                fs.Seek(callback.Offset, SeekOrigin.Begin);
                fs.Write(callback.Data, 0, callback.BytesToWrite);
                fileSize = (int)fs.Length;

                fs.Seek(0, SeekOrigin.Begin);
                using (var sha = new SHA1CryptoServiceProvider())
                {
                    sentryHash = sha.ComputeHash(fs);
                }
            }

            // inform the steam servers that we're accepting this sentry file
            steamUser.SendMachineAuthResponse(new SteamUser.MachineAuthDetails
            {
                JobID = callback.JobID,

                FileName = callback.FileName,

                BytesWritten = callback.BytesToWrite,
                FileSize = fileSize,
                Offset = callback.Offset,

                Result = EResult.OK,
                LastError = 0,

                OneTimePassword = callback.OneTimePassword,

                SentryFileHash = sentryHash,
            });

            Console.WriteLine("Done!");
        }

        static void OnAccountInfo(SteamUser.AccountInfoCallback callback)
        {
            // before being able to interact with friends, you must wait for the account info callback
            // this callback is posted shortly after a successful logon

            // at this point, we can go online on friends, so lets do that
            steamFriends.SetPersonaState(EPersonaState.Online);

            manager.Subscribe<SteamFriends.FriendMsgCallback>(OnChat);
            manager.Subscribe<SteamFriends.ChatMsgCallback>(OnChatMsg);
            manager.Subscribe<SteamFriends.ChatInviteCallback>(OnChatInvite);

        }
        static void OnChatInvite(SteamFriends.ChatInviteCallback callback)
        {
            steamFriends.JoinChat(callback.ChatRoomID);
        }
        static void OnChatMsg(SteamFriends.ChatMsgCallback callback)
        {
            if (callback.ChatMsgType == EChatEntryType.ChatMsg)
            {
                Scripting.LuaHook.Call("MessageRecieved", new Message(new Steam.SteamChatClient(callback.ChatRoomID, steamUser.SteamID), new Steam.KSteamUser(callback.ChatterID), callback.Message));
            }
        }
        static void OnChat(SteamFriends.FriendMsgCallback callback)
        {
            if (callback.EntryType == EChatEntryType.ChatMsg)
            {
                Scripting.LuaHook.Call("MessageRecieved", new Message(new Steam.SteamPMClient(callback.Sender, steamUser.SteamID), new Steam.KSteamUser(callback.Sender), callback.Message));
            }

        }
        static void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            Console.WriteLine("Disconnected from Steam");

            isRunning = false;
        }

        static void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            Console.WriteLine("Logged off of Steam: {0}", callback.Result);
        }
        [RegisterLuaFunction("Steam.AddFriend")]
        public static void AddFriend(string id)
        {
            steamFriends.AddFriend(new SteamID(id));
        }

        [RegisterLuaFunction("Steam.GetSteamFriends")]
        public static SteamFriends GetSteamFriends()
        {
            return SteamManager.steamFriends;
        }

        [RegisterLuaFunction("Steam.GetSteamUser")]
        public static SteamUser GetSteamUser()
        {
            return SteamManager.steamUser;
        }


        [RegisterLuaFunction("Steam.GetCallbackManager")]
        public static CallbackManager GetCallbackManager()
        {
            return SteamManager.manager;
        }

        [RegisterLuaFunction("Steam.GetSteamClient")]
        public static SteamClient GetSteamClient()
        {
            return SteamManager.steamClient;
        }
        [RegisterLuaFunction("Steam.SteamID")]
        public static SteamID GetSteamID(string id)
        {
            return new SteamID(id);
        }

        [RegisterLuaFunction("Steam.SteamUser")]
        public static KSteamUser GetSteamUser(string id)
        {
            return new KSteamUser(new SteamID(id));
        }
    }
}
