using Cyber.HabboHotel.Items;
using Cyber.HabboHotel.Pathfinding;
using System;
using System.Collections.Generic;
using Cyber.HabboHotel.Users.Badges;

namespace Cyber.HabboHotel.Rooms.Wired.Handlers.Conditions
{
    internal class UserIsNotWearingBadge : WiredItem
    {
        private WiredItemType mType = WiredItemType.ConditionIsNotWearingBadge;
        private RoomItem mItem;
        private Room mRoom;
        private List<RoomItem> mItems;
        private string mExtra;

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
                return mExtra;
            }
            set
            {
                mExtra = value;
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

        public UserIsNotWearingBadge(RoomItem Item, Room Room)
        {
            this.mItem = Item;
            this.mRoom = Room;
            this.mItems = new List<RoomItem>();
            this.mExtra = "";
        }

        public bool Execute(params object[] Stuff)
        {
            if (Stuff[0] == null || !(Stuff[0] is RoomUser))
            {
                return false;
            }
            RoomUser roomUser = (RoomUser)Stuff[0];

            if (roomUser.IsBot || roomUser.GetClient() == null || string.IsNullOrWhiteSpace(mExtra))
                return false;

            foreach (Badge badge in roomUser.GetClient().GetHabbo().GetBadgeComponent().BadgeList.Values)
            {
                if (badge.Slot > 0 && badge.Code.ToLower() == OtherString.ToLower())
                {
                    return false;
                }
            }

            return true;
        }
    }
}
