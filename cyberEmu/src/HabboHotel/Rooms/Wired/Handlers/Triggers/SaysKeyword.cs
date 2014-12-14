using Cyber.HabboHotel.Items;
using System;
using System.Collections.Generic;
namespace Cyber.HabboHotel.Rooms.Wired.Handlers.Triggers
{
	public class SaysKeyword : WiredItem
	{
		private WiredItemType mType = WiredItemType.TriggerUserSaysKeyword;
		private Room mRoom;
		private RoomItem mItem;
		private string mKeyword;
		private bool mOwner;
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
				return this.mKeyword;
			}
			set
			{
				this.mKeyword = value;
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
				return this.mOwner;
			}
			set
			{
				this.mOwner = value;
			}
		}
		public SaysKeyword(RoomItem Item, Room Room)
		{
			this.mItem = Item;
			this.mRoom = Room;
			this.mKeyword = "";
			this.mOwner = false;
		}
		public bool Execute(params object[] Stuff)
		{
			RoomUser roomUser = (RoomUser)Stuff[0];
			string text = (string)Stuff[1];
			if (string.IsNullOrEmpty(this.mKeyword))
			{
				return false;
			}
			if (text.ToLower() == this.mKeyword.ToLower())//but I think it's "Contains"
			{
				List<WiredItem> conditions = this.mRoom.GetWiredHandler().GetConditions(this);
				List<WiredItem> effects = this.mRoom.GetWiredHandler().GetEffects(this);
				bool flag = true;
				if (conditions.Count > 0)
				{
					foreach (WiredItem current in conditions)
					{
						this.mRoom.GetWiredHandler().OnEvent(current);
						if (!current.Execute(new object[]
						{
							roomUser
						}))
						{
							flag = false;
						}
					}
				}
                if (flag)
                {
                    roomUser.GetClient().SendWhisper(text);
                }
				if (effects.Count > 0 && flag)
				{
					foreach (WiredItem current2 in effects)
					{
						if (current2.Execute(new object[]
						{
							roomUser,
							this.Type
						}))
						{
							this.mRoom.GetWiredHandler().OnEvent(current2);
						}
					}
				}
               
				this.Room.GetWiredHandler().OnEvent(this);
				return true;
			}
			return false;
		}
	}
}
