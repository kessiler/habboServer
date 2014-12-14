using Cyber.HabboHotel.Items;
using System;
using System.Collections.Generic;
namespace Cyber.HabboHotel.Rooms.Wired.Handlers.Effects
{
	public class ShowMessage : WiredItem
	{
		private WiredItemType mType = WiredItemType.EffectShowMessage;
		private Room mRoom;
		private RoomItem mItem;
		private string mText;
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
				return this.mText;
			}
			set
			{
				this.mText = value;
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
		public ShowMessage(RoomItem Item, Room Room)
		{
			this.mItem = Item;
			this.mRoom = Room;
			this.mText = "";
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
			if (roomUser != null && !string.IsNullOrEmpty(this.mText))
			{
				roomUser.GetClient().SendWhisper(this.mText);
			}
			return true;
		}
	}
}
