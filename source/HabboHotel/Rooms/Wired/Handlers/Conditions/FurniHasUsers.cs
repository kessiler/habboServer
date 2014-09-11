using Cyber.HabboHotel.Items;
using Cyber.HabboHotel.Pathfinding;
using System;
using System.Collections.Generic;
namespace Cyber.HabboHotel.Rooms.Wired.Handlers.Conditions
{
	internal class FurniHasUsers : WiredItem
	{
		private WiredItemType mType = WiredItemType.ConditionFurniHasUsers;
		private RoomItem mItem;
		private Room mRoom;
		private List<RoomItem> mItems;
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
				return 0;
			}
			set
			{
			}
		}
		public FurniHasUsers(RoomItem Item, Room Room)
		{
			this.mItem = Item;
			this.mRoom = Room;
			this.mItems = new List<RoomItem>();
		}
		public bool Execute(params object[] Stuff)
		{
			if (this.mItems.Count == 0)
			{
				return true;
			}
			bool result = true;
			foreach (RoomItem current in this.mItems)
			{
				if (current != null && this.Room.GetRoomItemHandler().mFloorItems.ContainsKey(current.Id))
				{
					bool flag = false;
					foreach (ThreeDCoord current2 in current.GetAffectedTiles.Values)
					{
						if (this.mRoom.GetGameMap().SquareHasUsers(current2.X, current2.Y))
						{
							flag = true;
						}
					}
					if (this.mRoom.GetGameMap().SquareHasUsers(current.GetX, current.GetY))
					{
						flag = true;
					}
					if (!flag)
					{
						result = false;
					}
				}
			}
			return result;
		}
	}
}
