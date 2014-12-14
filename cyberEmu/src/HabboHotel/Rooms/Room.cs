using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.Core;
using Cyber.HabboHotel.Catalogs;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Groups;
using Cyber.HabboHotel.Items;
using Cyber.HabboHotel.Pets;
using Cyber.HabboHotel.RoomBots;
using Cyber.HabboHotel.Rooms.Games;
using Cyber.HabboHotel.Rooms.RoomInvokedItems;
using Cyber.HabboHotel.Rooms.Wired;
using Cyber.HabboHotel.SoundMachine;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Collections.Specialized;

namespace Cyber.HabboHotel.Rooms
{
	public class Room
	{
		private uint Id;
		internal string Name;
		internal string Description;
		internal string Type;

		internal string Owner;
		internal int OwnerId;

		internal string Password;
		internal int Category;
		internal int State;
		internal int TradeState;
		internal int UsersNow;
		internal int UsersMax;
		internal string ModelName;
		internal int Score;

        internal bool RoomMuted;

		private int tagCount;
		internal ArrayList Tags;
		internal int AllowPets;
		internal int AllowPetsEating;
		internal int AllowWalkthrough;

        internal int WallThickness;
        internal int FloorThickness;
		internal int Hidewall;
        internal int WallHeight;

		internal int ChatType;
		internal int ChatBalloon;
		internal int ChatSpeed;
		internal int ChatMaxDistance;
		internal int ChatFloodProtection;

		internal uint GroupId;
		internal Guild Group;
		internal RoomEvent Event;
        
		private bool mCycleEnded;
		private int IdleTime;
		internal TeamManager teambanzai;
		internal TeamManager teamfreeze;
		internal List<uint> UsersWithRights;
		internal bool EveryoneGotRights;
		internal int WhoCanKick;
		internal int WhoCanMute;
		internal int WhoCanBan;
		internal Dictionary<long, double> Bans;
		internal Dictionary<uint, string> LoadedGroups;
		internal Dictionary<uint, uint> MutedUsers;
		internal string Wallpaper;
		internal string Floor;
		internal string Landscape;
		private GameManager game;
		private Gamemap gamemap;
		private RoomItemHandling roomItemHandling;
		private RoomUserManager roomUserManager;
		private Soccer soccer;
		private BattleBanzai banzai;
		private Freeze freeze;
		private GameItemHandler gameItemHandler;
		private RoomMusicController musicController;
        internal bool MutedBots;
		private WiredHandler wiredHandler;
		internal MoodlightData MoodlightData;
		internal TonerData TonerData;
		internal ArrayList ActiveTrades;
        internal HashSet<Chatlog> RoomChat;
		internal List<string> WordFilter;
		private RoomData mRoomData;
		private bool isCrashed;
		private bool mDisposed;
        private Queue roomKick;

        private Thread RoomThread;
        private Timer ProcessTimer;

		internal int UserCount
		{
			get
			{
                return this.roomUserManager.GetRoomUserCount();
			}
		}
		internal int TagCount
		{
			get
			{
				return this.Tags.Count;
			}
		}
		internal uint RoomId
		{
			get
			{
				return this.Id;
			}
		}
		internal bool CanTradeInRoom
		{
			get
			{
				return true;
			}
		}
		internal RoomData RoomData
		{
			get
			{
				return this.mRoomData;
			}
		}
		internal Gamemap GetGameMap()
		{
			return this.gamemap;
		}
		internal RoomItemHandling GetRoomItemHandler()
		{
			return this.roomItemHandling;
		}
		internal RoomUserManager GetRoomUserManager()
		{
			return this.roomUserManager;
		}
		internal Soccer GetSoccer()
		{
			if (this.soccer == null)
			{
				this.soccer = new Soccer(this);
			}
			return this.soccer;
		}
		internal TeamManager GetTeamManagerForBanzai()
		{
			if (this.teambanzai == null)
			{
				this.teambanzai = TeamManager.createTeamforGame("banzai");
			}
			return this.teambanzai;
		}
		internal TeamManager GetTeamManagerForFreeze()
		{
			if (this.teamfreeze == null)
			{
				this.teamfreeze = TeamManager.createTeamforGame("freeze");
			}
			return this.teamfreeze;
		}
		internal BattleBanzai GetBanzai()
		{
			if (this.banzai == null)
			{
				this.banzai = new BattleBanzai(this);
			}
			return this.banzai;
		}
		internal Freeze GetFreeze()
		{
			if (this.freeze == null)
			{
				this.freeze = new Freeze(this);
			}
			return this.freeze;
		}
		internal GameManager GetGameManager()
		{
			if (this.game == null)
			{
				this.game = new GameManager(this);
			}
			return this.game;
		}
		internal GameItemHandler GetGameItemHandler()
		{
			if (this.gameItemHandler == null)
			{
				this.gameItemHandler = new GameItemHandler(this);
			}
			return this.gameItemHandler;
		}
		internal RoomMusicController GetRoomMusicController()
		{
			if (this.musicController == null)
			{
				this.musicController = new RoomMusicController();
			}
			return this.musicController;
		}
		public WiredHandler GetWiredHandler()
		{
			if (this.wiredHandler == null)
			{
				this.wiredHandler = new WiredHandler(this);
			}
			return this.wiredHandler;
		}
		internal bool GotMusicController()
		{
			return this.musicController != null;
		}
		internal bool GotSoccer()
		{
			return this.soccer != null;
		}
		internal bool GotBanzai()
		{
			return this.banzai != null;
		}
		internal bool GotFreeze()
		{
			return this.freeze != null;
		}
		internal Room(RoomData Data)
		{
			this.InitializeFromRoomData(Data);
			this.GetRoomItemHandler().LoadFurniture();
			this.GetGameMap().GenerateMaps(true);
            this.GetGameMap().lazyWalkablePoints();
		}
		private void InitializeFromRoomData(RoomData Data)
		{
			this.Initialize(Data.Id, Data.Name, Data.Description, Data.Type, Data.Owner, Data.OwnerId, Data.Category, Data.State, Data.TradeState, Data.UsersMax, Data.ModelName, Data.Score, Data.Tags, Data.AllowPets, Data.AllowPetsEating, Data.AllowWalkthrough, Data.Hidewall, Data.Password, Data.Wallpaper, Data.Floor, Data.Landscape, Data, Data.AllowRightsOverride, Data.WallThickness, Data.FloorThickness, Data.Group, Data.GameId, Data.ChatType, Data.ChatBalloon, Data.ChatSpeed, Data.ChatMaxDistance, Data.ChatFloodProtection, Data.WhoCanMute, Data.WhoCanKick, Data.WhoCanBan, Data.GroupId, Data.RoomChat, Data.WordFilter, Data.WallHeight);
		}
        private void Initialize(uint Id, string Name, string Description, string Type, string Owner, int OwnerId, int Category, int State, int TradeState, int UsersMax, string ModelName, int Score, List<string> pTags, int AllowPets, int AllowPetsEating, int AllowWalkthrough, int Hidewall, string Password, string Wallpaper, string Floor, string Landscape, RoomData RoomData, bool RightOverride, int walltickness, int floorthickness, Guild group, int GameId, int chattype, int chatballoon, int chatspeed, int chatmaxdis, int chatprotection, int whomute, int whokick, int whoban, uint groupid, HashSet<Chatlog> Chat, List<string> WordFilter, int WallHeight)
        {
            this.mDisposed = false;
            this.Id = Id;
            this.Name = Name;
            this.Description = Description;
            this.Owner = Owner;
            this.OwnerId = OwnerId;
            this.Category = Category;
            this.Type = Type;
            this.State = State;
            this.TradeState = TradeState;
            this.UsersNow = 0;
            this.UsersMax = UsersMax;
            this.ModelName = ModelName;
            this.Score = Score;
            this.tagCount = 0;
            this.Tags = new ArrayList();
            foreach (string current in pTags)
            {
                this.tagCount++;
                this.Tags.Add(current);
            }
            this.ChatType = chattype;
            this.ChatBalloon = chatballoon;
            this.ChatSpeed = chatspeed;
            this.ChatMaxDistance = chatmaxdis;
            this.ChatFloodProtection = chatprotection;
            this.AllowPets = AllowPets;
            this.AllowPetsEating = AllowPetsEating;
            this.AllowWalkthrough = AllowWalkthrough;
            this.Hidewall = Hidewall;
            this.Group = group;
            this.Password = Password;
            this.Bans = new Dictionary<long, double>();
            this.MutedUsers = new Dictionary<uint, uint>();
            this.Wallpaper = Wallpaper;
            this.Floor = Floor;
            this.Landscape = Landscape;
            this.ActiveTrades = new ArrayList();
            this.MutedBots = false;
            this.mCycleEnded = false;
            this.mRoomData = RoomData;
            this.EveryoneGotRights = RightOverride;
            this.LoadedGroups = new Dictionary<uint, string>();
            this.roomKick = new Queue();
            this.IdleTime = 0;
            this.RoomMuted = false;
            this.WallThickness = walltickness;
            this.FloorThickness = floorthickness;
            this.WallHeight = WallHeight;
            this.gamemap = new Gamemap(this);
            this.roomItemHandling = new RoomItemHandling(this);
            this.roomUserManager = new RoomUserManager(this);
            this.RoomChat = Chat;
            this.WordFilter = WordFilter;
            this.Event = CyberEnvironment.GetGame().GetRoomEvents().GetEvent(Id);
            this.WhoCanBan = whoban;
            this.WhoCanKick = whokick;
            this.WhoCanBan = whoban;
            this.GroupId = groupid;
            this.LoadRights();
            this.LoadMusic();
            this.LoadBans();
            this.InitUserBots();

            this.RoomThread = new Thread(new ThreadStart(StartRoomProcessing));
            this.RoomThread.Start();
            CyberEnvironment.GetGame().GetRoomManager().QueueActiveRoomAdd(this.mRoomData);
        }

        internal void StartRoomProcessing()
        {
            this.ProcessTimer = new Timer(new TimerCallback(ProcessRoom), null, 0, 490);
        }

		internal void InitUserBots()
		{
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT * FROM bots WHERE room_id = " + this.RoomId + " AND ai_type = 'generic'");
				DataTable table = queryreactor.getTable();
				if (table != null)
				{
					foreach (DataRow dataRow in table.Rows)
					{
						queryreactor.setQuery("SELECT text, shout FROM bots_speech WHERE bots_speech.bot_id = '" + dataRow["id"] + "';");
						DataTable table2 = queryreactor.getTable();
						RoomBot roomBot = BotManager.GenerateBotFromRow(dataRow);
						List<RandomSpeech> list = new List<RandomSpeech>();
						foreach (DataRow dataRow2 in table2.Rows)
						{
							list.Add(new RandomSpeech((string)dataRow2["text"], CyberEnvironment.EnumToBool(dataRow2["shout"].ToString())));
						}
						List<BotResponse> list2 = new List<BotResponse>();
						this.roomUserManager.DeployBot(new RoomBot(roomBot.BotId, roomBot.OwnerId, this.RoomId, AIType.Generic, "freeroam", roomBot.Name, roomBot.Motto, roomBot.Look, roomBot.X, roomBot.Y, (double)checked((int)roomBot.Z), 4, 0, 0, 0, 0, ref list, ref list2, roomBot.Gender, roomBot.DanceId, roomBot.IsBartender), null);
					}
				}
			}
		}
		internal void ClearTags()
		{
			this.Tags.Clear();
			this.tagCount = 0;
		}
		internal void AddTagRange(List<string> tags)
		{
			checked
			{
				this.tagCount += tags.Count;
				this.Tags.AddRange(tags);
			}
		}
		internal void InitBots()
		{
			List<RoomBot> botsForRoom = CyberEnvironment.GetGame().GetBotManager().GetBotsForRoom(this.RoomId);
			foreach (RoomBot current in botsForRoom)
			{
				if (!current.IsPet)
				{
					this.DeployBot(current);
				}
			}
		}
		internal void InitPets()
		{
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT * FROM bots WHERE room_id = " + this.RoomId + " AND ai_type='pet'");
				DataTable table = queryreactor.getTable();
				if (table != null)
				{
					foreach (DataRow dataRow in table.Rows)
					{
						queryreactor.setQuery("SELECT * FROM bots_petdata WHERE id=" + dataRow[0] + " LIMIT 1");
						DataRow row = queryreactor.getRow();
						if (row != null)
						{
							Pet pet = Catalog.GeneratePetFromRow(dataRow, row);
							List<RandomSpeech> list = new List<RandomSpeech>();
							List<BotResponse> list2 = new List<BotResponse>();
							this.roomUserManager.DeployBot(new RoomBot(pet.PetId, Convert.ToUInt32(this.OwnerId), this.RoomId, AIType.Pet, "freeroam", pet.Name, "", pet.Look, pet.X, pet.Y, (double)checked((int)pet.Z), 4, 0, 0, 0, 0, ref list, ref list2, "", 0, false), pet);
						}
					}
				}
			}
		}
		internal RoomUser DeployBot(RoomBot Bot)
		{
			return this.roomUserManager.DeployBot(Bot, null);
		}
		internal void QueueRoomKick(RoomKick kick)
		{
			lock (this.roomKick.SyncRoot)
			{
				this.roomKick.Enqueue(kick);
			}
		}

		private void WorkRoomKickQueue()
		{
			if (this.roomKick.Count > 0)
			{
				lock (this.roomKick.SyncRoot)
				{
					while (this.roomKick.Count > 0)
					{
						RoomKick roomKick = (RoomKick)this.roomKick.Dequeue();
						List<RoomUser> list = new List<RoomUser>();
						foreach (RoomUser current in this.roomUserManager.UserList.Values)
						{
							if (!current.IsBot && (ulong)current.GetClient().GetHabbo().Rank < (ulong)((long)roomKick.minrank))
							{
								if (roomKick.allert.Length > 0)
								{
									current.GetClient().SendNotif("You have been kicked by an moderator: " + roomKick.allert);
								}
								list.Add(current);
							}
						}
						foreach (RoomUser current2 in list)
						{
							this.GetRoomUserManager().RemoveUserFromRoom(current2.GetClient(), true, false);
							current2.GetClient().CurrentRoomUserID = -1;
						}
					}
				}
			}
		}
		
		
		
		internal void onRoomKick()
		{
			List<RoomUser> list = new List<RoomUser>();
			foreach (RoomUser current in this.roomUserManager.UserList.Values)
			{
				if (!current.IsBot && current.GetClient().GetHabbo().Rank < 4u)
				{
					list.Add(current);
				}
			}
			checked
			{
				for (int i = 0; i < list.Count; i++)
				{
					this.GetRoomUserManager().RemoveUserFromRoom(list[i].GetClient(), true, false);
					list[i].GetClient().CurrentRoomUserID = -1;
				}
			}
		}

        internal void OnUserEnter(RoomUser User)
        {
           this.GetWiredHandler().ExecuteWired(WiredItemType.TriggerUserEntersRoom, new object[]
							{
								User
							});

            int Count = 0;

            foreach (RoomUser current in this.roomUserManager.UserList.Values)
            {
                if (current.IsBot || current.IsPet)
                {
                    current.BotAI.OnUserEnterRoom(User);
                    Count++;
                }

                if (Count >= 3)
                {
                    break;
                }
            }
        }

		internal void OnUserSay(RoomUser User, string Message, bool Shout)
		{
            foreach (RoomUser current in this.roomUserManager.UserList.Values)
            {
                try
                {
                    if (current.IsBot || current.IsPet)
                    {
                        if (!current.IsPet && Message.StartsWith(current.BotData.Name))
                        {
                            Message = Message.Substring(current.BotData.Name.Length);
                            if (Shout)
                            {
                                current.BotAI.OnUserShout(User, Message);
                            }
                            else
                            {
                                current.BotAI.OnUserSay(User, Message);
                            }
                        }
                        else if (current.IsPet && Message.StartsWith(current.PetData.Name) && current.PetData.Type != 16)
                        {
                            Message = Message.Substring(current.PetData.Name.Length);
                                current.BotAI.OnUserSay(User, Message);
                            
                        }
                    }
                }
                catch (Exception)
                {
                    return;
                }
            }
            
		}
		internal void LoadMusic()
		{
			DataTable table;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT items_rooms_songs.songid,items.id,items.base_item FROM items_rooms_songs LEFT JOIN items ON items.id = items_rooms_songs.itemid WHERE items_rooms_songs.roomid = " + this.RoomId);
				table = queryreactor.getTable();
			}

            if (table != null)
            {
                foreach (DataRow dataRow in table.Rows)
                {
                    uint songID = (uint)dataRow[0];
                    uint num = Convert.ToUInt32(dataRow[1]);
                    int baseItem = Convert.ToInt32(dataRow[2]);
                    string songCode = "";
                    string extraData = "";
                    using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
                    {
                        queryreactor2.setQuery("SELECT extra_data,songcode FROM items WHERE id = " + num);
                        DataRow row = queryreactor2.getRow();
                        if (row != null)
                        {
                            extraData = (string)row["extra_data"];
                            songCode = (string)row["songcode"];
                        }
                    }
                    SongItem diskItem = new SongItem(num, songID, baseItem, extraData, songCode);
                    this.GetRoomMusicController().AddDisk(diskItem);
                }
            }
		}
		internal void LoadRights()
		{
			this.UsersWithRights = new List<uint>();
			DataTable dataTable = new DataTable();
			if (this.Group != null)
			{
				return;
			}
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT room_rights.user_id FROM room_rights WHERE room_id = " + this.Id);
				dataTable = queryreactor.getTable();
			}
			if (dataTable == null)
			{
				return;
			}
			foreach (DataRow dataRow in dataTable.Rows)
			{
				this.UsersWithRights.Add(Convert.ToUInt32(dataRow["user_id"]));
			}
		}
		internal void LoadBans()
		{
			this.Bans = new Dictionary<long, double>();
			DataTable table;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT user_id, expire FROM room_bans WHERE room_id = " + this.Id);
				table = queryreactor.getTable();
			}
			if (table == null)
			{
				return;
			}
			foreach (DataRow dataRow in table.Rows)
			{
				this.Bans.Add((long)((ulong)((uint)dataRow[0])), Convert.ToDouble(dataRow[1]));
			}
		}
		internal int GetRightsLevel(GameClient Session)
		{
			try
			{
				if (Session == null || Session.GetHabbo() == null)
				{
					int result = 0;
					return result;
				}
				if (Session.GetHabbo().Username == this.Owner)
				{
					int result = 4;
					return result;
				}
				if (Session.GetHabbo().HasFuse("fuse_admin") || Session.GetHabbo().HasFuse("fuse_any_room_controller"))
				{
					int result = 4;
					return result;
				}
				if (Session.GetHabbo().HasFuse("fuse_any_room_rights"))
				{
					int result = 3;
					return result;
				}
				if (this.UsersWithRights.Contains(Session.GetHabbo().Id))
				{
					int result = 1;
					return result;
				}
				if (this.EveryoneGotRights)
				{
					int result = 1;
					return result;
				}
			}
			catch (Exception pException)
			{
				Logging.HandleException(pException, "GetRightsLevel");
			}
			return 0;
		}
		internal bool CheckRights(GameClient Session)
		{
			return this.CheckRights(Session, false, false);
		}
		internal bool CheckGroupRights(GameClient Session)
		{
			try
			{
				if (this.Group == null)
				{
					bool result = false;
					return result;
				}
				if (this.Group.AdminOnlyDeco == 0u && this.Group.Members.ContainsKey(Session.GetHabbo().Id))
				{
					bool result = true;
					return result;
				}
				if (this.Group.Admins.ContainsKey(Session.GetHabbo().Id))
				{
					Session.SendNotif("Group Admin");
					bool result = true;
					return result;
				}
				if (Session.GetHabbo().Username == this.Owner)
				{
					bool result = true;
					return result;
				}
				if (Session.GetHabbo().HasFuse("fuse_admin") || Session.GetHabbo().HasFuse("fuse_any_room_controller"))
				{
					bool result = true;
					return result;
				}
			}
			catch (Exception pException)
			{
				Logging.HandleException(pException, "Room.CheckGroupRights");
			}
			return false;
		}
		internal bool CheckRights(GameClient Session, bool RequireOwnership, bool CheckForGroups = false)
		{
			try
			{
				if (Session == null || Session.GetHabbo() == null)
				{
					bool result = false;
					return result;
				}
				if (Session.GetHabbo().Username == this.Owner && this.Type == "private")
				{
					bool result = true;
					return result;
				}
				if (Session.GetHabbo().HasFuse("fuse_admin") || Session.GetHabbo().HasFuse("fuse_any_room_controller"))
				{
					bool result = true;
					return result;
				}
				if (!RequireOwnership && this.Type == "private")
				{
					if (Session.GetHabbo().HasFuse("fuse_any_room_rights"))
					{
						bool result = true;
						return result;
					}
					if (this.UsersWithRights.Contains(Session.GetHabbo().Id))
					{
						bool result = true;
						return result;
					}
					if (this.EveryoneGotRights)
					{
						bool result = true;
						return result;
					}
				}
				if (CheckForGroups && this.Type == "private")
				{
					if (this.Group == null)
					{
						bool result = false;
						return result;
					}
					if (this.Group.Admins.ContainsKey(Session.GetHabbo().Id))
					{
						bool result = true;
						return result;
					}
					if (this.Group.AdminOnlyDeco == 0u && this.Group.Members.ContainsKey(Session.GetHabbo().Id))
					{
						bool result = true;
						return result;
					}
				}
			}
			catch (Exception pException)
			{
				Logging.HandleException(pException, "Room.CheckRights");
			}
			return false;
		}

        internal void ProcessRoom(object callItem)
        {
            this.ProcessRoom();
        }

        internal void ProcessRoom()
        {
            try
            {
                if (isCrashed || mDisposed)
                    return;
                try
                {
                    int idle = 0;
                    GetRoomItemHandler().OnCycle();
                    GetRoomUserManager().OnCycle(ref idle);

                    if (idle > 0)
                    {
                        IdleTime++;
                    }
                    else
                    {
                        IdleTime = 0;
                    }

                    if (!mCycleEnded)
                    {
                        if (this.IdleTime >= 10/* && usersQueueToEnter.Count == 0*/)
                        {
                            CyberEnvironment.GetGame().GetRoomManager().UnloadRoom(this);
                            return;
                        }
                        else
                        {
                            ServerMessage serverMessage = this.GetRoomUserManager().SerializeStatusUpdates(false);

                            if (serverMessage != null)
                                SendMessage(serverMessage);
                        }
                    }

                    if (gameItemHandler != null)
                        gameItemHandler.OnCycle();
                    if (game != null)
                    { game.OnCycle(); }
                    if (GotBanzai())
                    { banzai.OnCycle(); }
                    if (GotSoccer())
                    { soccer.OnCycle(); }
                        roomUserManager.UserList.OnCycle();
                 
                    GetWiredHandler().OnCycle();
                    WorkRoomKickQueue();
                }
                catch (Exception e)
                {
                    OnRoomCrash(e);
                }
            }
            catch (Exception e)
            {
                Logging.LogCriticalException("Sub crash in room cycle: " + e.ToString());
            }
        }

		private void OnRoomCrash(Exception e)
		{
			Logging.LogThreadException(e.ToString(), "Room cycle task for room " + this.RoomId);
			CyberEnvironment.GetGame().GetRoomManager().UnloadRoom(this);
			this.isCrashed = true;
		}
        internal void SendMessage(byte[] message)
        {
            try
            {
                var roomUsers = GetRoomUserManager().GetRoomUsers();

                foreach (RoomUser User in roomUsers)
                {
                    User.SendMessage(message);
                }
            }
            catch
            {

            }
        }

        internal void BroadcastChatMessage(ServerMessage chatMsg, RoomUser roomUser, uint p)
        {
            try
            {
                var roomUsers = GetRoomUserManager().GetRoomUsers();
                byte[] msg = chatMsg.GetBytes();
                foreach (RoomUser User in roomUsers)
                {
                    if (!User.OnCampingTent && roomUser.OnCampingTent)
                        continue;
                    else if (User.GetClient().GetHabbo().MutedUsers.Contains(p))
                        continue;

                    User.SendMessage(msg);
                }
            }
            catch
            {

            }
        }

        internal void SendMessage(ServerMessage Message)
        {
            if (Message != null)
            {
                byte[] message = Message.GetBytes();
                SendMessage(message);
            }
        }


		internal void SendMessage(List<ServerMessage> Messages)
		{
			if (Messages.Count == 0)
			{
				return;
			}
				try
				{
					byte[] array = new byte[0];
					int num = 0;
					foreach (ServerMessage current in Messages)
					{
						byte[] bytes = current.GetBytes();
						int newSize = array.Length + bytes.Length;
						Array.Resize<byte>(ref array, newSize);
						for (int i = 0; i < bytes.Length; i++)
						{
							array[num] = bytes[i];
							num++;
						}
					}

                    SendMessage(array);
				}
				catch (Exception pException)
				{
					Logging.HandleException(pException, "Room.SendMessage List<ServerMessage>");
				}
		}
		
		internal void SendMessageToUsersWithRights(ServerMessage Message)
		{
			try
			{
				byte[] bytes = Message.GetBytes();
				foreach (RoomUser current in this.roomUserManager.UserList.Values)
				{
					if (!current.IsBot)
					{
						GameClient client = current.GetClient();
						if (client != null && this.CheckRights(client))
						{
							try
							{
								client.GetConnection().SendData(bytes);
							}
							catch (Exception pException)
							{
								Logging.HandleException(pException, "Room.SendMessageToUsersWithRights");
							}
						}
					}
				}
			}
			catch (Exception pException2)
			{
				Logging.HandleException(pException2, "Room.SendMessageToUsersWithRights");
			}
		}
		internal void Destroy()
		{
			this.SendMessage(new ServerMessage(Outgoing.OutOfRoomMessageComposer));
			this.Dispose();
		}
		private void Dispose()
		{
			if (!this.mDisposed)
			{
				this.mDisposed = true;
				this.mCycleEnded = true;
				CyberEnvironment.GetGame().GetRoomManager().QueueActiveRoomRemove(this.mRoomData);
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					this.GetRoomItemHandler().SaveFurniture(queryreactor, null);
					queryreactor.runFastQuery("UPDATE rooms SET users_now=0 WHERE id = " + this.Id + " LIMIT 1");
				}
                this.ProcessTimer.Dispose();
                this.ProcessTimer = null;
				this.tagCount = 0;
				this.Tags.Clear();
				this.roomUserManager.UserList.Clear();
				this.UsersWithRights.Clear();
				this.Bans.Clear();
				this.LoadedGroups.Clear();
				this.RoomChat.Clear();
				this.GetWiredHandler().Destroy();
				foreach (RoomItem current in this.GetRoomItemHandler().mFloorItems.Values)
				{
					current.Destroy();
				}
				foreach (RoomItem current2 in this.GetRoomItemHandler().mWallItems.Values)
				{
					current2.Destroy();
				}
				this.ActiveTrades.Clear();
			}
		}
		internal bool UserIsBanned(uint pId)
		{
			return this.Bans.ContainsKey((long)((ulong)pId));
		}
		internal void RemoveBan(uint pId)
		{
			this.Bans.Remove((long)((ulong)pId));
		}
		internal void AddBan(int pId, long Time)
		{
			if (!this.Bans.ContainsKey((long)Convert.ToInt32(pId)))
			{
				this.Bans.Add((long)pId, (double)checked(unchecked((long)CyberEnvironment.GetUnixTimestamp()) + Time));
			}
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"REPLACE INTO room_bans VALUES (",
					pId,
					", ",
					this.Id,
					", '",
					(CyberEnvironment.GetUnixTimestamp() + Time),
					"')"
				}));
			}
		}
		internal List<uint> BannedUsers()
		{
			List<uint> list = new List<uint>();
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT user_id FROM room_bans WHERE expire > UNIX_TIMESTAMP() AND room_id=" + this.Id);
				DataTable table = queryreactor.getTable();
				foreach (DataRow dataRow in table.Rows)
				{
					list.Add((uint)dataRow[0]);
				}
			}
			return list;
		}
		internal bool HasBanExpired(uint pId)
		{
			return !this.UserIsBanned(pId) || this.Bans[(long)((ulong)pId)] < (double)CyberEnvironment.GetUnixTimestamp();
		}
		internal void Unban(uint UserId)
		{
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"DELETE FROM room_bans WHERE user_id=",
					UserId,
					" AND room_id=",
					this.Id,
					" LIMIT 1"
				}));
			}
			this.Bans.Remove((long)((ulong)UserId));
		}
		internal bool HasActiveTrade(RoomUser User)
		{
			return !User.IsBot && this.HasActiveTrade(User.GetClient().GetHabbo().Id);
		}
		internal bool HasActiveTrade(uint UserId)
		{
			object[] array = this.ActiveTrades.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				Trade trade = (Trade)array[i];
				if (trade.ContainsUser(UserId))
				{
					return true;
				}
			}
			return false;
		}
		internal Trade GetUserTrade(uint UserId)
		{
			object[] array = this.ActiveTrades.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				Trade trade = (Trade)array[i];
				if (trade.ContainsUser(UserId))
				{
					return trade;
				}
			}
			return null;
		}
		internal void TryStartTrade(RoomUser UserOne, RoomUser UserTwo)
		{
			if (UserOne == null || UserTwo == null || UserOne.IsBot || UserTwo.IsBot || UserOne.IsTrading || UserTwo.IsTrading || this.HasActiveTrade(UserOne) || this.HasActiveTrade(UserTwo))
			{
				return;
			}
			this.ActiveTrades.Add(new Trade(UserOne.GetClient().GetHabbo().Id, UserTwo.GetClient().GetHabbo().Id, this.RoomId));
		}
		internal void TryStopTrade(uint UserId)
		{
			Trade userTrade = this.GetUserTrade(UserId);
			if (userTrade == null)
			{
				return;
			}
			userTrade.CloseTrade(UserId);
			this.ActiveTrades.Remove(userTrade);
		}
		internal void SetMaxUsers(int MaxUsers)
		{
			this.UsersMax = MaxUsers;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"UPDATE rooms SET users_max = ",
					MaxUsers,
					" WHERE id = ",
					this.RoomId
				}));
			}
		}
		internal void FlushSettings()
		{
			this.mCycleEnded = true;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				this.GetRoomItemHandler().SaveFurniture(queryreactor, null);
			}
			this.Tags.Clear();
			this.UsersWithRights.Clear();
			this.Bans.Clear();
			this.ActiveTrades.Clear();
			this.LoadedGroups.Clear();
			if (this.GotFreeze())
			{
				this.freeze = new Freeze(this);
			}
			if (this.GotBanzai())
			{
				this.banzai = new BattleBanzai(this);
			}
			if (this.GotSoccer())
			{
				this.soccer = new Soccer(this);
			}
			if (this.gameItemHandler != null)
			{
				this.gameItemHandler = new GameItemHandler(this);
			}
		}
		internal void ReloadSettings()
		{
			RoomData data = CyberEnvironment.GetGame().GetRoomManager().GenerateRoomData(this.RoomId);
			this.InitializeFromRoomData(data);
		}
		internal void RequestReload()
		{
		}
		internal void onReload()
		{
			HashSet<RoomUser> roomUsers = this.GetRoomUserManager().GetRoomUsers();
            Hashtable userID = new Hashtable(GetRoomUserManager().usersByUserID);
            Hashtable userName = new Hashtable(GetRoomUserManager().usersByUsername);
			int primaryID = 0;
			int secondaryID = 0;
			this.GetRoomUserManager().backupCounters(ref primaryID, ref secondaryID);
			this.FlushSettings();
			this.ReloadSettings();
			this.GetRoomUserManager().UpdateUserStats(roomUsers, userID, userName, primaryID, secondaryID);
			this.UpdateFurniture();
			this.GetGameMap().GenerateMaps(true);
            this.GetGameMap().lazyWalkablePoints();
		}
		internal void UpdateFurniture()
		{
			List<ServerMessage> list = new List<ServerMessage>();
			RoomItem[] array = this.GetRoomItemHandler().mFloorItems.Values.ToArray<RoomItem>();
			RoomItem[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				RoomItem roomItem = array2[i];
				ServerMessage serverMessage = new ServerMessage(Outgoing.UpdateRoomItemMessageComposer);
				roomItem.Serialize(serverMessage);
				list.Add(serverMessage);
			}
			Array.Clear(array, 0, array.Length);
			RoomItem[] array3 = this.GetRoomItemHandler().mWallItems.Values.ToArray<RoomItem>();
			RoomItem[] array4 = array3;
			for (int j = 0; j < array4.Length; j++)
			{
				RoomItem roomItem2 = array4[j];
				ServerMessage serverMessage2 = new ServerMessage(Outgoing.UpdateRoomWallItemMessageComposer);
				roomItem2.Serialize(serverMessage2);
				list.Add(serverMessage2);
			}
			Array.Clear(array3, 0, array3.Length);
			this.SendMessage(list);
		}
		internal bool CheckMute(GameClient Session)
		{
			if (this.MutedUsers.ContainsKey(Session.GetHabbo().Id))
			{
				if (this.MutedUsers[Session.GetHabbo().Id] >= CyberEnvironment.GetUnixTimestamp())
				{
					return true;
				}
				this.MutedUsers.Remove(Session.GetHabbo().Id);
			}
			return Session.GetHabbo().Muted || this.RoomMuted;
		}
		internal void AddChatlog(uint Id, string Message, bool Banned)
		{
			Chatlog item = new Chatlog(Id, Message, (double)CyberEnvironment.GetUnixTimestamp(), false);
				this.RoomChat.Add(item);
		}

        internal void ResetGamemap(string newModelName, int wallHeight, int wallThick, int floorThick)
        {
            this.ModelName = newModelName;
            this.mRoomData.ModelName = newModelName;
            this.mRoomData.ResetModel();
            this.mRoomData.WallHeight = wallHeight;
            this.mRoomData.WallThickness = WallThickness;
            this.mRoomData.FloorThickness = floorThick;
            this.gamemap = new Gamemap(this);
        }

       
    }
}
