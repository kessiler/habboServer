using Database_Manager.Database.Session_Details.Interfaces;
using HabboEncryption;
using HabboEncryption.Keys;
using Cyber.Core;
using Cyber.Database;
using Cyber.HabboHotel;
using Cyber.HabboHotel.Catalogs;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Groups;
using Cyber.HabboHotel.Pets;
using Cyber.HabboHotel.Users;
using Cyber.HabboHotel.Users.Messenger;
using Cyber.HabboHotel.Users.UserDataManagement;
using Cyber.Messages;
using Cyber.Messages.Headers;
using Cyber.Messages.StaticMessageHandlers;
using Cyber.Net;
using Cyber.Util;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Cyber
{
    internal static class CyberEnvironment
    {

        internal static readonly string PrettyBuild = "1.0.1";
        internal static readonly string PrettyVersion = "Cyber Emulator";
        internal static readonly string PrettyRelease = "RELEASE63-201408141029-609065162";
        internal static bool isLive;
        internal static bool SeparatedTasksInGameClientManager = false;
        internal static bool SeparatedTasksInMainLoops = false;
        internal static ConfigData ConfigData;
        internal static CultureInfo cultureInfo;
        internal static DateTime ServerStarted;
        internal static Dictionary<uint, List<OfflineMessage>> OfflineMessages;
        internal static GiftWrappers GiftWrappers;
        internal static int LiveCurrencyType = 105;
        internal static MusSocket MusSystem;
        private static ConfigurationData Configuration;
        private static ConnectionHandling ConnectionManager;
        private static DatabaseManager manager;
        private static Encoding DefaultEncoding;
        private static Game Game;
        public static uint FriendRequestLimit = 1000;

        private static readonly HashSet<char> allowedchars = new HashSet<char>(new char[]
		{
			'a',
			'b',
			'c',
			'd',
			'e',
			'f',
			'g',
			'h',
			'i',
			'j',
			'k',
			'l',
			'm',
			'n',
			'o',
			'p',
			'q',
			'r',
			's',
			't',
			'u',
			'v',
			'w',
			'x',
			'y',
			'z',
			'1',
			'2',
			'3',
			'4',
			'5',
			'6',
			'7',
			'8',
			'9',
			'0',
			'-',
			'.',
			'á',
			'é',
			'í',
			'ó',
			'ú',
			'ñ',
			'Ñ',
			'ü',
			'Ü',
			'Á',
			'É',
			'Í',
			'Ó',
			'Ú',
			' ', 'Ã', '©', '¡', '­', 'º', '³','Ã','‰','_'
		});

        private static HybridDictionary usersCached = new HybridDictionary();
        private static bool ShutdownInitiated = false;
        internal static uint StaffAlertMinRank = 4;

        internal static bool ShutdownStarted
        {
            get
            {
                return CyberEnvironment.ShutdownInitiated;
            }
        }
        internal static void Initialize()
        {
            Console.Clear();
            CyberEnvironment.ServerStarted = DateTime.Now;
            Console.SetWindowSize(120, 40);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.WriteLine(@"                              ____ _   _ ___  ____ ____    ____ _  _ _  _ _    ____ ___ ____ ____ ");
            Console.WriteLine(@"                              |     \_/  |__] |___ |__/    |___ |\/| |  | |    |__|  |  |  | |__/ ");
            Console.WriteLine(@"                              |___   |   |__] |___ |  \    |___ |  | |__| |___ |  |  |  |__| |  \ ");
            Console.WriteLine();
            Console.WriteLine("                                                Cyber Emulator - Version: " + PrettyBuild);
            Console.WriteLine("                                         based on Plus, developed by Kessiler Rodrigues.");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("                                                 " + PrettyRelease);
            Console.Title = "Cyber Emulator | Loading data [...]";
            CyberEnvironment.DefaultEncoding = Encoding.Default;
            Console.WriteLine("");

            CyberEnvironment.cultureInfo = CultureInfo.CreateSpecificCulture("en-GB");
            TextHandling.replaceDecimal();
            try
            {
                CyberEnvironment.Configuration = new ConfigurationData(Path.Combine(Application.StartupPath, "config.ini"), false);


                MySqlConnectionStringBuilder mySqlConnectionStringBuilder = new MySqlConnectionStringBuilder();
                mySqlConnectionStringBuilder.Server = (CyberEnvironment.GetConfig().data["db.hostname"]);
                mySqlConnectionStringBuilder.Port = (uint.Parse(CyberEnvironment.GetConfig().data["db.port"]));
                mySqlConnectionStringBuilder.UserID = (CyberEnvironment.GetConfig().data["db.username"]);
                mySqlConnectionStringBuilder.Password = (CyberEnvironment.GetConfig().data["db.password"]);
                mySqlConnectionStringBuilder.Database = (CyberEnvironment.GetConfig().data["db.name"]);
                mySqlConnectionStringBuilder.MinimumPoolSize = (uint.Parse(CyberEnvironment.GetConfig().data["db.pool.minsize"]));
                mySqlConnectionStringBuilder.MaximumPoolSize = (uint.Parse(CyberEnvironment.GetConfig().data["db.pool.maxsize"]));
                mySqlConnectionStringBuilder.Pooling = (true);
                mySqlConnectionStringBuilder.AllowZeroDateTime = (true);
                mySqlConnectionStringBuilder.ConvertZeroDateTime = (true);
                mySqlConnectionStringBuilder.DefaultCommandTimeout = (300u);
                mySqlConnectionStringBuilder.ConnectionTimeout = (10u);
                CyberEnvironment.manager = new DatabaseManager(mySqlConnectionStringBuilder.ToString());

                using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                {
                    ConfigData = new ConfigData(queryreactor);
                    PetLocale.Init(queryreactor);
                    OfflineMessages = new Dictionary<uint, List<OfflineMessage>>();
                    OfflineMessage.InitOfflineMessages(queryreactor);
                    GiftWrappers = new GiftWrappers(queryreactor);
                }

                FriendRequestLimit = (uint)int.Parse(CyberEnvironment.GetConfig().data["client.maxrequests"]);
                if (ExtraSettings.RunExtraSettings())
                {
                    Logging.WriteLine("Loaded an extra settings file.");
                }

                Game = new Game(int.Parse(GetConfig().data["game.tcp.conlimit"]));
                Game.start();

                ConnectionManager = new ConnectionHandling(int.Parse(GetConfig().data["game.tcp.port"]), int.Parse(GetConfig().data["game.tcp.conlimit"]), int.Parse(GetConfig().data["game.tcp.conperip"]), GetConfig().data["game.tcp.enablenagles"].ToLower() == "true");

                HabboCrypto.Initialize(new RsaKeyHolder());
                CyberEnvironment.ConnectionManager.init();
                CyberEnvironment.ConnectionManager.Start();
                StaticClientMessageHandler.Initialize();

                string[] allowedIps = GetConfig().data["mus.tcp.allowedaddr"].Split(';');
                MusSystem = new MusSocket(GetConfig().data["mus.tcp.bindip"], int.Parse(GetConfig().data["mus.tcp.port"]), allowedIps, 0);


                if (Configuration.data.ContainsKey("StaffAlert.MinRank"))
                {
                    StaffAlertMinRank = uint.Parse(CyberEnvironment.GetConfig().data["StaffAlert.MinRank"]);
                }
                if (Configuration.data.ContainsKey("SeparatedTasksInMainLoops.enabled") && Configuration.data["SeparatedTasksInMainLoops.enabled"] == "true")
                {
                    SeparatedTasksInMainLoops = true;
                }
                if (Configuration.data.ContainsKey("SeparatedTasksInGameClientManager.enabled") && Configuration.data["SeparatedTasksInGameClientManager.enabled"] == "true")
                {
                    SeparatedTasksInGameClientManager = true;
                }
                Logging.WriteLine("Game was succesfully loaded.");
                isLive = true;
            }
            catch (KeyNotFoundException ex)
            {
                Logging.WriteLine("Something is missing in your configuration", ConsoleColor.Red);
                Logging.WriteLine(ex.ToString(), ConsoleColor.Yellow);
                Logging.WriteLine("Please type a key to shut down ...", ConsoleColor.Gray);
                Console.ReadKey(true);
                CyberEnvironment.Destroy();
            }
            catch (InvalidOperationException ex1)
            {
                Logging.WriteLine("Something wrong happened: " + ex1.Message, ConsoleColor.Red);
                Logging.WriteLine(ex1.ToString(), ConsoleColor.Yellow);
                Logging.WriteLine("Please type a key to shut down...", ConsoleColor.Gray);
                Console.ReadKey(true);
                CyberEnvironment.Destroy();
            }
            catch (Exception ex2)
            {
                Logging.WriteLine("An exception got caught: " + ex2.Message, ConsoleColor.Red);
                Logging.WriteLine("Type a key to know more about the error", ConsoleColor.Gray);
                Console.ReadKey();
                Logging.WriteLine(ex2.ToString(), ConsoleColor.Yellow);
                Logging.WriteLine("Please type a ket to shut down...", ConsoleColor.Gray);
                Console.ReadKey();
                Environment.Exit(1);
            }
        }
        internal static bool EnumToBool(string Enum)
        {
            return Enum == "1";
        }

        internal static int BoolToInteger(bool Bool)
        {
            if (Bool)
            {
                return 1;
            }
            return 0;
        }
        internal static string BoolToEnum(bool Bool)
        {
            if (Bool)
            {
                return "1";
            }
            return "0";
        }
        internal static int GetRandomNumber(int Min, int Max)
        {
            return RandomNumber.GenerateNewRandom(Min, Max);
        }
        internal static int GetUnixTimestamp()
        {
            double totalSeconds = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
            return checked((int)totalSeconds);
        }
        internal static DateTime UnixToDateTime(double unixTimeStamp)
        {
            DateTime result = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            result = result.AddSeconds(unixTimeStamp).ToLocalTime();
            return result;
        }
        internal static int DateTimeToUnix(DateTime target)
        {
            DateTime d = new DateTime(1970, 1, 1, 0, 0, 0, target.Kind);
            return Convert.ToInt32((target - d).TotalSeconds);
        }
        internal static long Now()
        {
            double totalMilliseconds = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
            return checked((long)totalMilliseconds);
        }
        internal static string FilterFigure(string figure)
        {
            for (int i = 0; i < figure.Length; i++)
            {
                char character = figure[i];
                if (!CyberEnvironment.isValid(character))
                {
                    return "lg-3023-1335.hr-828-45.sh-295-1332.hd-180-4.ea-3168-89.ca-1813-62.ch-235-1332";
                }
            }
            return figure;
        }
        private static bool isValid(char character)
        {
            return CyberEnvironment.allowedchars.Contains(character);
        }
        internal static bool IsValidAlphaNumeric(string inputStr)
        {
            inputStr = inputStr.ToLower();
            if (string.IsNullOrEmpty(inputStr))
            {
                return false;
            }
            checked
            {
                for (int i = 0; i < inputStr.Length; i++)
                {
                    if (!CyberEnvironment.isValid(inputStr[i]))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        internal static Habbo getHabboForId(uint UserId)
        {
            Habbo result;
            try
            {
                GameClient clientByUserID = CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(UserId);
                if (clientByUserID != null)
                {
                    Habbo habbo = clientByUserID.GetHabbo();
                    if (habbo != null && habbo.Id > 0u)
                    {
                        if (CyberEnvironment.usersCached.Contains(UserId))
                        {
                            CyberEnvironment.usersCached.Remove(UserId);
                        }
                        result = habbo;
                        return result;
                    }
                }
                else
                {
                    if (CyberEnvironment.usersCached.Contains(UserId))
                    {
                        result = (Habbo)CyberEnvironment.usersCached[UserId];
                        return result;
                    }
                    UserData userData = UserDataFactory.GetUserData(checked((int)UserId));
                    Habbo user = userData.user;
                    if (user != null)
                    {
                        user.InitInformation(userData);
                        CyberEnvironment.usersCached.Add(UserId, user);
                        result = user;
                        return result;
                    }
                }
                result = null;
            }
            catch
            {
                result = null;
            }
            return result;
        }
        internal static Habbo getHabboForName(string UserName)
        {
            Habbo result;
            try
            {
                using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                {
                    queryreactor.setQuery("SELECT id FROM users WHERE username = @user");
                    queryreactor.addParameter("user", UserName);
                    int integer = queryreactor.getInteger();
                    if (integer > 0)
                    {
                        result = CyberEnvironment.getHabboForId(checked((uint)integer));
                        return result;
                    }
                }
                result = null;
            }
            catch
            {
                result = null;
            }
            return result;
        }
        internal static bool IsNum(string Int)
        {
            double num;
            return double.TryParse(Int, out num);
        }
        internal static ConfigurationData GetConfig()
        {
            return CyberEnvironment.Configuration;
        }
        internal static ConfigData GetDBConfig()
        {
            return CyberEnvironment.ConfigData;
        }
        internal static Encoding GetDefaultEncoding()
        {
            return CyberEnvironment.DefaultEncoding;
        }
        internal static ConnectionHandling GetConnectionManager()
        {
            return CyberEnvironment.ConnectionManager;
        }
        internal static Game GetGame()
        {
            return CyberEnvironment.Game;
        }
        internal static void Destroy()
        {
            CyberEnvironment.isLive = false;
            Logging.WriteLine("Destroying CyberEnvironment...", ConsoleColor.Gray);
            if (CyberEnvironment.GetGame() != null)
            {
                CyberEnvironment.GetGame().Destroy();
                CyberEnvironment.GetGame().GetPixelManager().Destroy();
                CyberEnvironment.Game = null;
            }
            if (CyberEnvironment.GetConnectionManager() != null)
            {
                Logging.WriteLine("Destroying ConnectionManager...", ConsoleColor.Gray);
                CyberEnvironment.GetConnectionManager().Destroy();
            }
            if (CyberEnvironment.manager != null)
            {
                try
                {
                    Logging.WriteLine("Destroying DatabaseManager...", ConsoleColor.Gray);
                    CyberEnvironment.manager.Destroy();
                }
                catch
                {
                }
            }
            Logging.WriteLine("Closing...", ConsoleColor.Gray);
            Thread.Sleep(500);
            Environment.Exit(1);
        }
        internal static void SendMassMessage(string Message)
        {
            try
            {
                ServerMessage serverMessage = new ServerMessage(Outgoing.BroadcastNotifMessageComposer);
                serverMessage.AppendString(Message);
                CyberEnvironment.GetGame().GetClientManager().QueueBroadcaseMessage(serverMessage);
            }
            catch (Exception pException)
            {
                Logging.HandleException(pException, "CyberEnvironment.SendMassMessage");
            }
        }
        internal static string FilterInjectionChars(string Input)
        {
            Input = Input.Replace('\u0001', ' ');
            Input = Input.Replace('\u0002', ' ');
            Input = Input.Replace('\u0003', ' ');
            Input = Input.Replace('\t', ' ');
            return Input;
        }
        internal static DatabaseManager GetDatabaseManager()
        {
            return CyberEnvironment.manager;
        }
        internal static void PerformShutDown()
        {
            CyberEnvironment.PerformShutDown(false);
        }
        internal static void PerformShutDown(bool Restart)
        {
            DateTime now = DateTime.Now;
            CyberEnvironment.ShutdownInitiated = true;
            ServerMessage serverMessage = new ServerMessage(Outgoing.SuperNotificationMessageComposer);
            serverMessage.AppendString("disconnection");
            serverMessage.AppendInt32(2);
            serverMessage.AppendString("title");
            serverMessage.AppendString("HEY EVERYONE!");
            serverMessage.AppendString("message");
            if (Restart)
            {
                serverMessage.AppendString("<b>The hotel is shutting down for a break.</b>\nYou may come back later.\r\n<b>So long!</b>");
            }
            else
            {
                serverMessage.AppendString("<b>The hotel is shutting down for a break.</b><br />You may come back soon. Don't worry, everything's going to be saved..<br /><b>So long!</b>\r\n~ This session was powered by Cyber Emulator");
            }
            CyberEnvironment.GetGame().GetClientManager().QueueBroadcaseMessage(serverMessage);
            Thread.Sleep(6000);
            CyberEnvironment.Game.StopGameLoop();
            DateTime arg_93_0 = DateTime.Now;
            Logging.WriteLine("Shutting down...", ConsoleColor.Yellow);
            Console.Title = "Cyber Emulator | Shutting down...";
            DateTime arg_AF_0 = DateTime.Now;
            CyberEnvironment.GetGame().GetClientManager().CloseAll();
            DateTime arg_C4_0 = DateTime.Now;
            CyberEnvironment.Game.GetRoomManager().RemoveAllRooms();
            foreach (Guild Group in Game.GetGroupManager().Groups.Values)
            {
                Group.UpdateForum();
            }

            CyberEnvironment.GetConnectionManager().Destroy();
            DateTime arg_E3_0 = DateTime.Now;
            using (IQueryAdapter queryreactor = CyberEnvironment.manager.getQueryReactor())
            {
                queryreactor.runFastQuery("UPDATE users SET online = '0'");
                queryreactor.runFastQuery("UPDATE rooms SET users_now = 0");
                queryreactor.runFastQuery("TRUNCATE TABLE user_roomvisits");
            }
            DateTime arg_121_0 = DateTime.Now;
            CyberEnvironment.ConnectionManager.Destroy();
            DateTime arg_131_0 = DateTime.Now;
            CyberEnvironment.Game.Destroy();
            DateTime arg_141_0 = DateTime.Now;
            try
            {
                Console.WriteLine("Destroying database manager...");
                CyberEnvironment.manager.Destroy();
            }
            catch
            {
            }
            TimeSpan span = DateTime.Now - now;
            Console.WriteLine("Cyber Emulator took " + CyberEnvironment.TimeSpanToString(span) + " in shutdown process.");
            Console.WriteLine("Cyber Emulator has shut down succesfully.");
            CyberEnvironment.isLive = false;
            if (Restart)
            {
                Process.Start(AppDomain.CurrentDomain.BaseDirectory + "\\Cyber Emulator.exe");
            }
            Console.WriteLine("Closing...");
            Environment.Exit(0);
        }
        internal static string TimeSpanToString(TimeSpan span)
        {
            return string.Concat(new object[]
			{
				span.Seconds,
				" s, ",
				span.Milliseconds,
				" ms"
			});
        }

    }
}
