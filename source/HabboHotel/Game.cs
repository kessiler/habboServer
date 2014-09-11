using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.Core;
using Cyber.HabboHotel.Achievements;
using Cyber.HabboHotel.Catalogs;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Groups;
using Cyber.HabboHotel.Items;
using Cyber.HabboHotel.Misc;
using Cyber.HabboHotel.Navigators;
using Cyber.HabboHotel.Pets;
using Cyber.HabboHotel.Polls;
using Cyber.HabboHotel.Quests;
using Cyber.HabboHotel.Roles;
using Cyber.HabboHotel.RoomBots;
using Cyber.HabboHotel.Rooms;
using Cyber.HabboHotel.SoundMachine;
using Cyber.HabboHotel.Support;
using Cyber.HabboHotel.Users.Inventory;
using Cyber.HabboHotel.YouTube;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace Cyber.HabboHotel
{
	internal class Game
	{
		private GameClientManager ClientManager;
		private ModerationBanManager BanManager;
		private RoleManager RoleManager;
		private Catalog Catalog;
		private Navigator Navigator;
		private ItemManager ItemManager;
		private RoomManager RoomManager;
		private HotelView HotelView;
		private PixelManager PixelManager;
		private AchievementManager AchievementManager;
		private ModerationTool ModerationTool;
		private BotManager BotManager;
		private InventoryGlobal globalInventory;
		private QuestManager questManager;
		private GroupManager groupManager;
		private RoomEvents Events;
		private TalentManager talentManager;
		private VideoManager VideoManager;
		private PinataHandler PinataHandler;
		private PollManager PollManager;
        private AntiMutant AntiMutant;
		private Thread gameLoop;
		private bool gameLoopActive;
        internal bool ClientManagerCycle_ended;
		internal bool RoomManagerCycle_ended;
		internal static bool gameLoopEnabled = true;
		internal bool gameLoopEnabled_EXT
		{
			get
			{
				return Game.gameLoopEnabled;
			}
		}
		internal bool gameLoopActive_EXT
		{
			get
			{
				return this.gameLoopActive;
			}
		}
		internal int gameLoopSleepTime_EXT
		{
			get
			{
				return 25;
			}
		}
        internal AntiMutant GetAntiMutant()
        {
            return this.AntiMutant;
        }

		internal GameClientManager GetClientManager()
		{
			return this.ClientManager;
		}
		internal ModerationBanManager GetBanManager()
		{
			return this.BanManager;
		}
		internal RoleManager GetRoleManager()
		{
			return this.RoleManager;
		}
		internal Catalog GetCatalog()
		{
			return this.Catalog;
		}
		internal VideoManager GetVideoManager()
		{
			return this.VideoManager;
		}
		internal RoomEvents GetRoomEvents()
		{
			return this.Events;
		}
		internal Navigator GetNavigator()
		{
			return this.Navigator;
		}
		internal ItemManager GetItemManager()
		{
			return this.ItemManager;
		}
		internal RoomManager GetRoomManager()
		{
			return this.RoomManager;
		}
		internal PixelManager GetPixelManager()
		{
			return this.PixelManager;
		}
		internal HotelView GetHotelView()
		{
			return this.HotelView;
		}
		internal AchievementManager GetAchievementManager()
		{
			return this.AchievementManager;
		}
		internal ModerationTool GetModerationTool()
		{
			return this.ModerationTool;
		}
		internal BotManager GetBotManager()
		{
			return this.BotManager;
		}
		internal InventoryGlobal GetInventory()
		{
			return this.globalInventory;
		}
		internal QuestManager GetQuestManager()
		{
			return this.questManager;
		}
		internal GroupManager GetGroupManager()
		{
			return this.groupManager;
		}
		internal TalentManager GetTalentManager()
		{
			return this.talentManager;
		}
		internal PinataHandler GetPinataHandler()
		{
			return this.PinataHandler;
		}
		internal PollManager GetPollManager()
		{
			return this.PollManager;
		}
		internal Game(int conns)
		{
            Logging.WriteLine("Starting modules...");
			this.ClientManager = new GameClientManager();
			this.BanManager = new ModerationBanManager();
			this.RoleManager = new RoleManager();
			this.Navigator = new Navigator();
			this.ItemManager = new ItemManager();
			this.Catalog = new Catalog();
			this.RoomManager = new RoomManager();
			this.PixelManager = new PixelManager();
			this.HotelView = new HotelView();
			this.ModerationTool = new ModerationTool();
			this.BotManager = new BotManager();
			this.questManager = new QuestManager();
			this.Events = new RoomEvents();
			this.groupManager = new GroupManager();
			this.talentManager = new TalentManager();
			this.VideoManager = new VideoManager();
			this.PinataHandler = new PinataHandler();
			this.PollManager = new PollManager();
            this.AntiMutant = new AntiMutant();
		}
		internal void start()
		{
			using (IQueryAdapter queryReactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
                uint itemsLoaded = 0;
                uint catalogPageLoaded = 0;
                uint navigatorLoaded = 0;
                uint roomModelLoaded = 0;
                uint videoPlaylistLoaded = 0;
                uint achievementLoaded = 0;
                uint pollLoaded = 0;

				this.BanManager.LoadBans(queryReactor);
				this.RoleManager.LoadRights(queryReactor);
				this.ItemManager.LoadItems(queryReactor, out itemsLoaded);
                Logging.WriteLine("Loaded a total of " + itemsLoaded + " item definition(s).");
				this.PinataHandler.Initialize(queryReactor);
				this.globalInventory = new InventoryGlobal();
				this.VideoManager.Load(queryReactor, out videoPlaylistLoaded);
                Logging.WriteLine("Loaded a total of " + videoPlaylistLoaded + " video playlist(s).");
				this.Catalog.Initialize(queryReactor, out catalogPageLoaded);
                Logging.WriteLine("Loaded a total of " + catalogPageLoaded + " catalogue page(s).");
				this.Navigator.Initialize(queryReactor, out navigatorLoaded);
                Logging.WriteLine("Loaded a total of " + navigatorLoaded + " official room(s).");
                this.RoomManager.LoadModels(queryReactor, out roomModelLoaded);
                Logging.WriteLine("Loaded a total of " + roomModelLoaded + " room model(s).");
				this.RoomManager.InitVotedRooms(queryReactor);
				this.AchievementManager = new AchievementManager(queryReactor, out achievementLoaded);
                Logging.WriteLine("Loaded a total of " + achievementLoaded + " achievement(s).");
				this.questManager.Initialize(queryReactor);
				this.PollManager.Init(queryReactor, out pollLoaded);
                Logging.WriteLine("Loaded a total of " + pollLoaded + " poll(s).");
				this.talentManager.Initialize(queryReactor);
				this.ModerationTool.LoadMessagePresets(queryReactor);
				this.ModerationTool.LoadPendingTickets(queryReactor);
				PetRace.Init(queryReactor);
				AntiPublicistas.Load(queryReactor);
				this.GetGroupManager().InitGroups(queryReactor);
                LowPriorityWorker.Init(queryReactor);
				SongManager.Initialize();
			}
			this.StartGameLoop();
            PixelManager.StartPixelTimer();
		}
		internal void StartGameLoop()
		{
			this.gameLoopActive = true;
			this.gameLoop = new Thread(new ThreadStart(this.MainGameLoop));
			this.gameLoop.Start();
		}
		internal void StopGameLoop()
		{
			this.gameLoopActive = false;
			while (!this.RoomManagerCycle_ended || !this.ClientManagerCycle_ended)
			{
				Thread.Sleep(25);
			}
		}

		private void MainGameLoop()
		{
            LowPriorityWorker.StartProcessing();

			while (this.gameLoopActive)
			{
				if (Game.gameLoopEnabled)
				{
                    try
                    {
                        this.RoomManagerCycle_ended = false;
                        this.ClientManagerCycle_ended = false;
                        this.RoomManager.OnCycle();
                        this.ClientManager.OnCycle();
                    }
                    catch (Exception ex)
                    {
                        Logging.LogCriticalException("Exception in Game Loop!: " + ex.ToString());
                    }
				}
				Thread.Sleep(25);
			}
		}
		internal static void DatabaseCleanup(IQueryAdapter dbClient)
		{
			dbClient.runFastQuery("UPDATE users SET online = '0' WHERE online <> '0'");
			dbClient.runFastQuery("UPDATE rooms SET users_now = 0 WHERE users_now <> 0");
			dbClient.runFastQuery("UPDATE server_status SET status = 1, users_online = 0, rooms_loaded = 0, server_ver = 'Cyber Emulator', stamp = '" + CyberEnvironment.GetUnixTimestamp() + "' ");
		}
		internal void Destroy()
		{
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				Game.DatabaseCleanup(queryreactor);
			}
			this.GetClientManager();
			Console.WriteLine("Habbo Hotel was destroyed.");
		}
		internal void reloaditems()
		{
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				this.ItemManager.LoadItems(queryreactor);
				this.globalInventory = new InventoryGlobal();
			}
		}
	}
}
