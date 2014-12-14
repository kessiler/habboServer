using Cyber.HabboHotel.Items;
using System;
using System.Collections;
using System.Collections.Generic;
namespace Cyber.HabboHotel.Rooms.Wired.Handlers.Effects
{
	public class ToggleFurniState : WiredItem, WiredCycler
	{
		private WiredItemType mType = WiredItemType.EffectToggleFurniState;
		private RoomItem mItem;
		private Room mRoom;
		private List<RoomItem> mItems;
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
				return null;
			}
			set
			{
			}
		}
		public ToggleFurniState(RoomItem Item, Room Room)
		{
			this.mItem = Item;
			this.mRoom = Room;
			this.mItems = new List<RoomItem>();
			this.mDelay = 0;
			this.mNext = 0L;
		}
		public bool Execute(params object[] Stuff)
		{
			RoomUser roomUser = (RoomUser)Stuff[0];
			if (roomUser == null || this.mItems.Count == 0)
			{
				return false;
			}
			if (this.mNext == 0L || this.mNext < CyberEnvironment.Now())
			{
				this.mNext = checked(CyberEnvironment.Now() + unchecked((long)this.mDelay));
			}
			this.mRoom.GetWiredHandler().EnqueueCycle(this);
			return true;
		}
		public bool OnCycle()
		{
			if (this.mItems.Count == 0)
			{
				return true;
			}
			long num = CyberEnvironment.Now();
			if (this.mNext < num)
			{
				foreach (RoomItem current in this.mItems)
				{
					if (current != null && this.Room.GetRoomItemHandler().mFloorItems.ContainsKey(current.Id))
					{
						current.Interactor.OnWiredTrigger(current);
					}
				}
			}
			if (this.mNext < num)
			{
				this.mNext = 0L;
				return true;
			}
			return false;
		}
	}
}
