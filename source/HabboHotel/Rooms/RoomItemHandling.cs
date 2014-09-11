using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.Collections;
using Cyber.Core;
using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Items;
using Cyber.HabboHotel.Pathfinding;
using Cyber.Messages;
using Cyber.Messages.Headers;
using Cyber.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Collections.Specialized;

namespace Cyber.HabboHotel.Rooms
{
	internal class RoomItemHandling
	{
		private Room room;
		internal QueuedDictionary<uint, RoomItem> mFloorItems;
		internal QueuedDictionary<uint, RoomItem> mWallItems;
		private HybridDictionary mRemovedItems;
		private HybridDictionary mMovedItems;
		private HybridDictionary mAddedItems;
		internal QueuedDictionary<uint, RoomItem> mRollers;
		private List<uint> rollerItemsMoved;
		private List<uint> rollerUsersMoved;
		private List<ServerMessage> rollerMessages;
		private bool mGotRollers;
		private int mRollerSpeed;
		private int mRoolerCycle;
		public int HopperCount;
		private Queue roomItemUpdateQueue;
		internal bool GotRollers
		{
			get
			{
				return this.mGotRollers;
			}
			set
			{
				this.mGotRollers = value;
			}
		}
		public RoomItemHandling(Room room)
		{
			this.room = room;
			this.mRemovedItems = new HybridDictionary();
			this.mMovedItems = new HybridDictionary();
			this.mAddedItems = new HybridDictionary();
			this.mRollers = new QueuedDictionary<uint, RoomItem>();
			this.mWallItems = new QueuedDictionary<uint, RoomItem>();
			this.mFloorItems = new QueuedDictionary<uint, RoomItem>();
			this.roomItemUpdateQueue = new Queue();
			this.mGotRollers = false;
			this.mRoolerCycle = 0;
			this.mRollerSpeed = 4;
			this.HopperCount = 0;
			this.rollerItemsMoved = new List<uint>();
			this.rollerUsersMoved = new List<uint>();
			this.rollerMessages = new List<ServerMessage>();
		}
		internal void QueueRoomItemUpdate(RoomItem item)
		{
			lock (this.roomItemUpdateQueue.SyncRoot)
			{
				this.roomItemUpdateQueue.Enqueue(item);
			}
		}
		internal List<RoomItem> RemoveAllFurniture(GameClient Session)
		{
			List<RoomItem> list = new List<RoomItem>();
			RoomItem[] array = this.mFloorItems.Values.ToArray<RoomItem>();
			for (int i = 0; i < array.Length; i++)
			{
				RoomItem roomItem = array[i];
				roomItem.Interactor.OnRemove(Session, roomItem);
				ServerMessage serverMessage = new ServerMessage(Outgoing.PickUpFloorItemMessageComposer);
				serverMessage.AppendString(roomItem.Id + string.Empty);
				serverMessage.AppendBoolean(false);
				serverMessage.AppendInt32(0);
				serverMessage.AppendUInt(roomItem.UserID);
				this.room.SendMessage(serverMessage);
				list.Add(roomItem);
			}
			RoomItem[] array2 = this.mWallItems.Values.ToArray<RoomItem>();
			for (int j = 0; j < array2.Length; j++)
			{
				RoomItem roomItem2 = array2[j];
				roomItem2.Interactor.OnRemove(Session, roomItem2);
				ServerMessage serverMessage2 = new ServerMessage(Outgoing.PickUpWallItemMessageComposer);
				serverMessage2.AppendString(roomItem2.Id + string.Empty);
				serverMessage2.AppendUInt(roomItem2.UserID);
				this.room.SendMessage(serverMessage2);
				list.Add(roomItem2);
			}
			this.mWallItems.Clear();
			this.mFloorItems.Clear();
			this.mRemovedItems.Clear();
			this.mMovedItems.Clear();
			this.mAddedItems.Clear();
			this.mRollers.QueueDelegate(new onCycleDoneDelegate(this.ClearRollers));
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.runFastQuery("UPDATE items SET room_id='0' WHERE room_id = " + this.room.RoomId);
			}
			this.room.GetGameMap().GenerateMaps(true);
            this.room.GetGameMap().lazyWalkablePoints();
			this.room.GetRoomUserManager().UpdateUserStatusses();
			return list;
		}
		private void ClearRollers()
		{
			this.mRollers.Clear();
		}
		internal void SetSpeed(int p)
		{
			this.mRollerSpeed = p;
		}
		public string WallPositionCheck(string wallPosition)
		{
			string result;
			try
			{
				if (wallPosition.Contains(Convert.ToChar(13)))
				{
					result = null;
				}
				else
				{
					if (wallPosition.Contains(Convert.ToChar(9)))
					{
						result = null;
					}
					else
					{
						string[] array = wallPosition.Split(new char[]
						{
							' '
						});
						if (array[2] != "l" && array[2] != "r")
						{
							result = null;
						}
						else
						{
							string[] array2 = array[0].Substring(3).Split(new char[]
							{
								','
							});
							int num = int.Parse(array2[0]);
							int num2 = int.Parse(array2[1]);
							if (num < 0 || num2 < 0 || num > 200 || num2 > 200)
							{
								result = null;
							}
							else
							{
								string[] array3 = array[1].Substring(2).Split(new char[]
								{
									','
								});
								int num3 = int.Parse(array3[0]);
								int num4 = int.Parse(array3[1]);
								if (num3 < 0 || num4 < 0 || num3 > 200 || num4 > 200)
								{
									result = null;
								}
								else
								{
									result = string.Concat(new object[]
									{
										":w=",
										num,
										",",
										num2,
										" l=",
										num3,
										",",
										num4,
										" ",
										array[2]
									});
								}
							}
						}
					}
				}
			}
			catch
			{
				result = null;
			}
			return result;
		}
		internal void LoadFurniture()
		{
			this.mFloorItems.Clear();
			this.mWallItems.Clear();
			checked
			{
				using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
				{
					queryreactor.setQuery("SELECT `items`.* , COALESCE(`items_groups`.`group_id`, 0) AS group_id FROM `items` LEFT OUTER JOIN `items_groups` ON `items`.`id` = `items_groups`.`id` WHERE items.room_id=@roomid LIMIT 2000");
					queryreactor.addParameter("roomid", this.room.RoomId);
					DataTable table = queryreactor.getTable();
					if (table.Rows.Count == 2000)
					{
						GameClient clientByUserID = CyberEnvironment.GetGame().GetClientManager().GetClientByUserID((uint)this.room.OwnerId);
						if (clientByUserID != null)
						{
							clientByUserID.SendNotif("Your room has more than 2000 items in it. The current limit of items per room is 2000.\nTo view the rest, pick some of these items up!");
						}
					}
					foreach (DataRow dataRow in table.Rows)
					{
						try
						{
							uint num = Convert.ToUInt32(dataRow[0]);
							int x = Convert.ToInt32(dataRow[5]);
							int y = Convert.ToInt32(dataRow[6]);
							double num2 = Convert.ToDouble(dataRow[7]);
							sbyte rot = Convert.ToSByte(dataRow[8]);
							uint num3 = Convert.ToUInt32(dataRow[1]);
							if (num3 == 0u)
							{
								queryreactor.setQuery("UPDATE items SET user_id=@userid WHERE id=@itemID LIMIT 1");
								queryreactor.addParameter("itemID", num);
								queryreactor.addParameter("userid", this.room.OwnerId);
								queryreactor.runQuery();
							}
							string text;
							if (string.IsNullOrWhiteSpace(dataRow[9].ToString()))
							{
								queryreactor.setQuery("SELECT type FROM furniture WHERE id=" + dataRow[3] + " LIMIT 1");
								string @string = queryreactor.getString();
								if (@string == "i")
								{
									text = ":w=0,2 l=11,53 l";
									queryreactor.runFastQuery(string.Concat(new object[]
									{
										"UPDATE items SET wall_pos='",
										text,
										"' WHERE id=",
										num,
										" LIMIT 1"
									}));
								}
							}
							text = Convert.ToString(dataRow[9]);
							uint num4 = Convert.ToUInt32(dataRow[3]);
							string extraData;
							if (DBNull.Value.Equals(dataRow[4]))
							{
								extraData = string.Empty;
							}
							else
							{
								extraData = (string)dataRow[4];
							}
							string songCode;
							if (DBNull.Value.Equals(dataRow["songcode"]))
							{
								songCode = string.Empty;
							}
							else
							{
								songCode = (string)dataRow["songcode"];
							}
							uint group = Convert.ToUInt32(dataRow["group_id"]);
							if (!string.IsNullOrWhiteSpace(text))
							{
								string wallCoord = this.WallPositionCheck(":" + text.Split(new char[]
								{
									':'
								})[1]);
								RoomItem value = new RoomItem(num, this.room.RoomId, num4, extraData, wallCoord, this.room, num3, group, CyberEnvironment.GetGame().GetItemManager().GetItem(num4).FlatId);
								if (!this.mWallItems.ContainsKey(num))
								{
									this.mWallItems.Inner.Add(num, value);
								}
							}
							else
							{
								RoomItem roomItem = new RoomItem(num, this.room.RoomId, num4, extraData, x, y, num2, (int)rot, this.room, num3, group, CyberEnvironment.GetGame().GetItemManager().GetItem(num4).FlatId, songCode);
								if (!this.room.GetGameMap().ValidTile(x, y))
								{
									GameClient clientByUserID2 = CyberEnvironment.GetGame().GetClientManager().GetClientByUserID(num3);
									if (clientByUserID2 != null)
									{
										clientByUserID2.GetHabbo().GetInventoryComponent().AddNewItem(roomItem.Id, roomItem.BaseItem, roomItem.ExtraData, group, true, true, 0, 0, "");
										queryreactor.runFastQuery("UPDATE items SET room_id='0' WHERE id='" + roomItem.Id + "' LIMIT 1");
										clientByUserID2.GetHabbo().GetInventoryComponent().UpdateItems(true);
									}
									else
									{
										queryreactor.runFastQuery("UPDATE items SET room_id='0' WHERE id='" + roomItem.Id + "' LIMIT 1");
									}
								}
								else
								{
									if (roomItem.GetBaseItem().InteractionType == InteractionType.hopper)
									{
										this.HopperCount++;
									}
									if (!this.mFloorItems.ContainsKey(num))
									{
										this.mFloorItems.Inner.Add(num, roomItem);
									}
								}
							}
						}
						catch (Exception value2)
						{
							Console.WriteLine(value2);
						}
					}
					new List<uint>();
					foreach (RoomItem current in this.mFloorItems.Values)
					{
						if (current.IsWired)
						{
							this.room.GetWiredHandler().LoadWired(this.room.GetWiredHandler().GenerateNewItem(current));
						}
						if (current.IsRoller)
						{
							this.mGotRollers = true;
						}
						else
						{
							if (current.GetBaseItem().InteractionType == InteractionType.dimmer)
							{
								if (this.room.MoodlightData == null)
								{
									this.room.MoodlightData = new MoodlightData(current.Id);
								}
							}
							else
							{
								if (current.GetBaseItem().InteractionType == InteractionType.roombg && this.room.TonerData == null)
								{
									this.room.TonerData = new TonerData(current.Id);
								}
							}
						}
					}
				}
			}
		}
		internal RoomItem GetItem(uint pId)
		{
            this.mFloorItems.OnCycle();
            this.mWallItems.OnCycle();

			if (this.mFloorItems.ContainsKey(pId))
			{
				return this.mFloorItems.GetValue(pId);
			}
			if (this.mWallItems.ContainsKey(pId))
			{
				return this.mWallItems.GetValue(pId);
			}
			return null;
		}
		internal void RemoveFurniture(GameClient Session, uint pId, bool WasPicked = true)
		{
			RoomItem item = this.GetItem(pId);
			if (item == null)
			{
				return;
			}
			if (item.GetBaseItem().InteractionType == InteractionType.fbgate)
			{
				this.room.GetSoccer().UnRegisterGate(item);
			}
			if (item.GetBaseItem().InteractionType != InteractionType.gift)
			{
				item.Interactor.OnRemove(Session, item);
			}
			this.RemoveRoomItem(item, WasPicked);
            this.mFloorItems.OnCycle();
		}
		internal void RemoveRoomItem(RoomItem Item, bool WasPicked)
		{
			if (Item.IsWallItem)
			{
				ServerMessage serverMessage = new ServerMessage(Outgoing.PickUpWallItemMessageComposer);
				serverMessage.AppendString(Item.Id + string.Empty);
				if (WasPicked)
				{
					serverMessage.AppendUInt(Item.UserID);
				}
				else
				{
					serverMessage.AppendInt32(0);
				}
				this.room.SendMessage(serverMessage);
			}
			else
			{
				if (Item.IsFloorItem)
				{
					ServerMessage serverMessage2 = new ServerMessage(Outgoing.PickUpFloorItemMessageComposer);
					serverMessage2.AppendString(Item.Id + string.Empty);
					serverMessage2.AppendBoolean(false);
					serverMessage2.AppendInt32((!WasPicked && Item.GetBaseItem().InteractionType == InteractionType.moplaseed) ? -1 : 0);
					if (WasPicked)
					{
						serverMessage2.AppendUInt(Item.UserID);
					}
					else
					{
						serverMessage2.AppendInt32(0);
					}
					this.room.SendMessage(serverMessage2);
				}
			}
			if (Item.IsWallItem)
			{
				this.mWallItems.Remove(Item.Id);
			}
			else
			{
				this.mFloorItems.Remove(Item.Id);
				this.mFloorItems.OnCycle();
				this.room.GetGameMap().RemoveFromMap(Item);
			}
			this.RemoveItem(Item);
			this.room.GetRoomUserManager().UpdateUserStatusses();
		}
		private List<ServerMessage> CycleRollers()
		{
			this.mRollers.OnCycle();
			if (this.mGotRollers)
			{
				if (this.mRoolerCycle >= this.mRollerSpeed || this.mRollerSpeed == 0)
				{
					this.rollerItemsMoved.Clear();
					this.rollerUsersMoved.Clear();
					this.rollerMessages.Clear();
					foreach (RoomItem current in this.mRollers.Values)
					{
						Point squareInFront = current.SquareInFront;
						List<RoomItem> roomItemForSquare = this.room.GetGameMap().GetRoomItemForSquare(current.GetX, current.GetY);
						RoomUser userForSquare = this.room.GetRoomUserManager().GetUserForSquare(current.GetX, current.GetY);
						if (roomItemForSquare.Count > 0 || userForSquare != null)
						{
							List<RoomItem> coordinatedItems = this.room.GetGameMap().GetCoordinatedItems(squareInFront);
							double nextZ = 0.0;
							int num = 0;
							bool flag = false;
							double num2 = 0.0;
							bool flag2 = true;
							foreach (RoomItem current2 in coordinatedItems)
							{
								if (current2.IsRoller)
								{
									flag = true;
									if (current2.TotalHeight > num2)
									{
										num2 = current2.TotalHeight;
									}
								}
							}
							if (flag)
							{
								using (List<RoomItem>.Enumerator enumerator3 = coordinatedItems.GetEnumerator())
								{
									while (enumerator3.MoveNext())
									{
										RoomItem current3 = enumerator3.Current;
										if (current3.TotalHeight > num2)
										{
											flag2 = false;
										}
									}
									goto IL_192;
								}
							}
							goto IL_17C;
							IL_192:
							nextZ = num2;
							bool flag3 = num > 0;
							if (this.room.GetRoomUserManager().GetUserForSquare(squareInFront.X, squareInFront.Y) != null)
							{
								flag3 = true;
							}
							foreach (RoomItem current4 in roomItemForSquare)
							{
								double num3 = current4.GetZ - current.TotalHeight;
								if (!this.rollerItemsMoved.Contains(current4.Id) && this.room.GetGameMap().CanRollItemHere(squareInFront.X, squareInFront.Y) && flag2 && current.GetZ < current4.GetZ && this.room.GetRoomUserManager().GetUserForSquare(squareInFront.X, squareInFront.Y) == null)
								{
									this.rollerMessages.Add(this.UpdateItemOnRoller(current4, squareInFront, current.Id, num2 + num3));
									this.rollerItemsMoved.Add(current4.Id);
								}
							}
							if (userForSquare != null && !userForSquare.IsWalking && flag2 && !flag3 && this.room.GetGameMap().CanRollItemHere(squareInFront.X, squareInFront.Y) && this.room.GetGameMap().GetFloorStatus(squareInFront) != 0 && !this.rollerUsersMoved.Contains(userForSquare.HabboId))
							{
								this.rollerMessages.Add(this.UpdateUserOnRoller(userForSquare, squareInFront, current.Id, nextZ));
								this.rollerUsersMoved.Add(userForSquare.HabboId);
								continue;
							}
							continue;
							IL_17C:
							num2 += this.room.GetGameMap().GetHeightForSquareFromData(squareInFront);
							goto IL_192;
						}
					}
					this.mRoolerCycle = 0;
					return this.rollerMessages;
				}
				checked
				{
					this.mRoolerCycle++;
				}
			}
			return new List<ServerMessage>();
		}
		internal ServerMessage UpdateItemOnRoller(RoomItem pItem, Point NextCoord, uint pRolledID, double NextZ)
		{
			ServerMessage serverMessage = new ServerMessage();
			serverMessage.Init(Outgoing.ItemAnimationMessageComposer);
			serverMessage.AppendInt32(pItem.GetX);
			serverMessage.AppendInt32(pItem.GetY);
			serverMessage.AppendInt32(NextCoord.X);
			serverMessage.AppendInt32(NextCoord.Y);
			serverMessage.AppendInt32(1);
			serverMessage.AppendUInt(pItem.Id);
			serverMessage.AppendString(TextHandling.GetString(pItem.GetZ));
			serverMessage.AppendString(TextHandling.GetString(NextZ));
			serverMessage.AppendUInt(pRolledID);
			this.SetFloorItem(pItem, NextCoord.X, NextCoord.Y, NextZ);
			return serverMessage;
		}
		internal ServerMessage UpdateUserOnRoller(RoomUser pUser, Point pNextCoord, uint pRollerID, double NextZ)
		{
			ServerMessage serverMessage = new ServerMessage(0);
			serverMessage.Init(Outgoing.ItemAnimationMessageComposer);
			serverMessage.AppendInt32(pUser.X);
			serverMessage.AppendInt32(pUser.Y);
			serverMessage.AppendInt32(pNextCoord.X);
			serverMessage.AppendInt32(pNextCoord.Y);
			serverMessage.AppendInt32(0);
			serverMessage.AppendUInt(pRollerID);
			serverMessage.AppendInt32(2);
			serverMessage.AppendInt32(pUser.VirtualId);
			serverMessage.AppendString(TextHandling.GetString(pUser.Z));
			serverMessage.AppendString(TextHandling.GetString(NextZ));
			this.room.GetGameMap().UpdateUserMovement(new Point(pUser.X, pUser.Y), new Point(pNextCoord.X, pNextCoord.Y), pUser);
			this.room.GetGameMap().GameMap[pUser.X, pUser.Y] = 1;
			pUser.X = pNextCoord.X;
			pUser.Y = pNextCoord.Y;
			pUser.Z = NextZ;
			this.room.GetGameMap().GameMap[pUser.X, pUser.Y] = 0;
			return serverMessage;
		}
		public void SaveFurniture(IQueryAdapter dbClient, GameClient Session = null)
		{
			try
			{
				if (this.mAddedItems.Count > 0 || this.mRemovedItems.Count > 0 || this.mMovedItems.Count > 0 || this.room.GetRoomUserManager().PetCount > 0)
				{
					QueryChunk queryChunk = new QueryChunk();
					QueryChunk queryChunk2 = new QueryChunk();
					QueryChunk queryChunk3 = new QueryChunk();
					foreach (RoomItem roomItem in this.mRemovedItems.Values)
					{
						queryChunk.AddQuery("UPDATE items SET room_id='0', x='0', y='0', z='0', rot='0' WHERE id = " + roomItem.Id + " ");
					}
					if (this.mAddedItems.Count > 0)
					{
						foreach (RoomItem roomItem2 in this.mAddedItems.Values)
						{
							if (!string.IsNullOrEmpty(roomItem2.ExtraData))
							{
								queryChunk3.AddQuery(string.Concat(new object[]
								{
									"UPDATE items SET extra_data=@edata",
									roomItem2.Id,
									" WHERE id='",
									roomItem2.Id,
									"'"
								}));
								queryChunk3.AddParameter("edata" + roomItem2.Id, roomItem2.ExtraData);
							}
							if (roomItem2.IsFloorItem)
							{
								queryChunk2.AddQuery(string.Concat(new object[]
								{
									"UPDATE items SET room_id=",
									roomItem2.RoomId,
									", x=",
									roomItem2.GetX,
									", y=",
									roomItem2.GetY,
									", z=",
									roomItem2.GetZ,
									", rot=",
									roomItem2.Rot,
									" WHERE id='",
									roomItem2.Id,
									"'"
								}));
							}
							else
							{
								queryChunk2.AddQuery(string.Concat(new object[]
								{
									"UPDATE items SET room_id='",
									roomItem2.RoomId,
									"',wall_pos='",
									roomItem2.wallCoord,
									"' WHERE id='",
									roomItem2.Id,
									"'"
								}));
							}
						}
					}
					foreach (RoomItem roomItem3 in this.mMovedItems.Values)
					{
						if (!string.IsNullOrEmpty(roomItem3.ExtraData))
						{
							queryChunk.AddQuery(string.Concat(new object[]
							{
								"UPDATE items SET extra_data = @EData",
								roomItem3.Id,
								" WHERE id='",
								roomItem3.Id,
								"'"
							}));
							queryChunk.AddParameter("EData" + roomItem3.Id, roomItem3.ExtraData);
						}
						if (roomItem3.IsWallItem && (!roomItem3.GetBaseItem().Name.Contains("wallpaper_single") || !roomItem3.GetBaseItem().Name.Contains("floor_single") || !roomItem3.GetBaseItem().Name.Contains("landscape_single")))
						{
							queryChunk.AddQuery(string.Concat(new object[]
							{
								"UPDATE items SET wall_pos='",
								roomItem3.wallCoord,
								"' WHERE id=",
								roomItem3.Id
							}));
						}
						else
						{
							if (roomItem3.GetBaseItem().Name.Contains("wallpaper_single") || roomItem3.GetBaseItem().Name.Contains("floor_single") || roomItem3.GetBaseItem().Name.Contains("landscape_single"))
							{
								queryChunk.AddQuery("DELETE FROM items WHERE id=" + roomItem3.Id + " LIMIT 1");
							}
							else
							{
								queryChunk.AddQuery(string.Concat(new object[]
								{
									"UPDATE items SET x=",
									roomItem3.GetX,
									", y=",
									roomItem3.GetY,
									", z=",
									roomItem3.GetZ,
									", rot=",
									roomItem3.Rot,
									" WHERE id=",
									roomItem3.Id
								}));
							}
						}
					}
					this.room.GetRoomUserManager().AppendPetsUpdateString(dbClient);
					if (Session != null)
					{
						Session.GetHabbo().GetInventoryComponent().RunDBUpdate();
					}
					this.mAddedItems.Clear();
					this.mRemovedItems.Clear();
					this.mMovedItems.Clear();
					queryChunk.Execute(dbClient);
					queryChunk2.Execute(dbClient);
					queryChunk3.Execute(dbClient);
					queryChunk.Dispose();
					queryChunk2.Dispose();
					queryChunk3.Dispose();
					queryChunk = null;
					queryChunk2 = null;
					queryChunk3 = null;
				}
			}
			catch (Exception ex)
			{
				Logging.LogCriticalException(string.Concat(new object[]
				{
					"Error during saving furniture for room ",
					this.room.RoomId,
					". Stack: ",
					ex.ToString()
				}));
			}
		}
		internal bool SetFloorItem(GameClient Session, RoomItem Item, int newX, int newY, int newRot, bool newItem, bool OnRoller, bool sendMessage)
		{
			return this.SetFloorItem(Session, Item, newX, newY, newRot, newItem, OnRoller, sendMessage, true);
		}
		internal bool SetFloorItem(GameClient Session, RoomItem Item, int newX, int newY, int newRot, bool newItem, bool OnRoller, bool sendMessage, bool updateRoomUserStatuses)
		{
			bool flag = false;
			if (!newItem)
			{
				flag = this.room.GetGameMap().RemoveFromMap(Item);
			}
			Dictionary<int, ThreeDCoord> affectedTiles = Gamemap.GetAffectedTiles(Item.GetBaseItem().InteractionType, Item.GetBaseItem().Length, Item.GetBaseItem().Width, newX, newY, newRot);
			if (!this.room.GetGameMap().ValidTile(newX, newY) || (this.room.GetGameMap().SquareHasUsers(newX, newY) && !Item.GetBaseItem().IsSeat))
			{
				if (flag)
				{
					this.AddItem(Item);
					this.room.GetGameMap().AddToMap(Item);
				}
				return false;
			}
            
			foreach (ThreeDCoord current in affectedTiles.Values)
			{
				if (!this.room.GetGameMap().ValidTile(current.X, current.Y) || (this.room.GetGameMap().SquareHasUsers(current.X, current.Y) && !Item.GetBaseItem().IsSeat))
				{
					if (flag)
					{
						this.AddItem(Item);
						this.room.GetGameMap().AddToMap(Item);
					}
					bool result = false;
					return result;
				}
			}
			double num = (double)this.room.GetGameMap().Model.SqFloorHeight[newX, newY];
			if (!OnRoller)
			{
				if (this.room.GetGameMap().Model.SqState[newX, newY] != SquareState.OPEN && !Item.GetBaseItem().IsSeat)
				{
					if (flag)
					{
						this.AddItem(Item);
						if (newItem)
						{
							this.room.GetGameMap().AddToMap(Item);
						}
					}
					return false;
				}
				foreach (ThreeDCoord current2 in affectedTiles.Values)
				{
					if (this.room.GetGameMap().Model.SqState[current2.X, current2.Y] != SquareState.OPEN && !Item.GetBaseItem().IsSeat)
					{
						if (flag)
						{
							this.AddItem(Item);
							this.room.GetGameMap().AddToMap(Item);
						}
						bool result = false;
						return result;
					}
				}
				if (!Item.GetBaseItem().IsSeat && !Item.IsRoller)
				{
					foreach (ThreeDCoord current3 in affectedTiles.Values)
					{
						if (this.room.GetGameMap().GetRoomUsers(new Point(current3.X, current3.Y)).Count > 0)
						{
							if (flag)
							{
								this.AddItem(Item);
								this.room.GetGameMap().AddToMap(Item);
							}
							bool result = false;
							return result;
						}
					}
				}
			}
			List<RoomItem> furniObjects = this.GetFurniObjects(newX, newY);
			List<RoomItem> list = new List<RoomItem>();
			List<RoomItem> list2 = new List<RoomItem>();
			foreach (ThreeDCoord current4 in affectedTiles.Values)
			{
				List<RoomItem> furniObjects2 = this.GetFurniObjects(current4.X, current4.Y);
				if (furniObjects2 != null)
				{
					list.AddRange(furniObjects2);
				}
			}
			list2.AddRange(furniObjects);
			list2.AddRange(list);
			if (!OnRoller)
			{
				foreach (RoomItem current5 in list2)
				{
					if (current5 != null && current5.Id != Item.Id && current5.GetBaseItem() != null && !current5.GetBaseItem().Stackable)
					{
						if (flag)
						{
							this.AddItem(Item);
							this.room.GetGameMap().AddToMap(Item);
						}
						bool result = false;
						return result;
					}
				}
			}
			if (Item.Rot != newRot && Item.GetX == newX && Item.GetY == newY)
			{
				num = Item.GetZ;
			}
			foreach (RoomItem current6 in list2)
			{
				if (current6.Id != Item.Id && current6.TotalHeight > num)
				{
					num = current6.TotalHeight;
				}
			}
			if (newRot != 0 && newRot != 2 && newRot != 4 && newRot != 6 && newRot != 8 && Item.GetBaseItem().InteractionType != InteractionType.mannequin)
			{
				newRot = 0;
			}
			Item.Rot = newRot;
			int arg_47A_0 = Item.GetX;
			int arg_481_0 = Item.GetY;
			Item.SetState(newX, newY, num, affectedTiles);
			if (!OnRoller && Session != null)
			{
				Item.Interactor.OnPlace(Session, Item);
			}
			if (newItem)
			{
				if (this.mFloorItems.ContainsKey(Item.Id))
				{
					return true;
				}
				if (Item.IsFloorItem && !this.mFloorItems.ContainsKey(Item.Id))
				{
					this.mFloorItems.Add(Item.Id, Item);
				}
				else
				{
					if (Item.IsWallItem && !this.mWallItems.ContainsKey(Item.Id))
					{
						this.mWallItems.Add(Item.Id, Item);
					}
				}
				this.AddItem(Item);
				if (sendMessage)
				{
					ServerMessage serverMessage = new ServerMessage(Outgoing.AddFloorItemMessageComposer);
					Item.Serialize(serverMessage);
					if (this.room.Group != null)
					{
						serverMessage.AppendString(Session.GetHabbo().Username);
					}
					else
					{
						serverMessage.AppendString(this.room.Owner);
					}
					this.room.SendMessage(serverMessage);
                     
				}
			}
			else
			{
				this.UpdateItem(Item);
				if (!OnRoller && sendMessage)
				{
					ServerMessage message = new ServerMessage(Outgoing.UpdateRoomItemMessageComposer);
					Item.Serialize(message);
					this.room.SendMessage(message);
				}
				if (Item.IsWired)
				{
					this.room.GetWiredHandler().MoveWired(Item);
				}
			}
			this.room.GetGameMap().AddToMap(Item);
			if (Item.GetBaseItem().IsSeat)
			{
				updateRoomUserStatuses = true;
			}
			if (updateRoomUserStatuses)
			{
				this.room.GetRoomUserManager().UpdateUserStatusses();
			}
            if (newItem)
            {
                this.OnHeightmapUpdate(affectedTiles);
            }

			return true;
		}
        internal void OnHeightmapUpdate(Dictionary<int, ThreeDCoord> affectedTiles)
        {
            ServerMessage Message = new ServerMessage(Outgoing.UpdateFurniStackMapMessageComposer);
            Message.AppendByte((byte)affectedTiles.Count);
            foreach (var Coord in affectedTiles.Values)
            {
                Message.AppendByte((byte)Coord.X);
                Message.AppendByte((byte)Coord.Y);
                Message.AppendShort((short)(room.GetGameMap().SqAbsoluteHeight(Coord.X, Coord.Y) * 256));
            }
            room.SendMessage(Message);
        }
        internal void OnHeightmapUpdate(ICollection affectedTiles)
        {
            ServerMessage Message = new ServerMessage(Outgoing.UpdateFurniStackMapMessageComposer);
            Message.AppendByte((byte)affectedTiles.Count);
            foreach (Point Coord in affectedTiles)
            {
                Message.AppendByte((byte)Coord.X);
                Message.AppendByte((byte)Coord.Y);
                Message.AppendShort((short)(room.GetGameMap().SqAbsoluteHeight(Coord.X, Coord.Y) * 256));
            }
            room.SendMessage(Message);
        }
        internal void OnHeightmapUpdate(List<Point> oldCoords, List<Point> newCoords)
        {
            ServerMessage Message = new ServerMessage(Outgoing.UpdateFurniStackMapMessageComposer);
            Message.AppendByte((byte)(oldCoords.Count + newCoords.Count));
            foreach (Point Coord in oldCoords)
            {
                Message.AppendByte((byte)Coord.X);
                Message.AppendByte((byte)Coord.Y);
                Message.AppendShort((short)(room.GetGameMap().SqAbsoluteHeight(Coord.X, Coord.Y) * 256));
            }
            foreach (Point nCoord in newCoords)
            {
                Message.AppendByte((byte)nCoord.X);
                Message.AppendByte((byte)nCoord.Y);
                Message.AppendShort((short)(room.GetGameMap().SqAbsoluteHeight(nCoord.X, nCoord.Y) * 256));
            }
            room.SendMessage(Message);
        }
		internal List<RoomItem> GetFurniObjects(int X, int Y)
		{
			return this.room.GetGameMap().GetCoordinatedItems(new Point(X, Y));
		}
		internal bool SetFloorItem(RoomItem Item, int newX, int newY, double newZ)
		{
			this.room.GetGameMap().RemoveFromMap(Item);
			Item.SetState(newX, newY, newZ, Gamemap.GetAffectedTiles(Item.GetBaseItem().InteractionType, Item.GetBaseItem().Length, Item.GetBaseItem().Width, newX, newY, Item.Rot));
			if (Item.GetBaseItem().InteractionType == InteractionType.roombg && this.room.TonerData == null)
			{
				this.room.TonerData = new TonerData(Item.Id);
			}
			this.UpdateItem(Item);
			this.room.GetGameMap().AddItemToMap(Item, true);
			return true;
		}
		internal bool SetFloorItem(RoomItem Item, int newX, int newY, double newZ, int rot, bool sendupdate)
		{
			this.room.GetGameMap().RemoveFromMap(Item);
			Item.SetState(newX, newY, newZ, Gamemap.GetAffectedTiles(Item.GetBaseItem().InteractionType, Item.GetBaseItem().Length, Item.GetBaseItem().Width, newX, newY, rot));
			if (Item.GetBaseItem().InteractionType == InteractionType.roombg && this.room.TonerData == null)
			{
				this.room.TonerData = new TonerData(Item.Id);
			}
			this.UpdateItem(Item);
			this.room.GetGameMap().AddItemToMap(Item, true);
			if (sendupdate)
			{
				ServerMessage message = new ServerMessage(Outgoing.UpdateRoomItemMessageComposer);
				Item.Serialize(message);
				this.room.SendMessage(message);
			}
			return true;
		}
		internal bool SetWallItem(GameClient Session, RoomItem Item)
		{
			if (!Item.IsWallItem || this.mWallItems.ContainsKey(Item.Id))
			{
				return false;
			}
			if (this.mFloorItems.ContainsKey(Item.Id))
			{
				return true;
			}
			Item.Interactor.OnPlace(Session, Item);
			if (Item.GetBaseItem().InteractionType == InteractionType.dimmer && this.room.MoodlightData == null)
			{
				this.room.MoodlightData = new MoodlightData(Item.Id);
				Item.ExtraData = this.room.MoodlightData.GenerateExtraData();
			}
			this.mWallItems.Add(Item.Id, Item);
			this.AddItem(Item);
			ServerMessage serverMessage = new ServerMessage(Outgoing.AddWallItemMessageComposer);
			Item.Serialize(serverMessage);
			serverMessage.AppendString(this.room.Owner);
			this.room.SendMessage(serverMessage);
			return true;
		}
		internal void UpdateItem(RoomItem item)
		{
            if (this.mAddedItems.Contains(item.Id))
			{
				return;
			}
            if (this.mRemovedItems.Contains(item.Id))
			{
				this.mRemovedItems.Remove(item.Id);
			}
            if (!this.mMovedItems.Contains(item.Id))
			{
				this.mMovedItems.Add(item.Id, item);
			}
		}
		internal void AddItem(RoomItem item)
		{
            if (this.mRemovedItems.Contains(item.Id))
			{
				this.mRemovedItems.Remove(item.Id);
			}
            if (!this.mMovedItems.Contains(item.Id) && !this.mAddedItems.Contains(item.Id))
			{
				this.mAddedItems.Add(item.Id, item);
			}
		}
		internal void RemoveItem(RoomItem item)
		{
            if (this.mAddedItems.Contains(item.Id))
			{
				this.mAddedItems.Remove(item.Id);
			}
            if (this.mMovedItems.Contains(item.Id))
			{
				this.mMovedItems.Remove(item.Id);
			}
            if (!this.mRemovedItems.Contains(item.Id))
			{
				this.mRemovedItems.Add(item.Id, item);
			}
			this.mRollers.Remove(item.Id);
		}
		internal void OnCycle()
		{
			if (this.mGotRollers)
			{
				try
				{
					this.room.SendMessage(this.CycleRollers());
				}
				catch (Exception ex)
				{
					Logging.LogThreadException(ex.ToString(), "rollers for room with ID " + this.room.RoomId);
					this.mGotRollers = false;
				}
			}
			if (this.roomItemUpdateQueue.Count > 0)
			{
				List<RoomItem> list = new List<RoomItem>();
				lock (this.roomItemUpdateQueue.SyncRoot)
				{
					while (this.roomItemUpdateQueue.Count > 0)
					{
						RoomItem roomItem = (RoomItem)this.roomItemUpdateQueue.Dequeue();
						roomItem.ProcessUpdates();
						if (roomItem.IsTrans || roomItem.UpdateCounter > 0)
						{
							list.Add(roomItem);
						}
					}
					foreach (RoomItem current in list)
					{
						this.roomItemUpdateQueue.Enqueue(current);
					}
				}
			}
			this.mFloorItems.OnCycle();
			this.mWallItems.OnCycle();
		}
		internal void Destroy()
		{
			this.mFloorItems.Clear();
			this.mWallItems.Clear();
			this.mRemovedItems.Clear();
			this.mMovedItems.Clear();
			this.mAddedItems.Clear();
			this.roomItemUpdateQueue.Clear();
			this.room = null;
			this.mFloorItems = null;
			this.mWallItems = null;
			this.mRemovedItems = null;
			this.mMovedItems = null;
			this.mAddedItems = null;
			this.mWallItems = null;
			this.roomItemUpdateQueue = null;
		}

        public bool CheckPosItem(GameClient Session, RoomItem Item, int newX, int newY, int newRot, bool newItem, bool SendNotify = true)
        {
            try
            {
                Dictionary<int, ThreeDCoord> dictionary = Gamemap.GetAffectedTiles(Item.GetBaseItem().InteractionType, Item.GetBaseItem().Length, Item.GetBaseItem().Width, newX, newY, newRot);
                if (!this.room.GetGameMap().ValidTile(newX, newY))
                {
                    return false;
                }
                foreach (ThreeDCoord coord in dictionary.Values)
                {
                    if ((this.room.GetGameMap().Model.DoorX == coord.X) && (this.room.GetGameMap().Model.DoorY == coord.Y))
                    {

                        return false;
                    }
                }
                if ((this.room.GetGameMap().Model.DoorX == newX) && (this.room.GetGameMap().Model.DoorY == newY))
                {
                    return false;
                }
                foreach (ThreeDCoord coord in dictionary.Values)
                {
                    if (!this.room.GetGameMap().ValidTile(coord.X, coord.Y))
                    {

                        return false;
                    }
                }
                double num = this.room.GetGameMap().Model.SqFloorHeight[newX, newY];
                if ((((Item.Rot == newRot) && (Item.GetX == newX)) && (Item.GetY == newY)) && (Item.GetZ != num))
                {

                    return false;
                }
                if (this.room.GetGameMap().Model.SqState[newX, newY] != SquareState.OPEN)
                {

                    return false;
                }
                foreach (ThreeDCoord coord in dictionary.Values)
                {
                    if (this.room.GetGameMap().Model.SqState[coord.X, coord.Y] != SquareState.OPEN)
                    {

                        return false;
                    }
                }
                if (!Item.GetBaseItem().IsSeat)
                {
                    if (this.room.GetGameMap().SquareHasUsers(newX, newY))
                    {

                        return false;
                    }
                    foreach (ThreeDCoord coord in dictionary.Values)
                    {
                        if (this.room.GetGameMap().SquareHasUsers(coord.X, coord.Y))
                        {

                            return false;
                        }
                    }
                }
                List<RoomItem> furniObjects = this.GetFurniObjects(newX, newY);
                List<RoomItem> collection = new List<RoomItem>();
                List<RoomItem> list3 = new List<RoomItem>();
                foreach (ThreeDCoord coord in dictionary.Values)
                {
                    List<RoomItem> list4 = this.GetFurniObjects(coord.X, coord.Y);
                    if (list4 != null)
                    {
                        collection.AddRange(list4);
                    }
                }
                if (furniObjects == null)
                {
                    furniObjects = new List<RoomItem>();
                }
                list3.AddRange(furniObjects);
                list3.AddRange(collection);
                foreach (RoomItem item in list3)
                {
                    if ((item.Id != Item.Id) && !item.GetBaseItem().Stackable)
                    {

                        return false;
                    }
                }
                return true;
            }
            catch
            {

                return false;
            }
        }
    }
}
