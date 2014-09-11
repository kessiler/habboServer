using Cyber.HabboHotel.Items;
using System;
using System.Collections;
using System.Collections.Generic;
namespace Cyber.HabboHotel.Rooms.Wired.Handlers.Triggers
{
	public class FurniStateToggled : WiredItem, WiredCycler
	{
		private WiredItemType mType = WiredItemType.TriggerToggleFurni;
		private Room mRoom;
		private RoomItem mItem;
		private List<RoomItem> mItems;
		private long mNext;
		private int mDelay;
		private List<RoomUser> mUsers;
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
				return null;
			}
			set
			{
			}
		}
		public FurniStateToggled(RoomItem Item, Room Room)
		{
			this.mItem = Item;
			this.mRoom = Room;
			this.mItems = new List<RoomItem>();
			this.mDelay = 0;
			this.mUsers = new List<RoomUser>();
		}
		public bool Execute(params object[] Stuff)
		{
			RoomUser roomUser = (RoomUser)Stuff[0];
			RoomItem roomItem = (RoomItem)Stuff[1];
			if (roomUser == null || roomItem == null)
			{
				return false;
			}
			if (!this.mItems.Contains(roomItem))
			{
				return false;
			}
			this.mUsers.Add(roomUser);
			if (this.mDelay == 0)
			{
				this.Room.GetWiredHandler().OnEvent(this);
				this.OnCycle();
			}
			else
			{
				if (this.mNext == 0L || this.mNext < CyberEnvironment.Now())
				{
					this.mNext = checked(CyberEnvironment.Now() + unchecked((long)this.mDelay));
				}
				this.mRoom.GetWiredHandler().EnqueueCycle(this);
			}
			return true;
		}
		public bool OnCycle()
		{
			long num = CyberEnvironment.Now();
			if (this.mNext < num)
			{
				List<WiredItem> conditions = this.mRoom.GetWiredHandler().GetConditions(this);
				List<WiredItem> effects = this.mRoom.GetWiredHandler().GetEffects(this);
				foreach (RoomUser current in this.mUsers)
				{
					if (conditions.Count > 0)
					{
						foreach (WiredItem current2 in conditions)
						{
							if (current2.Execute(new object[]
							{
								current
							}))
							{
								this.mRoom.GetWiredHandler().OnEvent(current2);
							}
						}
					}
					if (effects.Count > 0)
					{
						foreach (WiredItem current3 in effects)
						{
							if (current3.Execute(new object[]
							{
								current,
								this.Type
							}))
							{
								this.mRoom.GetWiredHandler().OnEvent(current3);
							}
						}
					}
				}
				this.Room.GetWiredHandler().OnEvent(this);
				this.mNext = 0L;
				return true;
			}
			return false;
		}
	}
}
