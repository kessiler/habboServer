using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.Core;
using Cyber.HabboHotel.Achievements;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Groups;
using Cyber.HabboHotel.Items;
using Cyber.HabboHotel.Misc;
using Cyber.HabboHotel.Polls;
using Cyber.HabboHotel.Rooms;
using Cyber.HabboHotel.Users.Badges;
using Cyber.HabboHotel.Users.Inventory;
using Cyber.HabboHotel.Users.Messenger;
using Cyber.HabboHotel.Users.Relationships;
using Cyber.HabboHotel.Users.Subscriptions;
using Cyber.HabboHotel.Users.UserDataManagement;
using Cyber.Messages;
using Cyber.Messages.Headers;
using Cyber.ServerManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Cyber.HabboHotel.Users
{
	public class Habbo
	{
		internal uint Id;
		internal string Username;
		internal string RealName;
		internal double CreateDate;
		internal uint Rank;
		internal string Motto;
		internal int LastChange;
		internal string Look;
		internal string Gender;
		internal int Credits;
		internal int AchievementPoints;
		internal int ActivityPoints;
		internal int BelCredits;
		internal bool Muted;
		internal int Respect;
		internal int DailyRespectPoints;
		internal int DailyPetRespectPoints;
		internal uint LoadingRoom;
		internal bool LoadingChecksPassed;
		internal uint CurrentRoomId;
		internal uint HomeRoom;
		internal int LastOnline;
		internal bool IsTeleporting;
		internal bool IsHopping;
		internal uint TeleportingRoomID;
		internal uint TeleporterId;
		internal uint HopperId;
		internal ArrayList FavoriteRooms;
		internal List<uint> MutedUsers;
		internal List<string> Tags;
		internal Dictionary<string, UserAchievement> Achievements;
		internal Dictionary<int, UserTalent> Talents;
        internal HashSet<uint> RatedRooms;
		internal HashSet<uint> RecentlyVisitedRooms;
		private SubscriptionManager SubscriptionManager;
		private HabboMessenger Messenger;
		private BadgeComponent BadgeComponent;
		private InventoryComponent InventoryComponent;
		private AvatarEffectsInventoryComponent AvatarEffectsInventoryComponent;
		private ChatCommandHandler CommandHandler;
		private GameClient mClient;
		internal bool SpectatorMode;
		internal bool Disconnected;
		internal bool HasFriendRequestsDisabled;
		internal HashSet<RoomData> UsersRooms;
        internal HashSet<GroupUser> UserGroups;
		internal uint FavouriteGroup;
		internal bool spamProtectionBol;
		internal int spamProtectionCount = 1;
		internal int spamProtectionTime;
		internal int spamProtectionAbuse;
		internal uint FriendCount;
		internal DateTime spamFloodTime;
		internal Dictionary<uint, int> quests;
		internal uint CurrentQuestId;
		internal uint LastQuestCompleted;
		internal int FloodTime;
		internal bool HideInRoom;
		internal bool AppearOffline;
		internal bool VIP;
		internal DateTime LastGiftPurchaseTime;
		internal DateTime LastGiftOpenTime;
		internal int TradeLockExpire;
		internal bool TradeLocked;
		internal string TalentStatus;
		internal int CurrentTalentLevel;
		internal Dictionary<int, Relationship> Relationships;
		internal HashSet<uint> AnsweredPolls;
        internal bool NUXPassed;
        internal int MinimailUnreadMessages;
        internal int lastSqlQuery;


		private bool HabboinfoSaved;

        internal string HeadPart
        {
            get
            {
                string[] strtmp = Look.Split('.');
                string tmp2 = strtmp.Where(x => x.Contains("hd-")).FirstOrDefault();
                string lookToReturn = tmp2 == null ? "" : tmp2;

                if (Look.Contains("ha-"))
                {
                    lookToReturn += "." + strtmp.Where(x => x.Contains("ha-")).FirstOrDefault();
                }
                if (Look.Contains("ea-"))
                {
                    lookToReturn += "." + strtmp.Where(x => x.Contains("ea-")).FirstOrDefault();
                }
                if (Look.Contains("hr-"))
                {
                    lookToReturn += "." + strtmp.Where(x => x.Contains("hr-")).FirstOrDefault();
                }
                if (Look.Contains("he-"))
                {
                    lookToReturn += "." + strtmp.Where(x => x.Contains("he-")).FirstOrDefault();
                }
                if (Look.Contains("fa-"))
                {
                    lookToReturn += "." + strtmp.Where(x => x.Contains("fa-")).FirstOrDefault();
                }

                return lookToReturn;
            }
        }

		internal bool InRoom
		{
			get
			{
				return this.CurrentRoomId >= 1u && this.CurrentRoom != null;
			}
		}
		internal Room CurrentRoom
		{
			get
			{
				if (this.CurrentRoomId <= 0u)
				{
					return null;
				}
				return CyberEnvironment.GetGame().GetRoomManager().GetRoom(this.CurrentRoomId);
			}
		}
		public bool CanChangeName
		{
			get
			{
                return (ExtraSettings.CHANGE_NAME_STAFF && this.HasFuse("fuse_can_change_name")) || (ExtraSettings.CHANGE_NAME_VIP && this.VIP) || (ExtraSettings.CHANGE_NAME_EVERYONE && CyberEnvironment.GetUnixTimestamp() > (LastChange + 604800));
			}
		}

		internal bool IsHelper
		{
			get
			{
				return this.TalentStatus == "helper" || Rank >= 4;
			}
		}
		internal bool IsCitizen
		{
			get
			{
				return this.CurrentTalentLevel > 4;
			}
		}
		internal string GetQueryString
		{
			get
			{
				this.HabboinfoSaved = true;
				return string.Concat(new object[]
				{
					"UPDATE users SET online='0', last_online = '",
					CyberEnvironment.GetUnixTimestamp(),
					"', activity_points = '",
					this.ActivityPoints,
                    "', seasonal_currency = '",
                    this.BelCredits,
					"', credits = '",
					this.Credits,
					"' WHERE id = '",
					this.Id,
					"'; UPDATE user_stats SET achievement_score = ",
					this.AchievementPoints,
					" WHERE id=",
					this.Id,
					" LIMIT 1; "
				});
			}
		}
		internal Habbo(uint Id, string Username, string RealName, uint Rank, string Motto, string Look, string Gender, int Credits, int ActivityPoints, double LastActivityPointsUpdate, bool Muted, uint HomeRoom, int Respect, int DailyRespectPoints, int DailyPetRespectPoints, bool HasFriendRequestsDisabled, uint currentQuestID, int currentQuestProgress, int achievementPoints, int RegTimestamp, int LastOnline, bool AppearOffline, bool HideInRoom, bool VIP, double CreateDate, bool Online, string Citizenship, int BelCredits, HashSet<GroupUser> Groups, uint FavId, int LastChange, bool TradeLocked, int TradeLockExpire, bool NUXPassed)
		{
			this.Id = Id;
			this.Username = Username;
			this.RealName = RealName;
			if (Rank < 1u)
			{
				Rank = 1u;
			}
			this.Rank = Rank;
			this.Motto = Motto;
            this.Look = CyberEnvironment.GetGame().GetAntiMutant().RunLook(Look.ToLower());
			this.VIP = VIP;
			this.LastChange = LastChange;
			this.TradeLocked = TradeLocked;
			this.TradeLockExpire = TradeLockExpire;
			this.Gender = Gender.ToLower();
			this.Credits = Credits;
			this.ActivityPoints = ActivityPoints;
			this.BelCredits = BelCredits;
			this.AchievementPoints = achievementPoints;
			this.Muted = Muted;
			this.LoadingRoom = 0u;
			this.CreateDate = CreateDate;
			this.LoadingChecksPassed = false;
			this.FloodTime = 0;
            this.NUXPassed = NUXPassed;
			this.CurrentRoomId = 0u;
			this.HomeRoom = HomeRoom;
			this.HideInRoom = HideInRoom;
			this.AppearOffline = AppearOffline;
			this.FavoriteRooms = new ArrayList();
			this.MutedUsers = new List<uint>();
			this.Tags = new List<string>();
			this.Achievements = new Dictionary<string, UserAchievement>();
			this.Talents = new Dictionary<int, UserTalent>();
			this.Relationships = new Dictionary<int, Relationship>();
			this.RatedRooms = new HashSet<uint>();
			this.Respect = Respect;
			this.DailyRespectPoints = DailyRespectPoints;
			this.DailyPetRespectPoints = DailyPetRespectPoints;
			this.IsTeleporting = false;
			this.TeleporterId = 0u;
			this.UsersRooms = new HashSet<RoomData>();
			this.HasFriendRequestsDisabled = HasFriendRequestsDisabled;
            this.LastOnline = CyberEnvironment.GetUnixTimestamp();//LastOnline;
            this.RecentlyVisitedRooms = new HashSet<uint>();
			this.CurrentQuestId = currentQuestID;
			this.IsHopping = false;
            
			if (CyberEnvironment.GetGame().GetGroupManager().GetGroup(FavId) != null)
			{
				this.FavouriteGroup = FavId;
			}
			else
			{
				this.FavouriteGroup = 0u;
			}
			this.UserGroups = Groups;
			if (this.DailyPetRespectPoints > 99)
			{
				this.DailyPetRespectPoints = 99;
			}
			if (this.DailyRespectPoints > 99)
			{
				this.DailyRespectPoints = 99;
			}
			this.LastGiftPurchaseTime = DateTime.Now;
			this.LastGiftOpenTime = DateTime.Now;
			this.TalentStatus = Citizenship;
			this.CurrentTalentLevel = this.GetCurrentTalentLevel();
		}
		internal void InitInformation(UserData data)
		{
			this.SubscriptionManager = new SubscriptionManager(this.Id, data);
			this.BadgeComponent = new BadgeComponent(this.Id, data);
			this.quests = data.quests;
			this.Messenger = new HabboMessenger(this.Id);
			this.Messenger.Init(data.friends, data.requests);
			this.SpectatorMode = false;
			this.Disconnected = false;
			this.UsersRooms = data.rooms;
			this.Relationships = data.Relations;
			this.AnsweredPolls = data.suggestedPolls;
		}
		internal void Init(GameClient client, UserData data)
		{
			this.mClient = client;
			this.SubscriptionManager = new SubscriptionManager(this.Id, data);
			this.BadgeComponent = new BadgeComponent(this.Id, data);
			this.InventoryComponent = InventoryGlobal.GetInventory(this.Id, client, data);
			this.InventoryComponent.SetActiveState(client);
			this.CommandHandler = new ChatCommandHandler(client);
			this.AvatarEffectsInventoryComponent = new AvatarEffectsInventoryComponent(this.Id, client, data);
			this.quests = data.quests;
			this.Messenger = new HabboMessenger(this.Id);
			this.Messenger.Init(data.friends, data.requests);
			this.FriendCount = Convert.ToUInt32(data.friends.Count);
			this.SpectatorMode = false;
			this.Disconnected = false;
			this.UsersRooms = data.rooms;
            this.MinimailUnreadMessages = data.miniMailCount;
			this.Relationships = data.Relations;
			this.AnsweredPolls = data.suggestedPolls;
		}
		internal ChatCommandHandler GetCommandHandler()
		{
			return this.CommandHandler;
		}
		internal void UpdateRooms(IQueryAdapter dbClient)
		{
			this.UsersRooms.Clear();
			dbClient.setQuery("SELECT * FROM rooms WHERE owner = @name ORDER BY id ASC LIMIT 50");
			dbClient.addParameter("name", this.Username);
			DataTable table = dbClient.getTable();
			foreach (DataRow dataRow in table.Rows)
			{
				this.UsersRooms.Add(CyberEnvironment.GetGame().GetRoomManager().FetchRoomData(Convert.ToUInt32(dataRow["id"]), dataRow));
			}
		}

		internal void LoadData(UserData data)
		{
			this.LoadAchievements(data.achievements);
			this.LoadTalents(data.talents);
			this.LoadFavorites(data.favouritedRooms);
			this.LoadMutedUsers(data.ignores);
			this.LoadTags(data.tags);
		}
		internal void SerializeQuests(ref QueuedServerMessage response)
		{
			CyberEnvironment.GetGame().GetQuestManager().GetList(this.mClient, null);
		}
		internal bool GotCommand(string Cmd)
		{
			return CyberEnvironment.GetGame().GetRoleManager().RankGotCommand(this.Rank, Cmd);
		}
		internal bool HasFuse(string Fuse)
		{
			return CyberEnvironment.GetGame().GetRoleManager().RankHasRight(this.Rank, Fuse) || (this.GetSubscriptionManager().HasSubscription && CyberEnvironment.GetGame().GetRoleManager().HasVIP(this.GetSubscriptionManager().GetSubscription().SubscriptionId, Fuse));
		}
		internal void LoadFavorites(List<uint> roomID)
		{
			this.FavoriteRooms = new ArrayList();
			foreach (uint current in roomID)
			{
				this.FavoriteRooms.Add(current);
			}
		}
		internal void LoadMutedUsers(List<uint> usersMuted)
		{
			this.MutedUsers = usersMuted;
		}
		internal void LoadTags(List<string> tags)
		{
			this.Tags = tags;
		}
		internal void SerializeClub()
		{
			GameClient client = this.GetClient();
			ServerMessage serverMessage = new ServerMessage();
			serverMessage.Init(Outgoing.SubscriptionStatusMessageComposer);
			serverMessage.AppendString("club_habbo");
			if (client.GetHabbo().GetSubscriptionManager().HasSubscription)
			{
				double num = (double)client.GetHabbo().GetSubscriptionManager().GetSubscription().ExpireTime;
				double num2 = num - (double)CyberEnvironment.GetUnixTimestamp();
				checked
				{
					int num3 = (int)Math.Ceiling(num2 / 86400.0);
					int i = (int)Math.Ceiling(unchecked((double)CyberEnvironment.GetUnixTimestamp() - (double)client.GetHabbo().GetSubscriptionManager().GetSubscription().ActivateTime) / 86400.0);
					int num4 = num3 / 31;
					if (num4 >= 1)
					{
						num4--;
					}
					serverMessage.AppendInt32(num3 - num4 * 31);
					serverMessage.AppendInt32(1);
					serverMessage.AppendInt32(num4);
					serverMessage.AppendInt32(1);
					serverMessage.AppendBoolean(true);
					serverMessage.AppendBoolean(true);
					serverMessage.AppendInt32(i);
					serverMessage.AppendInt32(i);
					serverMessage.AppendInt32(10);
				}
			}
			else
			{
				serverMessage.AppendInt32(0);
				serverMessage.AppendInt32(0);
				serverMessage.AppendInt32(0);
				serverMessage.AppendInt32(0);
				serverMessage.AppendBoolean(false);
				serverMessage.AppendBoolean(false);
				serverMessage.AppendInt32(0);
				serverMessage.AppendInt32(0);
				serverMessage.AppendInt32(0);
			}
			client.SendMessage(serverMessage);
			ServerMessage serverMessage2 = new ServerMessage(Outgoing.UserClubRightsMessageComposer);
			serverMessage2.AppendInt32(this.GetSubscriptionManager().HasSubscription ? 2 : 0);
			serverMessage2.AppendUInt(this.Rank);
			serverMessage2.AppendInt32(0);
			client.SendMessage(serverMessage2);
		}
		internal void LoadAchievements(Dictionary<string, UserAchievement> achievements)
		{
			this.Achievements = achievements;
		}
		internal void LoadTalents(Dictionary<int, UserTalent> Talents)
		{
			this.Talents = Talents;
		}
		internal void OnDisconnect()
		{
			if (this.Disconnected)
			{
				return;
			}
			this.Disconnected = true;
			if (this.InventoryComponent != null)
			{
				this.InventoryComponent.RunDBUpdate();
				this.InventoryComponent.SetIdleState();
			}
			CyberEnvironment.GetGame().GetClientManager().UnregisterClient(this.Id, this.Username);
			SessionManagement.IncreaseDisconnection();
			Logging.WriteLine("[UserMgr] [ " + this.Username + " ] got disconnected.", ConsoleColor.DarkRed);
			if (!this.HabboinfoSaved)
			{
				this.HabboinfoSaved = true;
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.runFastQuery(string.Concat(new object[]
					{
						"UPDATE users SET activity_points = ",
						this.ActivityPoints,
						", credits = ",
						this.Credits,
						", seasonal_currency = ",
						this.BelCredits,
						",online='0' WHERE id = ",
						this.Id,
						" LIMIT 1;UPDATE user_stats SET achievement_score=",
						this.AchievementPoints,
						" WHERE id=",
						this.Id,
						" LIMIT 1;"
					}));
					if (this.Rank >= 4u)
					{
						queryreactor.runFastQuery("UPDATE moderation_tickets SET status='open', moderator_id=0 WHERE status='picked' AND moderator_id=" + this.Id);
					}
				}
			}
			if (this.InRoom && this.CurrentRoom != null)
			{
				this.CurrentRoom.GetRoomUserManager().RemoveUserFromRoom(this.mClient, false, false);
			}
			if (this.Messenger != null)
			{
				this.Messenger.AppearOffline = true;
				this.Messenger.Destroy();
			}
			if (this.AppearOffline)
			{
				this.Messenger.AppearOffline = true;
			}
			if (this.AvatarEffectsInventoryComponent != null)
			{
				this.AvatarEffectsInventoryComponent.Dispose();
			}
			this.mClient = null;
		}
		internal void InitMessenger()
		{
			GameClient client = this.GetClient();
			if (client == null)
			{
				return;
			}
			this.Messenger.OnStatusChanged(false);
			client.SendMessage(this.Messenger.SerializeFriends());
			client.SendMessage(this.Messenger.SerializeRequests());
			if (CyberEnvironment.OfflineMessages.ContainsKey(this.Id))
			{
				List<OfflineMessage> list = CyberEnvironment.OfflineMessages[this.Id];
				foreach (OfflineMessage current in list)
				{
					client.SendMessage(this.Messenger.SerializeOfflineMessages(current));
				}
				CyberEnvironment.OfflineMessages.Remove(this.Id);
				OfflineMessage.RemoveAllMessages(CyberEnvironment.GetDatabaseManager().getQueryReactor(), this.Id);
			}
			if ((long)this.Messenger.requests.Count > (long)((ulong)CyberEnvironment.FriendRequestLimit))
			{
				client.SendNotif("¡Tienes muchos amigos! No puedes tener más.");
			}
		}
		internal void UpdateCreditsBalance()
		{
			if (this.mClient != null && this.mClient.GetMessageHandler() != null && this.mClient.GetMessageHandler().GetResponse() != null)
			{
				this.mClient.GetMessageHandler().GetResponse().Init(Outgoing.CreditsBalanceMessageComposer);
				this.mClient.GetMessageHandler().GetResponse().AppendString(this.Credits + ".0");
				this.mClient.GetMessageHandler().SendResponse();
			}
		}
		internal void UpdateActivityPointsBalance()
		{
			if (this.mClient == null || this.mClient.GetMessageHandler() == null || this.mClient.GetMessageHandler().GetResponse() == null)
			{
				return;
			}
			this.mClient.GetMessageHandler().GetResponse().Init(Outgoing.ActivityPointsMessageComposer);
			this.mClient.GetMessageHandler().GetResponse().AppendInt32(3);
			this.mClient.GetMessageHandler().GetResponse().AppendInt32(0);
			this.mClient.GetMessageHandler().GetResponse().AppendInt32(this.ActivityPoints);
			this.mClient.GetMessageHandler().GetResponse().AppendInt32(5);
			this.mClient.GetMessageHandler().GetResponse().AppendInt32(this.BelCredits);
            this.mClient.GetMessageHandler().GetResponse().AppendInt32(105);
            this.mClient.GetMessageHandler().GetResponse().AppendInt32(this.BelCredits);
			this.mClient.GetMessageHandler().SendResponse();
		}
		internal void UpdateSeasonalCurrencyBalance()
		{
			if (this.mClient == null || this.mClient.GetMessageHandler() == null || this.mClient.GetMessageHandler().GetResponse() == null)
			{
				return;
			}
			this.mClient.GetMessageHandler().GetResponse().Init(Outgoing.ActivityPointsMessageComposer);
			this.mClient.GetMessageHandler().GetResponse().AppendInt32(3);
			this.mClient.GetMessageHandler().GetResponse().AppendInt32(0);
			this.mClient.GetMessageHandler().GetResponse().AppendInt32(this.ActivityPoints);
			this.mClient.GetMessageHandler().GetResponse().AppendInt32(5);
			this.mClient.GetMessageHandler().GetResponse().AppendInt32(this.BelCredits);
            this.mClient.GetMessageHandler().GetResponse().AppendInt32(105);
            this.mClient.GetMessageHandler().GetResponse().AppendInt32(this.BelCredits);
			this.mClient.GetMessageHandler().SendResponse();
		}
		internal void NotifyNewPixels(int Change)
		{
			if (this.mClient == null || this.mClient.GetMessageHandler() == null || this.mClient.GetMessageHandler().GetResponse() == null)
			{
				return;
			}
			this.mClient.GetMessageHandler().GetResponse().Init(Outgoing.ActivityPointsNotificationMessageComposer);
			this.mClient.GetMessageHandler().GetResponse().AppendInt32(this.ActivityPoints);
			this.mClient.GetMessageHandler().GetResponse().AppendInt32(Change);
			this.mClient.GetMessageHandler().GetResponse().AppendInt32(0);
			this.mClient.GetMessageHandler().SendResponse();
		}
        internal void NotifyNewDiamonds(int Change)
        {
            if (this.mClient == null || this.mClient.GetMessageHandler() == null || this.mClient.GetMessageHandler().GetResponse() == null)
            {
                return;
            }

            this.mClient.GetMessageHandler().GetResponse().Init(Outgoing.ActivityPointsNotificationMessageComposer);
            this.mClient.GetMessageHandler().GetResponse().AppendInt32(this.BelCredits);
            this.mClient.GetMessageHandler().GetResponse().AppendInt32(Change);
            this.mClient.GetMessageHandler().GetResponse().AppendInt32(5);
            this.mClient.GetMessageHandler().SendResponse();
        }
		internal void NotifyVoucher(bool isValid, string productName, string productDescription)
		{
			if (isValid)
			{
				this.mClient.GetMessageHandler().GetResponse().Init(Outgoing.VoucherValidMessageComposer);
				this.mClient.GetMessageHandler().GetResponse().AppendString(productName);
				this.mClient.GetMessageHandler().GetResponse().AppendString(productDescription);
				this.mClient.GetMessageHandler().SendResponse();
				return;
			}
			this.mClient.GetMessageHandler().GetResponse().Init(Outgoing.VoucherErrorMessageComposer);
			this.mClient.GetMessageHandler().GetResponse().AppendString("1");
			this.mClient.GetMessageHandler().SendResponse();
		}
		internal void Mute()
		{
			if (!this.Muted)
			{
				this.Muted = true;
			}
		}
		internal void Unmute()
		{
			if (this.Muted)
			{
				this.GetClient().SendNotif("You were unmuted.");
				this.Muted = false;
			}
		}
		private GameClient GetClient()
		{
			return CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(this.Id);
		}
		internal SubscriptionManager GetSubscriptionManager()
		{
			return this.SubscriptionManager;
		}
		internal HabboMessenger GetMessenger()
		{
			return this.Messenger;
		}
		internal BadgeComponent GetBadgeComponent()
		{
			return this.BadgeComponent;
		}
		internal InventoryComponent GetInventoryComponent()
		{
			return this.InventoryComponent;
		}
		internal AvatarEffectsInventoryComponent GetAvatarEffectsInventoryComponent()
		{
			return this.AvatarEffectsInventoryComponent;
		}
		internal void RunDBUpdate(IQueryAdapter dbClient)
		{
			dbClient.runFastQuery(string.Concat(new object[]
			{
				"UPDATE users SET last_online = '",
				CyberEnvironment.GetUnixTimestamp(),
				"', activity_points = '",
				this.ActivityPoints,
				"', credits = '",
				this.Credits,
				"', seasonal_currency = '",
				this.BelCredits,
				"' WHERE id = '",
				this.Id,
				"' LIMIT 1; "
			}));
		}
		internal int GetQuestProgress(uint p)
		{
			int result = 0;
			this.quests.TryGetValue(p, out result);
			return result;
		}
		internal UserAchievement GetAchievementData(string p)
		{
			UserAchievement result = null;
			this.Achievements.TryGetValue(p, out result);
			return result;
		}
		internal UserTalent GetTalentData(int t)
		{
			UserTalent result = null;
			this.Talents.TryGetValue(t, out result);
			return result;
		}
		internal int GetCurrentTalentLevel()
		{
			int num = 1;
			foreach (UserTalent current in this.Talents.Values)
			{
				int level = CyberEnvironment.GetGame().GetTalentManager().GetTalent(current.TalentId).Level;
				if (level > num)
				{
					num = level;
				}
			}
			return num;
		}
		internal bool GotPollData(uint PollID)
		{
            return (AnsweredPolls.Contains(PollID));
		}
		internal bool CheckTrading()
		{
			if (!this.TradeLocked)
			{
				return true;
			}
			if (this.TradeLockExpire < CyberEnvironment.GetUnixTimestamp())
			{
				return false;
			}
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery("UPDATE users SET trade_lock = '0' WHERE id = " + this.Id);
			}
			this.TradeLocked = false;
			return true;
		}
	}
}
