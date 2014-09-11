using Cyber.HabboHotel.Items;
using Cyber.HabboHotel.Pathfinding;
using System;
using System.Collections.Generic;
using Cyber.HabboHotel.Users.Badges;

namespace Cyber.HabboHotel.Rooms.Wired.Handlers.Conditions
{
    internal class DateRangeActive : WiredItem
    {
        private WiredItemType mType = WiredItemType.ConditionDateRangeActive;
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

        public DateRangeActive(RoomItem Item, Room Room)
        {
            this.mItem = Item;
            this.mRoom = Room;
            this.mItems = new List<RoomItem>();
            this.mExtra = "";
        }

        public bool Execute(params object[] Stuff)
        {
            int Date1 = 0;
            int Date2 = 0;

            string[] strArray = mExtra.Split(',');

            if (string.IsNullOrWhiteSpace(strArray[0]))
            {
                return false;
            }

            int.TryParse(strArray[0], out Date1);

            if (strArray.Length > 1)
            {
                int.TryParse(strArray[1], out Date2);
            }

            if (Date1 == 0)
                return false;

            int CurrentTimestamp = CyberEnvironment.GetUnixTimestamp();

            bool Result = false;
            if (Date2 < 1)
            {
                Result = (CurrentTimestamp >= Date1);
            }
            else
            {
                Result = (CurrentTimestamp >= Date1 && CurrentTimestamp <= Date2);
            }
            return Result;
        }
    }
}