using Cyber.HabboHotel.Items;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cyber.HabboHotel.Rooms.Wired.Handlers.Effects
{
    public class MuteUser : WiredItem
    {
        private WiredItemType mType = WiredItemType.EffectMuteUser;
        private Room mRoom;
        private RoomItem mItem;
        private string mText;
        private string mExtra;
        private bool mBool;
        private string mExtra2;
        private int mDelay;
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
                return this.mExtra;
            }
            set
            {
                this.mExtra = value;
            }
        }
        public string OtherExtraString2
        {
            get
            {
                return this.mExtra2;
            }
            set
            {
                this.mExtra2 = value;
            }
        }
        public bool OtherBool
        {
            get
            {
                return this.mBool;
            }
            set
            {
                this.mBool = value;
            }
        }

        public MuteUser(RoomItem Item, Room Room)
        {
            this.mItem = Item;
            this.mRoom = Room;
            this.mText = "";
            this.mExtra = "";
            this.mExtra2 = "";
            this.mDelay = 0;
            this.mBanned = new List<WiredItemType>();
        }

        public bool Execute(params object[] Stuff)
        {
            RoomUser roomUser = (RoomUser)Stuff[0];

            if (roomUser == null || roomUser.IsBot || roomUser.GetClient() == null || roomUser.GetClient().GetHabbo() == null)
                return false;

            if (roomUser.GetClient().GetHabbo().Rank > 3)
            {
                return false;
            }
                
            if (this.Delay == 0)
                return false;

            int Minutes = this.Delay / 500;
            uint UserId = roomUser.GetClient().GetHabbo().Id;

            if (Room.MutedUsers.ContainsKey(UserId))
            {
                Room.MutedUsers.Remove(UserId);
            }
            Room.MutedUsers.Add(UserId, Convert.ToUInt32((CyberEnvironment.GetUnixTimestamp() + (Minutes * 60))));
            if (!String.IsNullOrEmpty(this.OtherString))
            {
                roomUser.GetClient().SendWhisper(this.OtherString);
            }
            return true;
        }

    }
}