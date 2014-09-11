using Cyber.HabboHotel.Items;
using System;
using System.Collections.Generic;
namespace Cyber.HabboHotel.Rooms.Wired.Handlers.Triggers
{
	public class UserEntersRoom : WiredItem
	{
        private WiredItemType mType = 0;
		private Room mRoom;
		private RoomItem mItem;
		private string mUsername;
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
				return this.mUsername;
			}
			set
			{
				this.mUsername = value;
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
		public UserEntersRoom(RoomItem Item, Room Room)
		{
			this.mItem = Item;
			this.mRoom = Room;
			this.mUsername = "";
		}
		public bool Execute(params object[] Stuff)
		{
			RoomUser roomUser = (RoomUser)Stuff[0];
			if (!string.IsNullOrEmpty(this.mUsername) && roomUser.GetUsername() != this.mUsername && !roomUser.GetClient().GetHabbo().IsTeleporting)
			{
				return false;
			}
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
			this.Room.GetWiredHandler().OnEvent(this);
			return true;
		}
	}
}
