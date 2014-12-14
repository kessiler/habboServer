using Cyber.HabboHotel.Items;
using Cyber.HabboHotel.Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
namespace Cyber.HabboHotel.Rooms.Wired.Handlers.Triggers
{
	internal class WalksOffFurni : WiredItem, WiredCycler
	{
		private WiredItemType mType = WiredItemType.TriggerWalksOffFurni;
		private Room mRoom;
		private RoomItem mItem;
		private List<RoomItem> mItems;
		private Queue mUsers;
		private int mDelay;
		private long mNext;
		public WiredItemType Type
		{
			get
			{
				return this.mType;
			}
		}
		public RoomItem Item
		{
			get
			{
				return this.mItem;
			}
			set
			{
				this.mItem = value;
			}
		}
		public Room Room
		{
			get
			{
				return this.mRoom;
			}
		}
		public List<RoomItem> Items
		{
			get
			{
				return this.mItems;
			}
			set
			{
				this.mItems = value;
			}
		}
		public int Delay
		{
			get
			{
				return this.mDelay;
			}
			set
			{
				this.mDelay = value;
			}
		}
		public string OtherString
		{
			get
			{
				return "";
			}
			set
			{
			}
		}
		public string OtherExtraString
		{
			get
			{
				return "";
			}
			set
			{
			}
		}
		public string OtherExtraString2
		{
			get
			{
				return "";
			}
			set
			{
			}
		}
		public bool OtherBool
		{
			get
			{
				return true;
			}
			set
			{
			}
		}
		public Queue ToWork
		{
			get
			{
				return this.mUsers;
			}
			set
			{
				this.mUsers = value;
			}
		}
		public WalksOffFurni(RoomItem Item, Room Room)
		{
			this.mItem = Item;
			this.mRoom = Room;
			this.mUsers = new Queue();
			this.mItems = new List<RoomItem>();
		}
		public bool Execute(params object[] Stuff)
		{
			RoomUser roomUser = (RoomUser)Stuff[0];
			RoomItem roomItem = (RoomItem)Stuff[1];
			if (!this.mItems.Contains(roomItem) || roomUser.LastItem != roomItem.Id)
			{
				return false;
			}
			foreach (ThreeDCoord current in roomItem.GetAffectedTiles.Values)
			{
				if ((current.X == roomUser.X && current.Y == roomUser.Y) || (roomUser.X == roomItem.GetX && roomUser.Y == roomItem.GetY))
				{
					return false;
				}
			}
			this.mUsers.Enqueue(roomUser);
			if (this.mDelay == 0)
			{
				this.OnCycle();
			}
			else
			{
				this.mNext = checked(CyberEnvironment.Now() + unchecked((long)this.mDelay));
				this.mRoom.GetWiredHandler().EnqueueCycle(this);
			}
			return true;
		}
		public bool OnCycle()
		{
			long num = CyberEnvironment.Now();
			if (num > this.mNext)
			{
				lock (this.mUsers.SyncRoot)
				{
					while (this.mUsers.Count > 0)
					{
						RoomUser roomUser = (RoomUser)this.mUsers.Dequeue();
						List<WiredItem> conditions = this.mRoom.GetWiredHandler().GetConditions(this);
						List<WiredItem> effects = this.mRoom.GetWiredHandler().GetEffects(this);
						if (conditions.Count > 0)
						{
							foreach (WiredItem current in conditions)
							{
								if (!current.Execute(new object[]
								{
									roomUser
								}))
								{
									return false;
								}
								this.mRoom.GetWiredHandler().OnEvent(current);
							}
						}
						if (effects.Count > 0)
						{
							foreach (WiredItem current2 in effects)
							{
								if (current2.Execute(new object[]
								{
									roomUser,
									this.mType
								}))
								{
									this.mRoom.GetWiredHandler().OnEvent(current2);
								}
							}
						}
					}
				}
				this.mNext = 0L;
				this.Room.GetWiredHandler().OnEvent(this);
				return true;
			}
			return false;
		}
	}
}
