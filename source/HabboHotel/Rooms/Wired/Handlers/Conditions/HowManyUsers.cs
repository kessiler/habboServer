using Cyber.HabboHotel.Items;
using System;
using System.Collections.Generic;

namespace Cyber.HabboHotel.Rooms.Wired.Handlers.Conditions
{
    class HowManyUsers : WiredItem
    {
        private WiredItemType mType = WiredItemType.ConditionHowManyUsers;
        private RoomItem mItem;
        private Room mRoom;
        private string mString;
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
                return mString;
            }
            set
            {
                mString = value;
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

        public HowManyUsers(RoomItem Item, Room Room)
        {
            this.mItem = Item;
            this.mRoom = Room;
            this.mItems = new List<RoomItem>();
            this.mString = "";
        }

        public bool Execute(params object[] Stuff)
        {
            bool Approved = false;

            int Minimum = 1;
            int Maximum = 50;

            if (!String.IsNullOrWhiteSpace(mString))
            {
                string[] Integers = mString.Split(',');
                Minimum = int.Parse(Integers[0]);
                Maximum = int.Parse(Integers[1]);
            }

            if (Room.UsersNow >= Minimum && Room.UsersNow <= Maximum)
            {
                Approved = true;
            }

            return Approved;
        }
    }
}
