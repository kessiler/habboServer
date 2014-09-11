using Cyber.HabboHotel.Items;
using System;
using System.Collections.Generic;
namespace Cyber.HabboHotel.Rooms.Wired.Handlers.Triggers
{
	internal class GameEnds : WiredItem
	{
		private WiredItemType mType = WiredItemType.TriggerGameEnds;
		private Room mRoom;
		private RoomItem mItem;
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
				return new List<RoomItem>();
			}
			set
			{
			}
		}
		public int Delay
		{
			get
			{
				return 0;
			}
			set
			{
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
		public GameEnds(RoomItem Item, Room Room)
		{
			this.mItem = Item;
			this.mRoom = Room;
		}
		public bool Execute(params object[] Stuff)
		{
			List<WiredItem> conditions = this.mRoom.GetWiredHandler().GetConditions(this);
			List<WiredItem> effects = this.mRoom.GetWiredHandler().GetEffects(this);
			if (conditions.Count > 0)
			{
				foreach (WiredItem current in conditions)
				{
					if (!current.Execute(null))
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
					foreach (RoomUser current3 in this.mRoom.GetRoomUserManager().UserList.Values)
					{
						current2.Execute(new object[]
						{
							current3,
							this.Type
						});
					}
					this.mRoom.GetWiredHandler().OnEvent(current2);
				}
			}
			this.Room.GetWiredHandler().OnEvent(this);
			return true;
		}
	}
}
