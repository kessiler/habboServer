using Cyber.HabboHotel.Items;
using Cyber.HabboHotel.Pathfinding;
using System;
using System.Collections.Generic;
namespace Cyber.HabboHotel.Rooms.Wired.Handlers.Conditions
{
    internal class TriggererNotOnFurni : WiredItem
    {
        private WiredItemType mType = WiredItemType.ConditionTriggererNotOnFurni;
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
        public TriggererNotOnFurni(RoomItem Item, Room Room)
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
            if (Stuff[0] == null || !(Stuff[0] is RoomUser))
            {
                return false;
            }
            RoomUser roomUser = (RoomUser)Stuff[0];
            if (roomUser == null)
            {
                return false;
            }
            foreach (RoomItem current in this.mItems)
            {
                if (current != null && this.Room.GetRoomItemHandler().mFloorItems.ContainsKey(current.Id))
                {
                    foreach (ThreeDCoord current2 in current.GetAffectedTiles.Values)
                    {
                        if (roomUser.X == current2.X && roomUser.Y == current2.Y)
                        {
                            return false;
                        }
                    }
                    if (roomUser.X == current.GetX && roomUser.Y == current.GetY)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
