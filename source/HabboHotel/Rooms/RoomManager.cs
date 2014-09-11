using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.Core;
using Cyber.HabboHotel.Events;
using Cyber.HabboHotel.GameClients;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Cyber.HabboHotel.Navigators;
using System.Collections.Specialized;

namespace Cyber.HabboHotel.Rooms
{
	internal class RoomManager
	{
		internal Dictionary<uint, Room> loadedRooms;
		private Queue roomsToAddQueue;
		private Queue roomsToRemoveQueue;
		private Queue roomDataToAddQueue;
		private Queue votedRoomsAddQueue;
		private Queue votedRoomsRemoveQueue;
		private Queue activeRoomsUpdateQueue;
		private Queue activeRoomsAddQueue;
		public Queue activeRoomsRemoveQueue;
		private HybridDictionary roomModels;
		private HybridDictionary loadedRoomData;
		private Dictionary<RoomData, int> votedRooms;
		private HashSet<RoomData> orderedVotedRooms;
		private Dictionary<RoomData, int> activeRooms;
		private HashSet<RoomData> orderedActiveRooms;
		private EventManager eventManager;

		internal int LoadedRoomsCount
		{
			get
			{
				return this.loadedRooms.Count;
			}
		}

		internal HashSet<RoomData> GetActiveRooms()
		{
            return orderedActiveRooms;
		}
		internal HashSet<RoomData> GetVotedRooms()
		{
			return orderedVotedRooms;
		}

		internal RoomModel GetModel(string Model)
		{
            if (this.roomModels.Contains(Model))
			{
				return (RoomModel)this.roomModels[Model];
			}
            else
            {
                LoadNewModel(Model);
                if (this.roomModels.Contains(Model))
                {
                    return (RoomModel)this.roomModels[Model];
                }
            }
			return null;
		}
		internal RoomData GenerateNullableRoomData(uint RoomId)
		{
			if (this.GenerateRoomData(RoomId) != null)
			{
				return this.GenerateRoomData(RoomId);
			}
			RoomData roomData = new RoomData();
			roomData.FillNull(RoomId);
			return roomData;
		}
		internal RoomData GenerateRoomData(uint RoomId)
		{
            if (this.loadedRoomData.Contains(RoomId))
			{
				return (RoomData)this.loadedRoomData[RoomId];
			}
			RoomData roomData = new RoomData();
			if (this.IsRoomLoaded(RoomId))
			{
				return this.GetRoom(RoomId).RoomData;
			}
			DataRow dataRow = null;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT * FROM rooms WHERE id = " + RoomId + " LIMIT 1");
				dataRow = queryreactor.getRow();
			}
			if (dataRow == null)
			{
				return null;
			}
			roomData.Fill(dataRow);
			this.loadedRoomData.Add(RoomId, roomData);
			return roomData;
		}
		internal KeyValuePair<RoomData, int>[] GetEventRooms()
		{
			return this.eventManager.GetRooms();
		}
		internal bool IsRoomLoaded(uint RoomId)
		{
			return this.loadedRooms.ContainsKey(RoomId);
		}
		internal Room LoadRoom(uint Id)
		{
			if (this.IsRoomLoaded(Id))
			{
				return this.GetRoom(Id);
			}
			RoomData roomData = this.GenerateRoomData(Id);
			if (roomData == null)
			{
				return null;
			}
			Room room = new Room(roomData);
            Logging.WriteLine("[RoomMgr] Room #[" + Id + "] was loaded.", ConsoleColor.Blue);
			lock (this.roomsToAddQueue.SyncRoot)
			{
				this.roomsToAddQueue.Enqueue(room);
			}
			room.InitBots();
			room.InitPets();
			return room;
		}
		internal RoomData FetchRoomData(uint RoomId, DataRow dRow)
		{
            if (this.loadedRoomData.Contains(RoomId))
			{
				return (RoomData)this.loadedRoomData[RoomId];
			}
			RoomData roomData = new RoomData();
			if (this.IsRoomLoaded(RoomId))
			{
				roomData.Fill(this.GetRoom(RoomId));
			}
			else
			{
				roomData.Fill(dRow);
			}
			this.loadedRoomData.Add(RoomId, roomData);
			return roomData;
		}
		internal Room GetRoom(uint roomID)
		{
			Room result;
			if (this.loadedRooms.TryGetValue(roomID, out result))
			{
				return result;
			}
			return null;
		}

		internal RoomData CreateRoom(GameClient Session, string Name, string Desc, string Model, int Category, int MaxVisitors, int TradeState)
		{
            if (!this.roomModels.Contains(Model))
			{
				Session.SendNotif("I can't create your room with that model!");
				return null;
			}

			uint RoomId = 0;
			using (IQueryAdapter dbClient = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				dbClient.setQuery("INSERT INTO rooms (roomtype,caption,description,owner,model_name,category,users_max,trade_state) VALUES ('private',@caption,@desc,@username,@model,@cat,@usmax,@tstate)");
				dbClient.addParameter("caption", Name);
				dbClient.addParameter("desc", Desc);
				dbClient.addParameter("username", Session.GetHabbo().Username);
				dbClient.addParameter("model", Model);
				dbClient.addParameter("cat", Category);
				dbClient.addParameter("usmax", MaxVisitors);
				dbClient.addParameter("tstate", TradeState.ToString());
				RoomId = (uint)dbClient.insertQuery();
			}
			RoomData Data = this.GenerateRoomData(RoomId);
			Session.GetHabbo().UsersRooms.Add(Data);
			return Data;
		}

		internal RoomManager()
		{
			this.loadedRooms = new Dictionary<uint, Room>();
			this.roomModels = new HybridDictionary();
			this.loadedRoomData = new HybridDictionary();
			this.votedRooms = new Dictionary<RoomData, int>();
			this.activeRooms = new Dictionary<RoomData, int>();
			this.roomsToAddQueue = new Queue();
			this.roomsToRemoveQueue = new Queue();
			this.roomDataToAddQueue = new Queue();
			this.votedRoomsRemoveQueue = new Queue();
			this.votedRoomsAddQueue = new Queue();
			this.activeRoomsRemoveQueue = new Queue();
			this.activeRoomsUpdateQueue = new Queue();
			this.activeRoomsAddQueue = new Queue();
			this.eventManager = new EventManager();
		}

		internal void InitVotedRooms(IQueryAdapter dbClient)
		{
			dbClient.setQuery("SELECT * FROM rooms WHERE score > 0 AND roomtype = 'private' ORDER BY score DESC LIMIT 40");
			DataTable table = dbClient.getTable();
			foreach (DataRow dataRow in table.Rows)
			{
				RoomData data = this.FetchRoomData(Convert.ToUInt32(dataRow["id"]), dataRow);
				this.QueueVoteAdd(data);
			}
		}

		internal void LoadNewModel(string Model)
		{
            if (roomModels.Contains(Model))
            {
                roomModels.Remove(Model);
            }

			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
                queryreactor.setQuery("SELECT id,door_x,door_y,door_z,door_dir,heightmap,public_items,club_only,poolmap FROM room_models WHERE id = @model");
				queryreactor.addParameter("model", Model);
				DataTable table = queryreactor.getTable();
				if (table != null)
				{
                    string key = Model;
                    foreach (DataRow row in table.Rows)
                    {
                        string staticFurniMap = (string)row["public_items"];
                        this.roomModels.Add(Model, new RoomModel((int)row["door_x"], (int)row["door_y"], (double)row["door_z"], (int)row["door_dir"], (string)row["heightmap"], staticFurniMap, CyberEnvironment.EnumToBool(row["club_only"].ToString()), (string)row["poolmap"]));
                    }
				}
			}
		}
        internal void LoadModels(IQueryAdapter dbClient, out uint loadedModel)
        {
            LoadModels(dbClient);
            loadedModel = (uint)roomModels.Count;
        }
		internal void LoadModels(IQueryAdapter dbClient)
		{
			this.roomModels.Clear();
			dbClient.setQuery("SELECT id,door_x,door_y,door_z,door_dir,heightmap,public_items,club_only,poolmap FROM room_models");
			DataTable table = dbClient.getTable();
			if (table == null)
			{
				return;
			}
			foreach (DataRow dataRow in table.Rows)
			{
				string key = (string)dataRow["id"];
                if (key.StartsWith("model_floorplan_"))
                {
                    continue;
                }
				string staticFurniMap = (string)dataRow["public_items"];
				this.roomModels.Add(key, new RoomModel((int)dataRow["door_x"], (int)dataRow["door_y"], (double)dataRow["door_z"], (int)dataRow["door_dir"], (string)dataRow["heightmap"], staticFurniMap, CyberEnvironment.EnumToBool(dataRow["club_only"].ToString()), (string)dataRow["poolmap"]));
			}
		}

        internal void OnCycle()
        {
            try
            {
                this.WorkRoomDataQueue();
                this.WorkRoomsToAddQueue();
                this.WorkRoomsToRemoveQueue();
                
                bool flag = this.WorkActiveRoomsAddQueue();
                bool flag2 = this.WorkActiveRoomsRemoveQueue();
                bool flag3 = this.WorkActiveRoomsUpdateQueue();
                if (flag || flag2 || flag3)
                {
                    this.SortActiveRooms();
                }
                bool flag4 = this.WorkVotedRoomsAddQueue();
                bool flag5 = this.WorkVotedRoomsRemoveQueue();
                if (flag4 || flag5)
                {
                    this.SortVotedRooms();
                }
                CyberEnvironment.GetGame().RoomManagerCycle_ended = true;
            }
            catch (Exception ex)
            {
                Logging.LogThreadException(ex.ToString(), "RoomManager.OnCycle Exception --> Not inclusive");
            }
        }

		private void SortActiveRooms()
		{
            this.orderedActiveRooms = new HashSet<RoomData>(activeRooms.Keys.OrderByDescending(x => x.UsersNow).Take(40));
		}
		private void SortVotedRooms()
		{
            this.orderedVotedRooms = new HashSet<RoomData>(votedRooms.Keys.OrderByDescending(x => x.UsersNow).Take(40));
		}

		private bool WorkActiveRoomsUpdateQueue()
		{
			if (this.activeRoomsUpdateQueue.Count > 0)
			{
				lock (this.activeRoomsUpdateQueue.SyncRoot)
				{
					while (this.activeRoomsUpdateQueue.Count > 0)
					{
						RoomData roomData = (RoomData)this.activeRoomsUpdateQueue.Dequeue();
						if (!roomData.ModelName.Contains("snowwar"))
						{
							if (!this.activeRooms.ContainsKey(roomData))
							{
								this.activeRooms.Add(roomData, roomData.UsersNow);
							}
							else
							{
								this.activeRooms[roomData] = roomData.UsersNow;
							}
						}
					}
				}
				return true;
			}
			return false;
		}
		private bool WorkActiveRoomsAddQueue()
		{
			if (this.activeRoomsAddQueue.Count > 0)
			{
				lock (this.activeRoomsAddQueue.SyncRoot)
				{
					while (this.activeRoomsAddQueue.Count > 0)
					{
						RoomData roomData = (RoomData)this.activeRoomsAddQueue.Dequeue();
						if (!this.activeRooms.ContainsKey(roomData) && !roomData.ModelName.Contains("snowwar"))
						{
							this.activeRooms.Add(roomData, roomData.UsersNow);
						}
					}
				}
				return true;
			}
			return false;
		}
		private bool WorkActiveRoomsRemoveQueue()
		{
			if (this.activeRoomsRemoveQueue.Count > 0)
			{
				lock (this.activeRoomsRemoveQueue.SyncRoot)
				{
					while (this.activeRoomsRemoveQueue.Count > 0)
					{
						RoomData key = (RoomData)this.activeRoomsRemoveQueue.Dequeue();
						this.activeRooms.Remove(key);
					}
				}
				return true;
			}
			return false;
		}
		private bool WorkVotedRoomsAddQueue()
		{
			if (this.votedRoomsAddQueue.Count > 0)
			{
				lock (this.votedRoomsAddQueue.SyncRoot)
				{
					while (this.votedRoomsAddQueue.Count > 0)
					{
						RoomData roomData = (RoomData)this.votedRoomsAddQueue.Dequeue();
						if (!this.votedRooms.ContainsKey(roomData))
						{
							this.votedRooms.Add(roomData, roomData.Score);
						}
						else
						{
							this.votedRooms[roomData] = roomData.Score;
						}
					}
				}
				return true;
			}
			return false;
		}
		private bool WorkVotedRoomsRemoveQueue()
		{
			if (this.votedRoomsRemoveQueue.Count > 0)
			{
				lock (this.votedRoomsRemoveQueue.SyncRoot)
				{
					while (this.votedRoomsRemoveQueue.Count > 0)
					{
						RoomData key = (RoomData)this.votedRoomsRemoveQueue.Dequeue();
						this.votedRooms.Remove(key);
					}
				}
				return true;
			}
			return false;
		}
		private void WorkRoomsToAddQueue()
		{
			if (this.roomsToAddQueue.Count > 0)
			{
				lock (this.roomsToAddQueue.SyncRoot)
				{
					while (this.roomsToAddQueue.Count > 0)
					{
						Room room = (Room)this.roomsToAddQueue.Dequeue();
						if (!this.loadedRooms.ContainsKey(room.RoomId))
						{
							this.loadedRooms.Add(room.RoomId, room);
						}
					}
				}
			}
		}
		private void WorkRoomsToRemoveQueue()
		{
			if (this.roomsToRemoveQueue.Count > 0)
			{
				lock (this.roomsToRemoveQueue.SyncRoot)
				{
					while (this.roomsToRemoveQueue.Count > 0)
					{
						uint key = (uint)this.roomsToRemoveQueue.Dequeue();
						this.loadedRooms.Remove(key);
					}
				}
			}
		}
		private void WorkRoomDataQueue()
		{
			if (this.roomDataToAddQueue.Count > 0)
			{
				lock (this.roomDataToAddQueue.SyncRoot)
				{
					while (this.roomDataToAddQueue.Count > 0)
					{
						RoomData roomData = (RoomData)this.roomDataToAddQueue.Dequeue();
						if (!this.loadedRooms.ContainsKey(roomData.Id))
						{
							this.loadedRoomData.Add(roomData.Id, roomData);
						}
					}
				}
			}
		}

		internal void QueueVoteAdd(RoomData data)
		{
			lock (this.votedRoomsAddQueue.SyncRoot)
			{
				this.votedRoomsAddQueue.Enqueue(data);
			}
		}
		internal void QueueVoteRemove(RoomData data)
		{
			lock (this.votedRoomsRemoveQueue.SyncRoot)
			{
				this.votedRoomsRemoveQueue.Enqueue(data);
			}
		}
		internal void QueueActiveRoomUpdate(RoomData data)
		{
			lock (this.activeRoomsUpdateQueue.SyncRoot)
			{
				this.activeRoomsUpdateQueue.Enqueue(data);
			}
		}
		internal void QueueActiveRoomAdd(RoomData data)
		{
			lock (this.activeRoomsAddQueue.SyncRoot)
			{
				this.activeRoomsAddQueue.Enqueue(data);
			}
		}
		internal void QueueActiveRoomRemove(RoomData data)
		{
			lock (this.activeRoomsRemoveQueue.SyncRoot)
			{
				this.activeRoomsRemoveQueue.Enqueue(data);
			}
		}
		internal void RemoveAllRooms()
		{
			int count = this.loadedRooms.Count;
			int num = 0;
			foreach (Room current in this.loadedRooms.Values)
			{
				CyberEnvironment.GetGame().GetRoomManager().UnloadRoom(current);
				Console.Clear();
				Console.WriteLine("<<- SERVER SHUTDOWN ->> ROOM ITEM SAVE: " + string.Format("{0:0.##}", (double)num / (double)count * 100.0) + "%");
				checked
				{
					num++;
				}
			}
			Console.WriteLine("RoomManager Destroyed!");
		}
		internal void UnloadRoom(Room Room)
		{
			if (Room == null)
			{
				return;
			}

            if (CyberEnvironment.GetGame().GetNavigator().PrivateCategories.Contains(Room.Category))
            {
                ((FlatCat)CyberEnvironment.GetGame().GetNavigator().PrivateCategories[Room.Category]).removeUsers(Room.UserCount);
            }
            Room.UsersNow = 0;
            
			StringBuilder stringBuilder = new StringBuilder();
			checked
			{
				for (int i = 0; i < Room.TagCount; i++)
				{
					if (i > 0)
					{
						stringBuilder.Append(",");
					}
					stringBuilder.Append(Room.Tags[i]);
				}
				string text = "open";
				if (Room.State == 1)
				{
					text = "locked";
				}
				else
				{
					if (Room.State > 1)
					{
						text = "password";
					}
				}
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.setQuery(string.Concat(new object[]
					{
						"UPDATE rooms SET caption = @caption, description = @description, password = @password, category = ",
						Room.Category,
						", state = '",
						text,
						"', tags = @tags, users_now = '0', users_max = ",
						Room.UsersMax,
						", allow_pets = '",
						Room.AllowPets,
						"', allow_pets_eat = '",
						Room.AllowPetsEating,
						"', allow_walkthrough = '",
						Room.AllowWalkthrough,
						"', allow_hidewall = '",
						Room.Hidewall,
						"', floorthick = ",
						Room.FloorThickness,
						", wallthick = ",
						Room.WallThickness,
						", mute_settings='",
						Room.WhoCanMute,
						"', kick_settings='",
						Room.WhoCanKick,
						"',ban_settings='",
						Room.WhoCanBan,
						"', walls_height = '", Room.WallHeight, "', chat_type = @chat_t,chat_balloon = @chat_b,chat_speed = @chat_s,chat_max_distance = @chat_m,chat_flood_protection = @chat_f WHERE id = ",
						Room.RoomId
					}));
					queryreactor.addParameter("caption", Room.Name);
					queryreactor.addParameter("description", Room.Description);
					queryreactor.addParameter("password", Room.Password);
					queryreactor.addParameter("tags", stringBuilder.ToString());
					queryreactor.addParameter("chat_t", Room.ChatType);
					queryreactor.addParameter("chat_b", Room.ChatBalloon);
					queryreactor.addParameter("chat_s", Room.ChatSpeed);
					queryreactor.addParameter("chat_m", Room.ChatMaxDistance);
					queryreactor.addParameter("chat_f", Room.ChatFloodProtection);
					queryreactor.runQuery();
				}
				lock (this.roomsToRemoveQueue.SyncRoot)
				{
					this.roomsToRemoveQueue.Enqueue(Room.RoomId);
				}
                Logging.WriteLine("[RoomMgr] Room #[" + Room.RoomId + "] was unloaded.", ConsoleColor.DarkYellow);
				foreach (RoomUser current in Room.GetRoomUserManager().UserList.Values)
				{
					if (current.IsPet)
					{
						using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
						{
							queryreactor2.setQuery("UPDATE bots SET x=@x, y=@y, z=@z WHERE id=@id LIMIT 1;");
							queryreactor2.addParameter("x", current.X);
							queryreactor2.addParameter("y", current.Y);
							queryreactor2.addParameter("z", current.Z);
							queryreactor2.addParameter("id", current.PetData.PetId);
							queryreactor2.runQuery();
							goto IL_4AA;
						}
					}
					goto IL_38A;
					IL_4AA:
					Room.GetRoomUserManager().RemoveRoomUser(current);
					continue;
					IL_38A:
					if (current.IsBot)
					{
						using (IQueryAdapter queryreactor3 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
						{
							queryreactor3.setQuery("UPDATE bots SET x=@x, y=@y, z=@z, name=@name, motto=@motto, look=@look, rotation=@rotation, dance=@dance WHERE id=@id LIMIT 1;");
							queryreactor3.addParameter("name", current.BotData.Name);
							queryreactor3.addParameter("motto", current.BotData.Motto);
							queryreactor3.addParameter("look", current.BotData.Look);
							queryreactor3.addParameter("rotation", current.BotData.Rot);
							queryreactor3.addParameter("dance", current.BotData.DanceId);
							queryreactor3.addParameter("x", current.X);
							queryreactor3.addParameter("y", current.Y);
							queryreactor3.addParameter("z", current.Z);
							queryreactor3.addParameter("id", current.BotData.BotId);
							queryreactor3.runQuery();
						}
						goto IL_4AA;
					}
					goto IL_4AA;
				}
                lock (Room.RoomChat)
                {
                    foreach (Chatlog current2 in Room.RoomChat)
                    {
                        current2.Save(Room.RoomId);
                    }
                }
				Room.Destroy();
			}
		}
	}
}
