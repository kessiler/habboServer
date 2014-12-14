using Cyber.HabboHotel.Items;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace Cyber.HabboHotel.Rooms.Wired.Handlers.Effects
{
	internal class TeleportToFurni : WiredItem, WiredCycler
	{
		private WiredItemType mType = WiredItemType.EffectTeleportToFurni;
		private RoomItem mItem;
		private Room mRoom;
		private List<RoomItem> mItems;
		private int mDelay;
		private long mNext;
		private Queue mUsers;
		private List<WiredItemType> mBanned;
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
		public TeleportToFurni(RoomItem Item, Room Room)
		{
			this.mItem = Item;
			this.mRoom = Room;
			this.mUsers = new Queue();
			this.mItems = new List<RoomItem>();
			this.mDelay = 0;
			this.mNext = 0L;
			this.mBanned = new List<WiredItemType>
			{
				WiredItemType.TriggerRepeatEffect
			};
		}
		public bool Execute(params object[] Stuff)
		{
			RoomUser roomUser = (RoomUser)Stuff[0];
			WiredItemType item = (WiredItemType)Stuff[1];
			if (this.mBanned.Contains(item))
			{
				return false;
			}
			if (roomUser == null || this.mItems.Count == 0 || this.mRoom.GetWiredHandler().IsCycleQueued(this))
			{
				return false;
			}
			if (this.mDelay < 500)
			{
				this.mDelay = 500;
			}
			if (this.mNext == 0L || this.mNext < CyberEnvironment.Now())
			{
				this.mNext = checked(CyberEnvironment.Now() + unchecked((long)this.mDelay));
			}
			lock (this.mUsers.SyncRoot)
			{
				if (!this.mUsers.Contains(roomUser))
				{
					this.mUsers.Enqueue(roomUser);
				}
			}
			this.mRoom.GetWiredHandler().EnqueueCycle(this);
			return true;
		}
		public bool OnCycle()
		{
			if (this.mUsers.Count == 0)
			{
				return true;
			}
			if (this.Room == null || this.Room.GetRoomItemHandler() == null || this.Room.GetRoomItemHandler().mFloorItems == null)
			{
				return false;
			}
			Queue queue = new Queue();
			long num = CyberEnvironment.Now();
			lock (this.mUsers.SyncRoot)
			{
				while (this.mUsers.Count > 0)
				{
					RoomUser roomUser = (RoomUser)this.mUsers.Dequeue();
					if (roomUser != null && roomUser.GetClient() != null)
					{
						if (this.mNext < num)
						{
							bool flag2 = this.Teleport(roomUser);
							if (flag2)
							{
								bool result = false;
								return result;
							}
						}
						else
						{
							if (checked(this.mNext - num) < 500L)
							{
								roomUser.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(4);
								queue.Enqueue(roomUser);
							}
							else
							{
								queue.Enqueue(roomUser);
							}
						}
					}
				}
			}
			if (this.mNext < num)
			{
				this.mNext = 0L;
				return true;
			}
			this.mUsers = queue;
			return false;
		}
		private bool Teleport(RoomUser User)
		{
			if (this.mItems.Count == 0)
			{
				return true;
			}
			if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null)
			{
				return true;
			}
			Random rnd = new Random();
			this.mItems = (
				from x in this.mItems
				orderby rnd.Next()
				select x).ToList<RoomItem>();
			RoomItem roomItem = null;
			foreach (RoomItem current in this.mItems)
			{
				if (current != null && this.Room.GetRoomItemHandler().mFloorItems.ContainsKey(current.Id))
				{
					roomItem = current;
				}
			}
			if (roomItem == null)
			{
				User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(0);
				return false;
			}
			this.mRoom.GetGameMap().TeleportToItem(User, roomItem);
			this.mRoom.GetRoomUserManager().UpdateUserStatusses();
			User.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(0);
			return true;
		}
	}
}
