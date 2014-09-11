using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.Collections;
using Cyber.Core;
using Cyber.HabboHotel.Catalogs;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Items;
using Cyber.HabboHotel.Pets;
using Cyber.HabboHotel.RoomBots;
using Cyber.HabboHotel.Rooms;
using Cyber.HabboHotel.Users.UserDataManagement;
using Cyber.Messages;
using Cyber.Messages.Headers;
using Cyber.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Collections.Specialized;

namespace Cyber.HabboHotel.Users.Inventory
{
	internal class InventoryComponent
	{
		private HybridDictionary floorItems;
		private HybridDictionary wallItems;
		private HybridDictionary discs;
		private SafeDictionary<uint, Pet> InventoryPets;
		private HybridDictionary mAddedItems;
		private HybridDictionary mRemovedItems;
		private GameClient mClient;
		private SafeDictionary<uint, RoomBot> InventoryBots;
		internal uint UserId;
		private bool isUpdated;
		private bool userAttatched;
		internal bool NeedsUpdate
		{
			get
			{
				return !this.userAttatched && !this.isUpdated;
			}
		}
		public bool isInactive
		{
			get
			{
				return !this.userAttatched;
			}
		}
		internal HybridDictionary songDisks
		{
			get
			{
				return this.discs;
			}
		}
		internal InventoryComponent(uint UserId, GameClient Client, UserData UserData)
		{
			this.mClient = Client;
			this.UserId = UserId;
			this.floorItems = new HybridDictionary();
			this.wallItems = new HybridDictionary();
			this.discs = new HybridDictionary();
			foreach (UserItem current in UserData.inventory)
			{
				if (current.GetBaseItem().InteractionType == InteractionType.musicdisc)
				{
					this.discs.Add(current.Id, current);
				}
				if (current.isWallItem)
				{
					this.wallItems.Add(current.Id, current);
				}
				else
				{
					this.floorItems.Add(current.Id, current);
				}
			}
			this.InventoryPets = new SafeDictionary<uint, Pet>(UserData.pets);
			this.InventoryBots = new SafeDictionary<uint, RoomBot>(UserData.Botinv);
			this.mAddedItems = new HybridDictionary();
			this.mRemovedItems = new HybridDictionary();
			this.isUpdated = false;
		}
		internal void ClearItems()
		{
			this.UpdateItems(true);
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery("DELETE FROM items WHERE room_id='0' AND user_id = " + this.UserId);
			}
			this.mAddedItems.Clear();
			this.mRemovedItems.Clear();
			this.floorItems.Clear();
			this.wallItems.Clear();
			this.discs.Clear();
			this.InventoryPets.Clear();
			this.isUpdated = true;
			this.mClient.GetMessageHandler().GetResponse().Init(Outgoing.UpdateInventoryMessageComposer);
			this.GetClient().GetMessageHandler().SendResponse();
		}
		internal void Redeemcredits(GameClient session)
		{
			Room currentRoom = session.GetHabbo().CurrentRoom;
			if (currentRoom == null)
			{
				return;
			}
			DataTable table;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT id FROM items WHERE user_id=" + session.GetHabbo().Id + " AND room_id='0'");
				table = queryreactor.getTable();
			}
			checked
			{
				foreach (DataRow dataRow in table.Rows)
				{
					UserItem item = this.GetItem(Convert.ToUInt32(dataRow[0]));
					if (item != null && (item.GetBaseItem().Name.StartsWith("CF_") || item.GetBaseItem().Name.StartsWith("CFC_")))
					{
						string[] array = item.GetBaseItem().Name.Split(new char[]
						{
							'_'
						});
						int num = int.Parse(array[1]);
						using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
						{
							queryreactor2.runFastQuery("DELETE FROM items WHERE id=" + item.Id + " LIMIT 1");
						}
						if (currentRoom.GetRoomItemHandler().GetItem(item.Id) != null)
						{
							RoomItem item2 = currentRoom.GetRoomItemHandler().GetItem(item.Id);
							currentRoom.GetRoomItemHandler().RemoveItem(item2);
						}
						this.RemoveItem(item.Id, false);
						if (num > 0)
						{
							session.GetHabbo().Credits += num;
							session.GetHabbo().UpdateCreditsBalance();
						}
					}
				}
			}
		}
		internal void SetActiveState(GameClient client)
		{
			this.mClient = client;
			this.userAttatched = true;
		}
		internal void SetIdleState()
		{
			this.userAttatched = false;
			this.mClient = null;
		}
		internal Pet GetPet(uint Id)
		{
			if (this.InventoryPets.ContainsKey(Id))
			{
				return this.InventoryPets[Id];
			}
			return null;
		}
		internal bool RemovePet(uint PetId)
		{
			this.isUpdated = false;
			ServerMessage serverMessage = new ServerMessage(604);
			serverMessage.AppendUInt(PetId);
			this.GetClient().SendMessage(serverMessage);
			this.InventoryPets.Remove(PetId);
			return true;
		}
		internal void MovePetToRoom(uint PetId)
		{
			this.isUpdated = false;
			this.RemovePet(PetId);
		}
		internal void AddPet(Pet Pet)
		{
			this.isUpdated = false;
			if (Pet == null || this.InventoryPets.ContainsKey(Pet.PetId))
			{
				return;
			}
			Pet.PlacedInRoom = false;
			Pet.RoomId = 0u;
			this.InventoryPets.Add(Pet.PetId, Pet);
			ServerMessage message = new ServerMessage(603);
			Pet.SerializeInventory(message);
			this.GetClient().SendMessage(message);
		}
		internal void LoadInventory()
		{
			this.floorItems.Clear();
			this.wallItems.Clear();
			DataTable table;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT * FROM ITEMS WHERE user_id=@userid AND room_id='0' LIMIT 8000;");
				queryreactor.addParameter("userid", checked((int)this.UserId));
				table = queryreactor.getTable();
			}
			foreach (DataRow dataRow in table.Rows)
			{
				uint num = Convert.ToUInt32(dataRow[0]);
				uint baseItem = Convert.ToUInt32(dataRow[3]);
				string extraData;
				if (!DBNull.Value.Equals(dataRow[4]))
				{
					extraData = (string)dataRow[4];
				}
				else
				{
					extraData = string.Empty;
				}
				uint group = Convert.ToUInt32(dataRow[10]);
				string songCode;
				if (!DBNull.Value.Equals(dataRow["songcode"]))
				{
					songCode = (string)dataRow["songcode"];
				}
				else
				{
					songCode = string.Empty;
				}
				UserItem userItem = new UserItem(num, baseItem, extraData, group, songCode);
                if (userItem.GetBaseItem().InteractionType == InteractionType.musicdisc && !this.discs.Contains(num))
				{
					this.discs.Add(num, userItem);
				}
				if (userItem.isWallItem)
				{
					if (!this.wallItems.Contains(num))
					{
						this.wallItems.Add(num, userItem);
					}
				}
				else
				{
                    if (!this.floorItems.Contains(num))
					{
						this.floorItems.Add(num, userItem);
					}
				}
			}
			this.discs.Clear();
			this.InventoryPets.Clear();
			using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor2.setQuery("SELECT * FROM bots WHERE user_id = " + this.UserId + " AND room_id = 0 AND ai_type='pet'");
				DataTable table2 = queryreactor2.getTable();
				if (table2 != null)
				{
					foreach (DataRow dataRow2 in table2.Rows)
					{
						queryreactor2.setQuery("SELECT * FROM bots_petdata WHERE id=" + dataRow2[0] + " LIMIT 1");
						DataRow row = queryreactor2.getRow();
						if (row != null)
						{
							Pet pet = Catalog.GeneratePetFromRow(dataRow2, row);
							if (this.InventoryPets.ContainsKey(pet.PetId))
							{
								this.InventoryPets.Remove(pet.PetId);
							}
							this.InventoryPets.Add(pet.PetId, pet);
						}
					}
				}
			}
		}
		internal void UpdateItems(bool FromDatabase)
		{
			if (FromDatabase)
			{
				this.RunDBUpdate();
				this.LoadInventory();
			}
			this.mClient.GetMessageHandler().GetResponse().Init(Outgoing.UpdateInventoryMessageComposer);
			this.mClient.GetMessageHandler().SendResponse();
		}
		internal UserItem GetItem(uint Id)
		{
			this.isUpdated = false;
            if (this.floorItems.Contains(Id))
			{
				return (UserItem)this.floorItems[Id];
			}
            if (this.wallItems.Contains(Id))
			{
				return (UserItem)this.wallItems[Id];
			}
			return null;
		}
		internal void AddBot(RoomBot Bot)
		{
			this.isUpdated = false;
			if (Bot == null)
			{
				Logging.WriteLine("Bot was null", ConsoleColor.Gray);
				return;
			}
			if (this.InventoryBots.ContainsKey(Bot.BotId))
			{
				Logging.WriteLine("Contains Bot", ConsoleColor.Gray);
			}
			Bot.RoomId = 0u;
			this.InventoryBots.Add(Bot.BotId, Bot);
		}
		internal RoomBot GetBot(uint Id)
		{
			if (this.InventoryBots.ContainsKey(Id))
			{
				return this.InventoryBots[Id];
			}
			Logging.WriteLine("Failed to load BOT: " + Id, ConsoleColor.Gray);
			return null;
		}
		internal bool RemoveBot(uint PetId)
		{
			this.isUpdated = false;
			if (this.InventoryBots.ContainsKey(PetId))
			{
				this.InventoryBots.Remove(PetId);
			}
			return true;
		}
		internal void MoveBotToRoom(uint PetId)
		{
			this.isUpdated = false;
			this.RemoveBot(PetId);
		}
		internal UserItem AddNewItem(uint Id, uint BaseItem, string ExtraData, uint Group, bool insert, bool fromRoom, int limno, int limtot, string SongCode = "")
		{
			this.isUpdated = false;
			if (insert)
			{
				if (fromRoom)
				{
					using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
					{
						queryreactor.runFastQuery("UPDATE items SET room_id='0' WHERE id=" + Id + " LIMIT 1");
						goto IL_1C9;
					}
				}
				using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor2.setQuery(string.Concat(new object[]
					{
						"INSERT INTO items (base_item, user_id, group_id) VALUES (",
						BaseItem,
						", ",
						this.UserId,
                        ", ",
                        Group,
						")"
					}));
					Id = checked((uint)queryreactor2.insertQuery());
					this.SendNewItems(Id);

					if (!string.IsNullOrEmpty(ExtraData))
					{
						queryreactor2.setQuery("UPDATE items SET extra_data=@extradata WHERE id=" + Id + " LIMIT 1");
						queryreactor2.addParameter("extradata", ExtraData);
						queryreactor2.runQuery();
					}
					if (limno > 0)
					{
						queryreactor2.runFastQuery(string.Concat(new object[]
						{
							"INSERT INTO items_limited VALUES (",
							Id,
							", ",
							limno,
							", ",
							limtot,
							")"
						}));
					}
					if (!string.IsNullOrEmpty(SongCode))
					{
						queryreactor2.setQuery("UPDATE items SET songcode=@song_code WHERE id=" + Id + " LIMIT 1");
						queryreactor2.addParameter("song_code", SongCode);
						queryreactor2.runQuery();
					}
				}
			}
			IL_1C9:
			UserItem userItem = new UserItem(Id, BaseItem, ExtraData, Group, SongCode);
			if (this.UserHoldsItem(Id))
			{
				this.RemoveItem(Id, false);
			}
			if (userItem.GetBaseItem().InteractionType == InteractionType.musicdisc)
			{
				this.discs.Add(userItem.Id, userItem);
			}
			if (userItem.isWallItem)
			{
				this.wallItems.Add(userItem.Id, userItem);
			}
			else
			{
				this.floorItems.Add(userItem.Id, userItem);
			}
			if (this.mRemovedItems.Contains(Id))
			{
				this.mRemovedItems.Remove(Id);
			}
            if (!this.mAddedItems.Contains(Id))
			{
				this.mAddedItems.Add(Id, userItem);
			}
			return userItem;
		}
		private bool UserHoldsItem(uint itemID)
		{
            return this.discs.Contains(itemID) || this.floorItems.Contains(itemID) || this.wallItems.Contains(itemID);
		}
		internal void RemoveItem(uint Id, bool PlacedInroom)
		{
			if (this.GetClient() == null || this.GetClient().GetHabbo() == null || this.GetClient().GetHabbo().GetInventoryComponent() == null)
			{
				this.GetClient().Disconnect();
			}
			this.isUpdated = false;
			this.GetClient().GetMessageHandler().GetResponse().Init(Outgoing.RemoveInventoryObjectMessageComposer);
			this.GetClient().GetMessageHandler().GetResponse().AppendUInt(Id);
			this.GetClient().GetMessageHandler().SendResponse();
            if (this.mAddedItems.Contains(Id))
			{
				this.mAddedItems.Remove(Id);
			}
			if (this.mRemovedItems.Contains(Id))
			{
				return;
			}
			UserItem item = this.GetClient().GetHabbo().GetInventoryComponent().GetItem(Id);
			this.discs.Remove(Id);
			this.floorItems.Remove(Id);
			this.wallItems.Remove(Id);
			this.mRemovedItems.Add(Id, item);
		}
		internal ServerMessage SerializeFloorItemInventory()
		{
			int i = checked(this.floorItems.Count + this.discs.Count + this.wallItems.Count);
			ServerMessage serverMessage = new ServerMessage(Outgoing.LoadInventoryMessageComposer);
			serverMessage.AppendInt32(1);
			serverMessage.AppendInt32(0);
			serverMessage.AppendInt32(i);
			foreach (UserItem userItem in this.floorItems.Values)
			{
				userItem.SerializeFloor(serverMessage, true);
			}
			foreach (UserItem userItem2 in this.discs.Values)
			{
				userItem2.SerializeFloor(serverMessage, true);
			}
			foreach (UserItem userItem3 in this.wallItems.Values)
			{
				userItem3.SerializeWall(serverMessage, true);
			}
			return serverMessage;
		}
		internal ServerMessage SerializeWallItemInventory()
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.LoadInventoryMessageComposer);
			serverMessage.AppendString("I");
			serverMessage.AppendInt32(1);
			serverMessage.AppendInt32(1);
			serverMessage.AppendInt32(this.wallItems.Count);
			foreach (UserItem userItem in this.wallItems.Values)
			{
				userItem.SerializeWall(serverMessage, true);
			}
			return serverMessage;
		}
		internal ServerMessage SerializePetInventory()
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.PetInventoryMessageComposer);
			serverMessage.AppendInt32(1);
			serverMessage.AppendInt32(1);
			serverMessage.AppendInt32(this.InventoryPets.Count);
			foreach (Pet current in this.InventoryPets.Values)
			{
				current.SerializeInventory(serverMessage);
			}
			return serverMessage;
		}
		internal ServerMessage SerializeBotInventory()
		{
			ServerMessage serverMessage = new ServerMessage();
			serverMessage.Init(Outgoing.BotInventoryMessageComposer);
			serverMessage.AppendInt32(this.InventoryBots.Count);
			foreach (RoomBot current in this.InventoryBots.Values)
			{
				serverMessage.AppendUInt(current.BotId);
				serverMessage.AppendString(current.Name);
				serverMessage.AppendString(current.Motto);
				serverMessage.AppendString("m");
				serverMessage.AppendString(current.Look);
			}
			return serverMessage;
		}
		private GameClient GetClient()
		{
			return CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(this.UserId);
		}
		internal void AddItemArray(List<RoomItem> RoomItemList)
		{
			foreach (RoomItem current in RoomItemList)
			{
				this.AddItem(current);
			}
		}
		internal void AddItem(RoomItem item)
		{
			this.AddNewItem(item.Id, item.BaseItem, item.ExtraData, item.GroupId, true, true, 0, 0, item.SongCode);
		}
		internal void RunCycleUpdate()
		{
			this.isUpdated = true;
			this.RunDBUpdate();
		}
		internal void RunDBUpdate()
		{
			try
			{
				if (this.mRemovedItems.Count > 0 || this.mAddedItems.Count > 0 || this.InventoryPets.Count > 0)
				{
					QueryChunk queryChunk = new QueryChunk();
					if (this.mAddedItems.Count > 0)
					{
						foreach (UserItem userItem in this.mAddedItems.Values)
						{
							queryChunk.AddQuery(string.Concat(new object[]
							{
								"UPDATE items SET user_id = ",
								this.UserId,
								", room_id='0' WHERE id = ",
								userItem.Id
							}));
						}
						this.mAddedItems.Clear();
					}
					if (this.mRemovedItems.Count > 0)
					{
						try
						{
							foreach (UserItem userItem2 in this.mRemovedItems.Values)
							{
								using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
								{
									this.GetClient().GetHabbo().CurrentRoom.GetRoomItemHandler().SaveFurniture(queryreactor, null);
								}
								if (this.discs.Contains(userItem2.Id))
								{
									this.discs.Remove(userItem2.Id);
								}
							}
						}
						catch (Exception)
						{
						}
						this.mRemovedItems.Clear();
					}
					foreach (Pet current in this.InventoryPets.Values)
					{
						if (current.DBState == DatabaseUpdateState.NeedsUpdate)
						{
							queryChunk.AddParameter(current.PetId + "name", current.Name);
							queryChunk.AddParameter(current.PetId + "race", current.Race);
							queryChunk.AddParameter(current.PetId + "color", current.Color);
							queryChunk.AddQuery(string.Concat(new object[]
							{
								"UPDATE bots SET room_id = ",
								current.RoomId,
								", name = @",
								current.PetId,
								"name, x = ",
								current.X,
								", Y = ",
								current.Y,
								", Z = ",
								current.Z,
								" WHERE id = ",
								current.PetId
							}));
							queryChunk.AddQuery(string.Concat(new object[]
							{
								"UPDATE bots_petdata SET race = @",
								current.PetId,
								"race, color = @",
								current.PetId,
								"color, type = ",
								current.Type,
								", experience = ",
								current.Experience,
								", energy = ",
								current.Energy,
								", nutrition = ",
								current.Nutrition,
								", respect = ",
								current.Respect,
								", createstamp = '",
								current.CreationStamp,
								"', lasthealth_stamp = ",
								CyberEnvironment.DateTimeToUnix(current.LastHealth),
								", untilgrown_stamp = ",
								CyberEnvironment.DateTimeToUnix(current.UntilGrown),
								" WHERE id = ",
								current.PetId
							}));
						}
						current.DBState = DatabaseUpdateState.Updated;
					}
					using (IQueryAdapter queryreactor2 = CyberEnvironment.GetDatabaseManager().getQueryReactor())
					{
						queryChunk.Execute(queryreactor2);
					}
				}
			}
			catch (Exception ex)
			{
				Logging.LogCacheError("FATAL ERROR DURING USER INVENTORY DB UPDATE: " + ex.ToString());
			}
		}
		internal ServerMessage SerializeMusicDiscs()
		{
			ServerMessage serverMessage = new ServerMessage(Outgoing.SongsLibraryMessageComposer);
			serverMessage.AppendInt32(this.discs.Count);
			foreach (UserItem current in 
				from X in this.floorItems.Values.OfType<UserItem>()
				where X.GetBaseItem().InteractionType == InteractionType.musicdisc
				select X)
			{
				uint i = 0u;
				uint.TryParse(current.ExtraData, out i);
				serverMessage.AppendUInt(current.Id);
				serverMessage.AppendUInt(i);
			}
			return serverMessage;
		}
		internal List<Pet> GetPets()
		{
			List<Pet> list = new List<Pet>();
			foreach (Pet current in this.InventoryPets.Values)
			{
				list.Add(current);
			}
			return list;
		}
		internal void SendFloorInventoryUpdate()
		{
			this.mClient.SendMessage(this.SerializeFloorItemInventory());
		}
		internal void SendNewItems(uint Id)
		{
			ServerMessage serverMessage = new ServerMessage();
			serverMessage.Init(Outgoing.NewInventoryObjectMessageComposer);
			serverMessage.AppendInt32(1);
			serverMessage.AppendInt32(1);
			serverMessage.AppendInt32(1);
			serverMessage.AppendUInt(Id);
			this.mClient.SendMessage(serverMessage);
		}
	}
}
