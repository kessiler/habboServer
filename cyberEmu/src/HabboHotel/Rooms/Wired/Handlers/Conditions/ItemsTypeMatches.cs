using Cyber.HabboHotel.Items;
using Cyber.HabboHotel.Pathfinding;
using System;
using System.Collections.Generic;

namespace Cyber.HabboHotel.Rooms.Wired.Handlers.Conditions
{
    internal class ItemsTypeMatches : WiredItem
    {
        private WiredItemType mType = WiredItemType.ConditionFurniTypeMatches;
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

        public ItemsTypeMatches(RoomItem Item, Room Room)
        {
            this.mItem = Item;
            this.mRoom = Room;
            this.mItems = new List<RoomItem>();
        }

        public bool Execute(params object[] Stuff)
        {
            if (this.mItems.Count <= 1)
            {
                return true;
            }

            bool Approved = true;
            RoomItem lastitem = null;

            foreach (RoomItem current in mItems)
            {
                if (lastitem == null)
                {
                    lastitem = current;
                    continue;
                }

                if (current.GetBaseItem().InteractionType == InteractionType.none || lastitem.GetBaseItem().InteractionType == InteractionType.none)
                {
                    if (current.GetBaseItem().SpriteId != lastitem.GetBaseItem().SpriteId)
                    {
                        Approved = false;
                    }
                }
                else
                {
                    if (current.GetBaseItem().InteractionType.ToString().StartsWith("banzai") && lastitem.GetBaseItem().InteractionType.ToString().StartsWith("banzai"))
                    {
                        Approved = true;
                    }
                    else if (current.GetBaseItem().InteractionType.ToString().StartsWith("football") && lastitem.GetBaseItem().InteractionType.ToString().StartsWith("football"))
                    {
                        Approved = true;
                    }
                    else if (current.GetBaseItem().InteractionType.ToString().StartsWith("freeze") && lastitem.GetBaseItem().InteractionType.ToString().StartsWith("freeze"))
                    {
                        Approved = true;
                    }
                    else if (current.GetBaseItem().InteractionType != lastitem.GetBaseItem().InteractionType)
                    {
                        Approved = false;
                    }
                }
            }


            return Approved;
        }
    }
}
