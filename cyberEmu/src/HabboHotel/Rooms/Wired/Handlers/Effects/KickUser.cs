using Cyber.HabboHotel.Items;
using System;
using System.Collections;
using System.Timers;
using System.Collections.Generic;
namespace Cyber.HabboHotel.Rooms.Wired.Handlers.Effects
{
	public class KickUser : WiredItem
	{
		private WiredItemType mType = WiredItemType.EffectKickUser;
		private Room mRoom;
		private RoomItem mItem;
		private string mText;
		private List<RoomUser> mUsers;
		private List<WiredItemType> mBanned;
        private Timer mTimer;

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

		public KickUser(RoomItem Item, Room Room)
		{
			this.mItem = Item;
			this.mRoom = Room;
			this.mText = "";
            this.mUsers = new List<RoomUser>();
			this.mBanned = new List<WiredItemType>
			{
				WiredItemType.TriggerRepeatEffect,
				WiredItemType.TriggerUserEntersRoom
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

			if (roomUser != null && !string.IsNullOrWhiteSpace(this.mText))
			{
				if (roomUser.GetClient().GetHabbo().HasFuse("fuse_mod") || this.mRoom.Owner == roomUser.GetUsername())
				{
					return false;
				}
                roomUser.GetClient().GetHabbo().GetAvatarEffectsInventoryComponent().ActivateCustomEffect(4, false);
                roomUser.GetClient().SendWhisper(this.mText);
                mUsers.Add(roomUser);
			}

            if (mTimer == null)
            {
                mTimer = new Timer(2000);
            }

                this.mTimer.Elapsed += ExecuteKick;
                this.mTimer.Enabled = true;
            
			return true;
		}

        private void ExecuteKick(object Source, ElapsedEventArgs EEA)
        {
            mTimer.Stop();

            lock (mUsers)
            {
                foreach (RoomUser User in mUsers)
                {
                    mRoom.GetRoomUserManager().RemoveUserFromRoom(User.GetClient(), true, false);
                }
            }
            mUsers.Clear();
            this.mTimer = null;
        }
	}
}
