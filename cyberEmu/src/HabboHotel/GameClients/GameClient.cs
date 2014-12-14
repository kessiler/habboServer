using ConnectionManager;
using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.Core;
using Cyber.HabboHotel.Misc;
using Cyber.HabboHotel.Rooms;
using Cyber.HabboHotel.Users;
using Cyber.HabboHotel.Users.UserDataManagement;
using Cyber.HabboHotel.Achievements;
using Cyber.Messages;
using Cyber.Messages.Headers;
using Cyber.Net;
using System;
using System.Threading.Tasks;
using HabboEncryption.Hurlant.Crypto.Prng;

namespace Cyber.HabboHotel.GameClients
{
	public class GameClient
	{
		private uint Id;
		internal byte PublicistaCount;
		private ConnectionInformation Connection;
		private GameClientMessageHandler MessageHandler;
		private Habbo Habbo;
		internal DateTime TimePingedReceived;
		internal GamePacketParser packetParser;
		internal int DesignedHandler = 1;
		internal int CurrentRoomUserID;
		internal string MachineId;
		private bool Disconnected;
		internal uint ConnectionID
		{
			get
			{
				return this.Id;
			}
		}


        internal ARC4 ARC4
        {
            get;
            set;
        }

		internal GameClient(uint ClientId, ConnectionInformation pConnection)
		{
			this.Id = ClientId;
			this.Connection = pConnection;
			this.CurrentRoomUserID = -1;
			this.packetParser = new GamePacketParser(this);
            this.Connection.SetClient(this);
		}
		private void SwitchParserRequest()
		{
			if (this.MessageHandler == null)
			{
				this.InitHandler();
			}
			this.packetParser.SetConnection(this.Connection);
			this.packetParser.onNewPacket += new GamePacketParser.HandlePacket(this.parser_onNewPacket);
			byte[] currentData = (this.Connection.parser as InitialPacketParser).currentData;
			this.Connection.parser.Dispose();
			this.Connection.parser = this.packetParser;
			this.Connection.parser.handlePacketData(currentData);
		}
		private void parser_onNewPacket(ClientMessage Message)
		{
			try
			{
                this.MessageHandler.HandleRequest(Message);
			}
			catch (Exception ex)
			{
				Logging.LogPacketException(Message.ToString(), ex.ToString());
			}
		}
		private void PolicyRequest()
		{
			this.Connection.SendData(CyberEnvironment.GetDefaultEncoding().GetBytes(CrossdomainPolicy.GetXmlPolicy()));
		}
		internal void HandlePublicista(string Message)
		{
			bool flag = false;
			if (this.PublicistaCount < 2)
			{
				CyberEnvironment.GetGame().GetClientManager().SendSuperNotif("Hey!!", "Please stop advertising other hotels. You will be muted if you do it again.<br /> Need more information? Click the link below.", "frank10", this, "event:", "ok", false, false);
			}
			else if (this.PublicistaCount < 3)
			{
				CyberEnvironment.GetGame().GetClientManager().SendSuperNotif("You have been muted", "Sorry but you were muted by <b>advertising other hotel</b>", "frank10", this, "event:", "ok", false, false);
				this.GetHabbo().Mute();
				flag = true;
			}
            else if (this.PublicistaCount >= 3)
            {
                return;
            }
			ServerMessage serverMessage = new ServerMessage(Outgoing.SuperNotificationMessageComposer);
			serverMessage.AppendString("staffcloud");
			serverMessage.AppendInt32(4);
			serverMessage.AppendString("title");
			serverMessage.AppendString("Possible advertiser found!");
			serverMessage.AppendString("message");
			serverMessage.AppendString(string.Concat(new string[]
			{
				"This user has been detected as advertiser: <b>",
				this.GetHabbo().Username,
				".</b> Is he/she advertising an hotel like this?:<br />\"<b>",
				Message,
				"</b>\".<br /><br />",
				flag ? "<i>The user was automatically muted.</i>" : "The user was automatically warned.</i>"
			}));
            serverMessage.AppendString("link");
            serverMessage.AppendString("event:");
            serverMessage.AppendString("linkTitle");
            serverMessage.AppendString("ok");
			CyberEnvironment.GetGame().GetClientManager().StaffAlert(serverMessage, 0U);
		}
		internal ConnectionInformation GetConnection()
		{
			return this.Connection;
		}
		internal GameClientMessageHandler GetMessageHandler()
		{
			return this.MessageHandler;
		}
		internal Habbo GetHabbo()
		{
			return this.Habbo;
		}
		internal void StartConnection()
		{
			if (this.Connection == null)
			{
				return;
			}
			this.TimePingedReceived = DateTime.Now;
			(this.Connection.parser as InitialPacketParser).PolicyRequest += new InitialPacketParser.NoParamDelegate(this.PolicyRequest);
			(this.Connection.parser as InitialPacketParser).SwitchParserRequest += new InitialPacketParser.NoParamDelegate(this.SwitchParserRequest);
			this.Connection.startPacketProcessing();
		}
		internal void InitHandler()
		{
			this.MessageHandler = new GameClientMessageHandler(this);
		}
		internal bool tryLogin(string AuthTicket)
		{
			try
			{
				string ip = this.GetConnection().getIp();
				byte b = 0;
				UserData userData = UserDataFactory.GetUserData(AuthTicket, ip, out b);
				bool result;
				if (b == 1)
				{
					result = false;
					return result;
				}
				if (b == 2)
				{
					result = false;
					return result;
				}
				CyberEnvironment.GetGame().GetClientManager().RegisterClient(this, userData.userID, userData.user.Username);
				this.Habbo = userData.user;
				userData.user.LoadData(userData);
				string banReason = CyberEnvironment.GetGame().GetBanManager().GetBanReason(userData.user.Username, ip, this.MachineId);
				if (!string.IsNullOrEmpty(banReason) || userData.user.Username == null)
				{
					this.SendNotifWithScroll(banReason);
					using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
					{
						queryreactor.setQuery("SELECT ip_last FROM users WHERE id=" + this.GetHabbo().Id + " LIMIT 1");
						string @string = queryreactor.getString();
						queryreactor.setQuery("SELECT COUNT(0) FROM bans_access WHERE user_id=" + this.Habbo.Id + " LIMIT 1");
						int integer = queryreactor.getInteger();
						if (integer > 0)
						{
							queryreactor.runFastQuery(string.Concat(new object[]
							{
								"UPDATE bans_access SET attempts = attempts + 1, ip='",
								@string,
								"' WHERE user_id=",
								this.GetHabbo().Id,
								" LIMIT 1"
							}));
						}
						else
						{
							queryreactor.runFastQuery(string.Concat(new object[]
							{
								"INSERT INTO bans_access (user_id, ip) VALUES (",
								this.GetHabbo().Id,
								", '",
								@string,
								"')"
							}));
						}
					}
					result = false;
					return result;
				}
				userData.user.Init(this, userData);
				QueuedServerMessage queuedServerMessage = new QueuedServerMessage(this.Connection);
				ServerMessage serverMessage = new ServerMessage(Outgoing.UniqueMachineIDMessageComposer);
				serverMessage.AppendString(this.MachineId);
				queuedServerMessage.appendResponse(serverMessage);
				queuedServerMessage.appendResponse(new ServerMessage(Outgoing.AuthenticationOKMessageComposer));
				if (this.Habbo != null)
				{
					ServerMessage serverMessage2 = new ServerMessage(Outgoing.HomeRoomMessageComposer);
					serverMessage2.AppendUInt(this.Habbo.HomeRoom);
					serverMessage2.AppendUInt(this.Habbo.HomeRoom);
					queuedServerMessage.appendResponse(serverMessage2);
				}
				ServerMessage serverMessage3 = new ServerMessage(Outgoing.MinimailCountMessageComposer);
				serverMessage3.AppendInt32(this.Habbo.MinimailUnreadMessages);
				queuedServerMessage.appendResponse(serverMessage3);

				ServerMessage serverMessage4 = new ServerMessage(Outgoing.FavouriteRoomsMessageComposer);
				serverMessage4.AppendInt32(30);
				serverMessage4.AppendInt32(userData.user.FavoriteRooms.Count);
				object[] array = userData.user.FavoriteRooms.ToArray();
				for (int i = 0; i < array.Length; i++)
				{
					uint i2 = (uint)array[i];
					serverMessage4.AppendUInt(i2);
				}
				queuedServerMessage.appendResponse(serverMessage4);
				

                ServerMessage rightsMessage = new ServerMessage(Outgoing.UserClubRightsMessageComposer);
                rightsMessage.AppendInt32(userData.user.GetSubscriptionManager().HasSubscription ? 2 : 0);
                rightsMessage.AppendUInt(userData.user.Rank);
                rightsMessage.AppendInt32(0);


                queuedServerMessage.appendResponse(rightsMessage);
				ServerMessage serverMessage5 = new ServerMessage(Outgoing.EnableNotificationsMessageComposer);
				serverMessage5.AppendBoolean(true);
				serverMessage5.AppendBoolean(false);
				queuedServerMessage.appendResponse(serverMessage5);
				ServerMessage serverMessage6 = new ServerMessage(Outgoing.EnableTradingMessageComposer);
				serverMessage6.AppendBoolean(true);
				queuedServerMessage.appendResponse(serverMessage6);
				userData.user.UpdateCreditsBalance();
				ServerMessage serverMessage7 = new ServerMessage(Outgoing.ActivityPointsMessageComposer);
				serverMessage7.AppendInt32(2);
				serverMessage7.AppendInt32(0);
				serverMessage7.AppendInt32(userData.user.ActivityPoints);
				serverMessage7.AppendInt32(5);
				serverMessage7.AppendInt32(userData.user.BelCredits);
				queuedServerMessage.appendResponse(serverMessage7);
				if (userData.user.HasFuse("fuse_mod"))
				{
					queuedServerMessage.appendResponse(CyberEnvironment.GetGame().GetModerationTool().SerializeTool());
				}
				if (!string.IsNullOrWhiteSpace(CyberEnvironment.GetDBConfig().DBData["welcome_message"]))
				{
					this.SendBroadcastMessage(CyberEnvironment.GetDBConfig().DBData["welcome_message"]);
				}

                ServerMessage AchievementData = new ServerMessage(Outgoing.SendAchievementsRequirementsMessageComposer);

                AchievementData.AppendInt32(CyberEnvironment.GetGame().GetAchievementManager().Achievements.Count);
                foreach (Achievement Ach in CyberEnvironment.GetGame().GetAchievementManager().Achievements.Values)
                {
                    AchievementData.AppendString(Ach.GroupName.Replace("ACH_", ""));
                    AchievementData.AppendInt32(Ach.Levels.Count);

                    for (int i = 1; i < Ach.Levels.Count + 1; i++)
                    {
                        AchievementData.AppendInt32(i);
                        AchievementData.AppendInt32(Ach.Levels[i].Requirement);
                    }
                }
                AchievementData.AppendInt32(0);
                queuedServerMessage.appendResponse(AchievementData);

                if (!GetHabbo().NUXPassed && ExtraSettings.NEW_USER_GIFTS_ENABLED)
                {
                    queuedServerMessage.appendResponse(new ServerMessage(Outgoing.NuxSuggestFreeGiftsMessageComposer));
                }

                queuedServerMessage.appendResponse(this.GetHabbo().GetAvatarEffectsInventoryComponent().GetPacket());

				queuedServerMessage.sendResponse();
				result = true;
				return result;
			}
			catch (Exception ex)
			{
				Logging.LogCriticalException("Bug during user login: " + ex.Message);
			}
			return false;
		}
		internal void SendNotifWithScroll(string message)
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.MOTDNotificationMessageComposer);
			serverMessage.AppendInt32(1);
			serverMessage.AppendString(message);
			this.SendMessage(serverMessage);
		}
		internal void SendBroadcastMessage(string Message)
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.BroadcastNotifMessageComposer);
			serverMessage.AppendString(Message);
			serverMessage.AppendString("");
			this.SendMessage(serverMessage);
		}
		internal void SendWhisper(string Message)
		{
			if (this.GetHabbo().CurrentRoom == null)
			{
				return;
			}
			RoomUser roomUserByHabbo = this.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.GetHabbo().Username);
			if (roomUserByHabbo == null)
			{
				return;
			}

            ServerMessage Whisp = new ServerMessage(Outgoing.WhisperMessageComposer);
            Whisp.AppendInt32(roomUserByHabbo.VirtualId);
            Whisp.AppendString(Message);
            Whisp.AppendInt32(0);
            Whisp.AppendInt32(roomUserByHabbo.LastBubble);
            Whisp.AppendInt32(0);
            Whisp.AppendInt32(0);

			this.SendMessage(Whisp);
		}
		internal void SendNotif(string Message, string Title="Notification")
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.SuperNotificationMessageComposer);
			serverMessage.AppendString("admin");
			serverMessage.AppendInt32(4);
            serverMessage.AppendString("title");
            serverMessage.AppendString(Title);
			serverMessage.AppendString("message");
			serverMessage.AppendString(Message);
            serverMessage.AppendString("linkUrl");
            serverMessage.AppendString("event:");
            serverMessage.AppendString("linkTitle");
            serverMessage.AppendString("ok");
			this.SendMessage(serverMessage);
		}
		internal void Stop()
		{
			if (this.GetMessageHandler() != null)
			{
				this.MessageHandler.Destroy();
			}
			if (this.GetHabbo() != null)
			{
				this.Habbo.OnDisconnect();
			}
			this.CurrentRoomUserID = -1;
			this.MessageHandler = null;
			this.Habbo = null;
			this.Connection = null;
		}
		internal void Disconnect()
		{
			if (this.GetHabbo() != null)
			{
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.runFastQuery(this.GetHabbo().GetQueryString);
				}
				this.GetHabbo().OnDisconnect();
			}
			if (!this.Disconnected)
			{
				if (this.Connection != null)
				{
					this.Connection.Dispose();
				}
				this.Disconnected = true;
			}
		}
		internal void SendMessage(ServerMessage Message)
		{
			byte[] bytes = Message.GetBytes();
			if (Message == null)
			{
				return;
			}
			if (this.GetConnection() == null)
			{
				return;
			}
			this.GetConnection().SendData(bytes);
		}
	}
}
