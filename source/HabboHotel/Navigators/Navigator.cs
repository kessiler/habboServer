using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Rooms;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Collections.Specialized;

namespace Cyber.HabboHotel.Navigators
{
	internal sealed class Navigator
	{
		internal HybridDictionary PrivateCategories;
		private Dictionary<int, PublicItem> PublicItems;
		internal Navigator()
		{
			this.PrivateCategories = new HybridDictionary();
			this.PublicItems = new Dictionary<int, PublicItem>();
		}
        public void Initialize(IQueryAdapter dbClient, out uint navLoaded)
        {
            Initialize(dbClient);
            navLoaded = (uint)PublicItems.Count;
        }
		public void Initialize(IQueryAdapter dbClient)
		{
			dbClient.setQuery("SELECT id,caption,min_rank FROM navigator_flatcats WHERE enabled = 2");
			DataTable table = dbClient.getTable();
			dbClient.setQuery("SELECT * FROM navigator_publics ORDER BY ordernum ASC");
			DataTable table2 = dbClient.getTable();
			if (table != null)
			{
				this.PrivateCategories.Clear();
				foreach (DataRow dataRow in table.Rows)
				{
					this.PrivateCategories.Add((int)dataRow["id"], new FlatCat((int)dataRow["id"], (string)dataRow["caption"], (int)dataRow["min_rank"]));
				}
			}
			if (table2 != null)
			{
				this.PublicItems.Clear();
				foreach (DataRow dataRow2 in table2.Rows)
				{
					this.PublicItems.Add((int)dataRow2["id"], new PublicItem((int)dataRow2["id"], int.Parse(dataRow2["bannertype"].ToString()), (string)dataRow2["caption"], (string)dataRow2["description"], (string)dataRow2["image"], (dataRow2["image_type"].ToString().ToLower() == "internal") ? PublicImageType.INTERNAL : PublicImageType.EXTERNAL, Convert.ToUInt32(dataRow2["room_id"]), (int)dataRow2["category_id"], (int)dataRow2["category_parent_id"], CyberEnvironment.EnumToBool(dataRow2["recommended"].ToString()), (int)dataRow2["typeofdata"], (string)dataRow2["tag"]));
				}
			}
		}
		internal FlatCat GetFlatCat(int Id)
		{
            if (this.PrivateCategories.Contains(Id))
			{
				return (FlatCat)this.PrivateCategories[Id];
			}
			return null;
		}

        internal ServerMessage SerializeNewFlatCategories()
        {
            var flatcat = CyberEnvironment.GetGame().GetNavigator().PrivateCategories.OfType<FlatCat>();
            var rooms = CyberEnvironment.GetGame().GetRoomManager().loadedRooms;

            ServerMessage Message = new ServerMessage(Outgoing.NavigatorNewFlatCategoriesMessageComposer);
            Message.AppendInt32(flatcat.Count());

            foreach (FlatCat cat in flatcat)
            {
                Message.AppendInt32(cat.Id);
                Message.AppendInt32(cat.UsersNow);
                Message.AppendInt32(500);
            }
            
            return Message;
        }

		internal ServerMessage SerializeFlatCategories(GameClient Session)
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.FlatCategoriesMessageComposer);
			serverMessage.AppendInt32(this.PrivateCategories.Count);
			foreach (FlatCat flatCat in this.PrivateCategories.Values)
			{
				serverMessage.AppendInt32(flatCat.Id);
				serverMessage.AppendString(flatCat.Caption);
				serverMessage.AppendBoolean(flatCat.MinRank <= Session.GetHabbo().Rank);
                serverMessage.AppendBoolean(false); // New Build Fix by Finn
                serverMessage.AppendString("NONE"); // New Build Fix by Finn
                serverMessage.AppendString(""); // New Build Fix by Finn
			}
			return serverMessage;
		}

		internal ServerMessage SerializePublicRooms()
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.OfficialRoomsMessageComposer);
			serverMessage.AppendInt32(this.PublicItems.Count);
			foreach (PublicItem current in this.PublicItems.Values)
			{
				if (current.ParentId <= 0)
				{
					current.Serialize(serverMessage);
					if (current.itemType == PublicItemType.CATEGORY)
					{
                        foreach (PublicItem current2 in this.PublicItems.Values.Where(X => X.ParentId == current.Id))
                        {
                            current2.Serialize(serverMessage);
                        }
					}
				}
			}
			if (this.PublicItems.Count > 0)
			{
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					serverMessage.AppendInt32(1);
					queryreactor.setQuery("SELECT id FROM navigator_publics WHERE recommended = '1'");
					queryreactor.runQuery();
					int key = Convert.ToInt32(queryreactor.getInteger());
					this.PublicItems[key].Serialize(serverMessage);
					goto IL_118;
				}
			}
			serverMessage.AppendInt32(0);
			IL_118:
			serverMessage.AppendInt32(0);
			return serverMessage;
		}
		private void SerializeItemsFromCata(int Id, ServerMessage Message)
		{
			foreach (PublicItem current in this.PublicItems.Values)
			{
				if (current.ParentId == Id)
				{
					current.Serialize(Message);
				}
			}
		}
		internal ServerMessage SerializeFavoriteRooms(GameClient Session)
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.NavigatorListingsMessageComposer);
			serverMessage.AppendInt32(6);
			serverMessage.AppendString("");
			serverMessage.AppendInt32(Session.GetHabbo().FavoriteRooms.Count);
			object[] array = Session.GetHabbo().FavoriteRooms.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				uint roomId = (uint)array[i];
				RoomData roomData = CyberEnvironment.GetGame().GetRoomManager().GenerateRoomData(roomId);
				if (roomData != null)
				{
					roomData.Serialize(serverMessage, false);
				}
			}
			serverMessage.AppendBoolean(false);
			return serverMessage;
		}
		internal ServerMessage SerializeRecentRooms(GameClient Session)
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.NavigatorListingsMessageComposer);
			serverMessage.AppendInt32(7);
			serverMessage.AppendString("");

			serverMessage.AppendInt32(Session.GetHabbo().RecentlyVisitedRooms.Count);
			foreach (uint current in Session.GetHabbo().RecentlyVisitedRooms)
			{
				RoomData roomData = CyberEnvironment.GetGame().GetRoomManager().GenerateRoomData(current);
				roomData.Serialize(serverMessage, false);
			}

			serverMessage.AppendBoolean(false);
			return serverMessage;
		}
		internal ServerMessage SerializeEventListing()
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.NavigatorListingsMessageComposer);
			serverMessage.AppendInt32(16);
			serverMessage.AppendString("");
			KeyValuePair<RoomData, int>[] eventRooms = CyberEnvironment.GetGame().GetRoomManager().GetEventRooms();
			serverMessage.AppendInt32(eventRooms.Length);
			KeyValuePair<RoomData, int>[] array = eventRooms;
			for (int i = 0; i < array.Length; i++)
			{
				KeyValuePair<RoomData, int> keyValuePair = array[i];
				keyValuePair.Key.Serialize(serverMessage, true);
			}
			return serverMessage;
		}

        internal bool RoomIsPublicItem(uint RoomId)
        {
           /* foreach (PublicItem Item in PublicItems.Values)
            {
                if (Item.RoomId == RoomId)
                {
                    return true;
                }
            }*/
            return false;
        }
		internal ServerMessage SerializePopularRoomTags()
		{
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			ServerMessage result;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT tags, users_now FROM rooms WHERE roomtype = 'private' AND users_now > 0 ORDER BY users_now DESC LIMIT 50");
				DataTable table = queryreactor.getTable();
				if (table != null)
				{
					foreach (DataRow dataRow in table.Rows)
					{
						int num;
						if (!string.IsNullOrEmpty(dataRow["users_now"].ToString()))
						{
							num = (int)dataRow["users_now"];
						}
						else
						{
							num = 0;
						}
						List<string> list = new List<string>();
						string[] array = dataRow["tags"].ToString().Split(new char[]
						{
							','
						});
						for (int i = 0; i < array.Length; i++)
						{
							string item = array[i];
							list.Add(item);
						}
						foreach (string current in list)
						{
							if (dictionary.ContainsKey(current))
							{
								Dictionary<string, int> dictionary2;
								string key;
								(dictionary2 = dictionary)[key = current] = checked(dictionary2[key] + num);
							}
							else
							{
								dictionary.Add(current, num);
							}
						}
					}
				}
				List<KeyValuePair<string, int>> list2 = new List<KeyValuePair<string, int>>(dictionary);
				list2.Sort((KeyValuePair<string, int> firstPair, KeyValuePair<string, int> nextPair) => firstPair.Value.CompareTo(nextPair.Value));
				ServerMessage serverMessage = new ServerMessage(Outgoing.PopularRoomTagsMessageComposer);
				serverMessage.AppendInt32(list2.Count);
				foreach (KeyValuePair<string, int> current2 in list2)
				{
					serverMessage.AppendString(current2.Key);
					serverMessage.AppendInt32(current2.Value);
				}
				result = serverMessage;
			}
			return result;
		}
		internal ServerMessage SerializeSearchResults(string SearchQuery)
		{
			DataTable dataTable = new DataTable();
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				if (SearchQuery.Length > 0)
				{
					if (SearchQuery.ToLower().StartsWith("owner:"))
					{
						queryreactor.setQuery("SELECT * FROM rooms WHERE owner = @query AND roomtype = 'private' ORDER BY users_now DESC LIMIT 50");
						queryreactor.addParameter("query", SearchQuery.Remove(0, 6));
					}
                    else if (SearchQuery.ToLower().StartsWith("roomname:"))
                    {
                        queryreactor.setQuery("SELECT * FROM rooms WHERE caption LIKE @query AND roomtype = 'private' ORDER BY users_now DESC LIMIT 50");
                        queryreactor.addParameter("query", SearchQuery.Remove(0, 9));
                    }
                    else if (SearchQuery.ToLower().StartsWith("tag:"))
                    {
                        queryreactor.setQuery("SELECT * FROM rooms WHERE tags LIKE @query AND roomtype = 'private' ORDER BY users_now DESC LIMIT 50");
                        queryreactor.addParameter("query", "%" + SearchQuery.Remove(0, 4) + "%");
                    }
					else
					{
						queryreactor.setQuery("SELECT * FROM rooms WHERE caption LIKE @query OR owner LIKE @query AND roomtype = 'private' ORDER BY users_now DESC LIMIT 50");
						queryreactor.addParameter("query", "%" + SearchQuery + "%");
					}
					dataTable = queryreactor.getTable();
				}
			}
			List<RoomData> list = new List<RoomData>();
			if (dataTable != null)
			{
				foreach (DataRow dataRow in dataTable.Rows)
				{
					RoomData item = CyberEnvironment.GetGame().GetRoomManager().FetchRoomData(Convert.ToUInt32(dataRow["id"]), dataRow);
					list.Add(item);
				}
			}
			ServerMessage serverMessage = new ServerMessage(Outgoing.NavigatorListingsMessageComposer);
			serverMessage.AppendInt32(8);
			serverMessage.AppendString(SearchQuery);
			serverMessage.AppendInt32(list.Count);
			foreach (RoomData current in list)
			{
				current.Serialize(serverMessage, false);
			}
			serverMessage.AppendBoolean(false);
			return serverMessage;
		}
		private ServerMessage SerializeActiveRooms(int category)
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.NavigatorListingsMessageComposer);
			serverMessage.AppendInt32(1);
			serverMessage.AppendString(category.ToString());
			HashSet<RoomData> activeRooms = CyberEnvironment.GetGame().GetRoomManager().GetActiveRooms();
			this.SerializeNavigatorPopularRooms(ref serverMessage, activeRooms, category);
            activeRooms = null;
			return serverMessage;
		}
		internal ServerMessage SerializeNavigator(GameClient session, int mode)
		{
			if (mode >= 0)
			{
				return this.SerializeActiveRooms(mode);
			}
			ServerMessage serverMessage = new ServerMessage(Outgoing.NavigatorListingsMessageComposer);
			switch (mode)
			{
			case -16:
			{
				Dictionary<uint, RoomEvent> events = CyberEnvironment.GetGame().GetRoomEvents().GetEvents();
				serverMessage.AppendInt32(16);
				this.SerializeNavigatorRooms(ref serverMessage, events);
				return serverMessage;
			}
			case -15:
				break;
			case -14:
			{
				serverMessage.AppendInt32(14);
				HashSet<RoomData> rooms = CyberEnvironment.GetGame().GetRoomManager().GetActiveRooms();
				this.SerializeNavigatorRooms(ref serverMessage, rooms);
				return serverMessage;
			}
			default:
				switch (mode)
				{
				case -5:
				case -4:
				{
					serverMessage.AppendInt32(checked(mode * -1));
                    HashSet<RoomData> rooms2 = session.GetHabbo().GetMessenger().GetActiveFriendsRooms();
					this.SerializeNavigatorRooms(ref serverMessage, rooms2);
					return serverMessage;
				}
				case -3:
					serverMessage.AppendInt32(5);
					this.SerializeNavigatorRooms(ref serverMessage, session.GetHabbo().UsersRooms);
					return serverMessage;
				case -2:
				{
					serverMessage.AppendInt32(2);
					HashSet<RoomData> votedRooms = CyberEnvironment.GetGame().GetRoomManager().GetVotedRooms();
					this.SerializeNavigatorRooms(ref serverMessage, votedRooms);
                    votedRooms = null;
					return serverMessage;
				}
				case -1:
					serverMessage.AppendInt32(1);
					serverMessage.AppendString("-1");
					try
					{
						HashSet<RoomData> activeRooms = CyberEnvironment.GetGame().GetRoomManager().GetActiveRooms();
						this.SerializeNavigatorPopularRooms(ref serverMessage, activeRooms);
                        activeRooms = null;
					}
					catch
					{
						serverMessage.AppendInt32(0);
						serverMessage.AppendBoolean(false);
					}
					return serverMessage;
				}
				break;
			}
			return serverMessage;
		}
        private void SerializeNavigatorPopularRooms(ref ServerMessage reply, HashSet<RoomData> ALLrooms, int Category)
        {
            var rooms = new HashSet<RoomData>(ALLrooms.Where(X => X.Category == Category));
            reply.AppendInt32(rooms.Count);

            foreach (RoomData key in rooms)
            {
                key.Serialize(reply, false);
            }
            reply.AppendInt32(0);
            reply.AppendInt32(0);
            reply.AppendBoolean(false);
            rooms = null;
        }
		private void SerializeNavigatorPopularRooms(ref ServerMessage reply, HashSet<RoomData> rooms)
		{
			reply.AppendInt32(rooms.Count);
            foreach (RoomData data in rooms)
            {
                data.Serialize(reply, false);
            }
			reply.AppendBoolean(false);
		}
		private void SerializeNavigatorRooms(ref ServerMessage reply, HashSet<RoomData> rooms)
		{
			reply.AppendString("");
			reply.AppendInt32(rooms.Count);
			foreach (RoomData current in rooms)
			{
				current.Serialize(reply, false);
			}
			reply.AppendBoolean(false);
		}
		private void SerializeNavigatorRooms(ref ServerMessage reply, Dictionary<uint, RoomEvent> Events)
		{
			reply.AppendString("");
			reply.AppendInt32(Events.Count);
			foreach (RoomEvent current in Events.Values)
			{
				RoomData roomData = CyberEnvironment.GetGame().GetRoomManager().GenerateRoomData(current.RoomId);
				roomData.Serialize(reply, false);
			}
			reply.AppendBoolean(false);
		}
		private void SerializeNavigatorRooms(ref ServerMessage reply, KeyValuePair<RoomData, int>[] rooms)
		{
			reply.AppendString(string.Empty);
			reply.AppendInt32(rooms.Length);
			for (int i = 0; i < rooms.Length; i++)
			{
				KeyValuePair<RoomData, int> keyValuePair = rooms[i];
				keyValuePair.Key.Serialize(reply, false);
			}
			reply.AppendBoolean(false);
		}
	}
}
