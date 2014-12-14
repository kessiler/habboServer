using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.Core;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Quests;
using Cyber.HabboHotel.Rooms;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
namespace Cyber.HabboHotel.Users.Messenger
{
	internal class HabboMessenger
	{
		private uint UserId;
		internal Dictionary<uint, MessengerRequest> requests;
		internal Dictionary<uint, MessengerBuddy> friends;
		internal bool AppearOffline;
		internal int myFriends
		{
			get
			{
				return this.friends.Count;
			}
		}
		internal HabboMessenger(uint UserId)
		{
			this.requests = new Dictionary<uint, MessengerRequest>();
			this.friends = new Dictionary<uint, MessengerBuddy>();
			this.UserId = UserId;
		}
		internal void Init(Dictionary<uint, MessengerBuddy> friends, Dictionary<uint, MessengerRequest> requests)
		{
			this.requests = new Dictionary<uint, MessengerRequest>(requests);
			this.friends = new Dictionary<uint, MessengerBuddy>(friends);
		}
		internal void ClearRequests()
		{
			this.requests.Clear();
		}
		internal MessengerRequest GetRequest(uint senderID)
		{
			if (this.requests.ContainsKey(senderID))
			{
				return this.requests[senderID];
			}
			return null;
		}
		internal void Destroy()
		{
			IEnumerable<GameClient> clientsById = CyberEnvironment.GetGame().GetClientManager().GetClientsById(this.friends.Keys);
			foreach (GameClient current in clientsById)
			{
				if (current.GetHabbo() != null && current.GetHabbo().GetMessenger() != null)
				{
					current.GetHabbo().GetMessenger().UpdateFriend(this.UserId, null, true);
				}
			}
		}
		internal void OnStatusChanged(bool notification)
		{
			if (this.friends == null)
			{
				return;
			}
			IEnumerable<GameClient> clientsById = CyberEnvironment.GetGame().GetClientManager().GetClientsById(this.friends.Keys);
			foreach (GameClient current in clientsById)
			{
				if (current != null && current.GetHabbo() != null && current.GetHabbo().GetMessenger() != null)
				{
					current.GetHabbo().GetMessenger().UpdateFriend(this.UserId, current, true);
					this.UpdateFriend(current.GetHabbo().Id, current, notification);
				}
			}
		}
		internal void UpdateFriend(uint userid, GameClient client, bool notification)
		{
			if (this.friends.ContainsKey(userid))
			{
				this.friends[userid].UpdateUser(client);
				if (notification)
				{
					GameClient client2 = this.GetClient();
					if (client2 != null)
					{
						client2.SendMessage(this.SerializeUpdate(this.friends[userid]));
					}
				}
			}
		}
		internal void SerializeMessengerAction(int Type, string Name)
		{
			if (this.GetClient() == null)
			{
				return;
			}
			ServerMessage serverMessage = new ServerMessage();
			serverMessage.Init(Outgoing.ConsoleMessengerActionMessageComposer);
			serverMessage.AppendString(this.GetClient().GetHabbo().Id.ToString());
			serverMessage.AppendInt32(Type);
			serverMessage.AppendString(Name);
			foreach (MessengerBuddy current in this.friends.Values)
			{
				if (current.client != null)
				{
					current.client.SendMessage(serverMessage);
				}
			}
		}
		internal void HandleAllRequests()
		{
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"DELETE FROM messenger_requests WHERE from_id = ",
					this.UserId,
					" OR to_id = ",
					this.UserId
				}));
			}
			this.ClearRequests();
		}
		internal void HandleRequest(uint sender)
		{
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"DELETE FROM messenger_requests WHERE (from_id = ",
					this.UserId,
					" AND to_id = ",
					sender,
					") OR (to_id = ",
					this.UserId,
					" AND from_id = ",
					sender,
					")"
				}));
			}
			this.requests.Remove(sender);
		}
		internal void CreateFriendship(uint friendID)
		{
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"REPLACE INTO messenger_friendships (user_one_id,user_two_id) VALUES (",
					this.UserId,
					",",
					friendID,
					")"
				}));
			}
			this.OnNewFriendship(friendID);
			GameClient clientByUserID = CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(friendID);
			if (clientByUserID != null && clientByUserID.GetHabbo().GetMessenger() != null)
			{
				clientByUserID.GetHabbo().GetMessenger().OnNewFriendship(this.UserId);
			}
		}
		internal void DestroyFriendship(uint friendID)
		{
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery(string.Concat(new object[]
				{
					"DELETE FROM messenger_friendships WHERE (user_one_id = ",
					this.UserId,
					" AND user_two_id = ",
					friendID,
					") OR (user_two_id = ",
					this.UserId,
					" AND user_one_id = ",
					friendID,
					")"
				}));
			}
			this.OnDestroyFriendship(friendID);
			GameClient clientByUserID = CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(friendID);
			if (clientByUserID != null && clientByUserID.GetHabbo().GetMessenger() != null)
			{
				clientByUserID.GetHabbo().GetMessenger().OnDestroyFriendship(this.UserId);
			}
		}
		internal void OnNewFriendship(uint friendID)
		{
			GameClient clientByUserID = CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(friendID);
			MessengerBuddy messengerBuddy;
			if (clientByUserID == null || clientByUserID.GetHabbo() == null)
			{
				DataRow row;
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.setQuery("SELECT id,username,motto,look,last_online,hide_inroom,hide_online FROM users WHERE id = " + friendID);
					row = queryreactor.getRow();
				}
				messengerBuddy = new MessengerBuddy(friendID, (string)row["username"], (string)row["look"], (string)row["motto"], (int)row["last_online"], CyberEnvironment.EnumToBool(row["hide_online"].ToString()), CyberEnvironment.EnumToBool(row["hide_inroom"].ToString()));
			}
			else
			{
				Habbo habbo = clientByUserID.GetHabbo();
				messengerBuddy = new MessengerBuddy(friendID, habbo.Username, habbo.Look, habbo.Motto, 0, habbo.AppearOffline, habbo.HideInRoom);
				messengerBuddy.UpdateUser(clientByUserID);
			}
			if (!this.friends.ContainsKey(friendID))
			{
				this.friends.Add(friendID, messengerBuddy);
			}
			this.GetClient().SendMessage(this.SerializeUpdate(messengerBuddy));
		}
		internal bool RequestExists(uint requestID)
		{
			if (this.requests.ContainsKey(requestID))
			{
				return true;
			}
			checked
			{
				bool result;
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.setQuery("SELECT user_one_id FROM messenger_friendships WHERE user_one_id = @myID AND user_two_id = @friendID");
					queryreactor.addParameter("myID", (int)this.UserId);
					queryreactor.addParameter("friendID", (int)requestID);
					result = queryreactor.findsResult();
				}
				return result;
			}
		}
		internal bool FriendshipExists(uint friendID)
		{
			return this.friends.ContainsKey(friendID);
		}
		internal void OnDestroyFriendship(uint Friend)
		{
			this.friends.Remove(Friend);
			this.GetClient().GetMessageHandler().GetResponse().Init(Outgoing.FriendUpdateMessageComposer);
			this.GetClient().GetMessageHandler().GetResponse().AppendInt32(0);
			this.GetClient().GetMessageHandler().GetResponse().AppendInt32(1);
			this.GetClient().GetMessageHandler().GetResponse().AppendInt32(-1);
			this.GetClient().GetMessageHandler().GetResponse().AppendUInt(Friend);
			this.GetClient().GetMessageHandler().SendResponse();
		}
		internal bool RequestBuddy(string UserQuery)
		{
			GameClient clientByUsername = CyberEnvironment.GetGame().GetClientManager().GetClientByUsername(UserQuery);
			uint num;
			bool flag;
			if (clientByUsername == null)
			{
				DataRow dataRow = null;
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.setQuery("SELECT id,block_newfriends FROM users WHERE username = @query");
					queryreactor.addParameter("query", UserQuery.ToLower());
					dataRow = queryreactor.getRow();
				}
				if (dataRow == null)
				{
					return false;
				}
				num = Convert.ToUInt32(dataRow["id"]);
				flag = CyberEnvironment.EnumToBool(dataRow["block_newfriends"].ToString());
			}
			else
			{
				num = clientByUsername.GetHabbo().Id;
				flag = clientByUsername.GetHabbo().HasFriendRequestsDisabled;
			}
			if (flag && this.GetClient().GetHabbo().Rank < 4u)
			{
				this.GetClient().GetMessageHandler().GetResponse().Init(Outgoing.NotAcceptingRequestsMessageComposer);
				this.GetClient().GetMessageHandler().GetResponse().AppendInt32(39);
				this.GetClient().GetMessageHandler().GetResponse().AppendInt32(3);
				this.GetClient().GetMessageHandler().SendResponse();
				return false;
			}
			uint num2 = num;
			if (this.RequestExists(num2))
			{
				return true;
			}
			using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor2.runFastQuery(string.Concat(new object[]
				{
					"REPLACE INTO messenger_requests (from_id,to_id) VALUES (",
					this.UserId,
					",",
					num2,
					")"
				}));
			}
			CyberEnvironment.GetGame().GetQuestManager().ProgressUserQuest(this.GetClient(), QuestType.ADD_FRIENDS, 0u);
			GameClient clientByUserID = CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(num2);
			if (clientByUserID == null || clientByUserID.GetHabbo() == null)
			{
				return true;
			}
			MessengerRequest messengerRequest = new MessengerRequest(num2, this.UserId, CyberEnvironment.GetGame().GetClientManager().GetNameById(this.UserId));
			clientByUserID.GetHabbo().GetMessenger().OnNewRequest(this.UserId);
			ServerMessage serverMessage = new ServerMessage(Outgoing.ConsoleSendFriendRequestMessageComposer);
			messengerRequest.Serialize(serverMessage);
			clientByUserID.SendMessage(serverMessage);
			this.requests.Add(num2, messengerRequest);
			return true;
		}
		internal void OnNewRequest(uint friendID)
		{
			if (!this.requests.ContainsKey(friendID))
			{
				this.requests.Add(friendID, new MessengerRequest(this.UserId, friendID, CyberEnvironment.GetGame().GetClientManager().GetNameById(friendID)));
			}
		}
		internal void SendInstantMessage(uint ToId, string Message)
		{
			checked
			{
				if (AntiPublicistas.CheckPublicistas(Message))
				{
					GetClient().PublicistaCount += 1;
					this.GetClient().HandlePublicista(Message);
					return;
				}
				if (!this.FriendshipExists(ToId))
				{
					this.DeliverInstantMessageError(6, ToId);
					return;
				}
				GameClient clientByUserID = CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(ToId);
				if (clientByUserID == null || clientByUserID.GetHabbo().GetMessenger() == null)
				{
					if (CyberEnvironment.OfflineMessages.ContainsKey(ToId))
					{
						CyberEnvironment.OfflineMessages[ToId].Add(new OfflineMessage(this.GetClient().GetHabbo().Id, Message, (double)CyberEnvironment.GetUnixTimestamp()));
					}
					else
					{
						CyberEnvironment.OfflineMessages.Add(ToId, new List<OfflineMessage>());
						CyberEnvironment.OfflineMessages[ToId].Add(new OfflineMessage(this.GetClient().GetHabbo().Id, Message, (double)CyberEnvironment.GetUnixTimestamp()));
					}
					OfflineMessage.SaveMessage(CyberEnvironment.GetDatabaseManager().getQueryReactor(), ToId, this.GetClient().GetHabbo().Id, Message);
					return;
				}
				if (this.GetClient().GetHabbo().Muted)
				{
					this.DeliverInstantMessageError(4, ToId);
					return;
				}
				if (clientByUserID.GetHabbo().Muted)
				{
					this.DeliverInstantMessageError(3, ToId);
				}
				if (Message == "")
				{
					return;
				}
				clientByUserID.GetHabbo().GetMessenger().DeliverInstantMessage(Message, this.UserId);
				// CAUSES LAG: this.LogPM(this.UserId, ToId, Message);
			}
		}
		internal void LogPM(uint From_Id, uint ToId, string Message)
		{
			uint arg_10_0 = this.GetClient().GetHabbo().Id;
			DateTime arg_16_0 = DateTime.Now;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery(string.Concat(new object[]
				{
					"INSERT INTO chatlogs_console VALUES (NULL, ",
					From_Id,
					", ",
					ToId,
					", @message, UNIX_TIMESTAMP())"
				}));
				queryreactor.addParameter("message", Message);
				queryreactor.runQuery();
			}
		}
		internal void DeliverInstantMessage(string message, uint convoID)
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.ConsoleChatMessageComposer);
			serverMessage.AppendUInt(convoID);
			serverMessage.AppendString(message);
			serverMessage.AppendInt32(0);
			this.GetClient().SendMessage(serverMessage);
		}
		internal void DeliverInstantMessageError(int ErrorId, uint ConversationId)
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.ConsoleChatErrorMessageComposer);
			serverMessage.AppendInt32(ErrorId);
			serverMessage.AppendUInt(ConversationId);
			serverMessage.AppendString("");
			this.GetClient().SendMessage(serverMessage);
		}
		internal ServerMessage SerializeFriends()
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.LoadFriendsMessageComposer);
			serverMessage.AppendInt32(2000);
			serverMessage.AppendInt32(300);
			serverMessage.AppendInt32(800);
			serverMessage.AppendInt32(1100);
			serverMessage.AppendInt32(0);
			serverMessage.AppendInt32(this.friends.Count);
			foreach (MessengerBuddy current in this.friends.Values)
			{
				current.Serialize(serverMessage, this.GetClient());
			}
			return serverMessage;
		}
		internal ServerMessage SerializeOfflineMessages(OfflineMessage Message)
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.ConsoleChatMessageComposer);
			serverMessage.AppendUInt(Message.FromId);
			serverMessage.AppendString(Message.Message);
			serverMessage.AppendInt32(checked((int)unchecked((double)CyberEnvironment.GetUnixTimestamp() - Message.Timestamp)));
			return serverMessage;
		}
		internal ServerMessage SerializeUpdate(MessengerBuddy friend)
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.FriendUpdateMessageComposer);
			serverMessage.AppendInt32(0);
			serverMessage.AppendInt32(1);
			serverMessage.AppendInt32(0);
			friend.Serialize(serverMessage, this.GetClient());
			serverMessage.AppendBoolean(false);
			return serverMessage;
		}
		internal ServerMessage SerializeRequests()
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.FriendRequestsMessageComposer);
			serverMessage.AppendInt32(((long)this.requests.Count > (long)((ulong)CyberEnvironment.FriendRequestLimit)) ? checked((int)CyberEnvironment.FriendRequestLimit) : this.requests.Count);
			serverMessage.AppendInt32(((long)this.requests.Count > (long)((ulong)CyberEnvironment.FriendRequestLimit)) ? checked((int)CyberEnvironment.FriendRequestLimit) : this.requests.Count);
			int num = 0;
			foreach (MessengerRequest current in this.requests.Values)
			{
				checked
				{
					num++;
				}
				if ((long)num > (long)((ulong)CyberEnvironment.FriendRequestLimit))
				{
					break;
				}
				current.Serialize(serverMessage);
			}
			return serverMessage;
		}
		internal ServerMessage PerformSearch(string query)
		{
			List<SearchResult> searchResult = SearchResultFactory.GetSearchResult(query);
			List<SearchResult> list = new List<SearchResult>();
			List<SearchResult> list2 = new List<SearchResult>();
			foreach (SearchResult current in searchResult)
			{
				if (current.userID != this.GetClient().GetHabbo().Id)
				{
					if (this.FriendshipExists(current.userID))
					{
						list.Add(current);
					}
					else
					{
						list2.Add(current);
					}
				}
			}
			ServerMessage serverMessage = new ServerMessage(Outgoing.ConsoleSearchFriendMessageComposer);
			serverMessage.AppendInt32(list.Count);
			foreach (SearchResult current2 in list)
			{
				current2.Searialize(serverMessage);
			}
			serverMessage.AppendInt32(list2.Count);
			foreach (SearchResult current3 in list2)
			{
				current3.Searialize(serverMessage);
			}
			return serverMessage;
		}
		private GameClient GetClient()
		{
			return CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(this.UserId);
		}
		internal HashSet<RoomData> GetActiveFriendsRooms()
		{
            HashSet<RoomData> toReturn = new HashSet<RoomData>();
			foreach (MessengerBuddy current in 
				from p in this.friends.Values
				where p.InRoom
				select p)
			{
                toReturn.Add(current.CurrentRoom.RoomData);
			}
            return toReturn;
		}
	}
}
