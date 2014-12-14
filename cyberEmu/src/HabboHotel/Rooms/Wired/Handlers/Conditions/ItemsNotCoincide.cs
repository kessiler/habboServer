using Cyber.HabboHotel.Items;
using Cyber.HabboHotel.Pathfinding;
using System;
using System.Collections.Generic;

namespace Cyber.HabboHotel.Rooms.Wired.Handlers.Conditions
{
    internal class ItemsNotCoincide : WiredItem
    {
        private WiredItemType mType = WiredItemType.ConditionItemsDontMatch;
        private RoomItem mItem;
        private Room mRoom;

        private string mString;
        private string mExtra;
        private string mExtra2;

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
                return mExtra;
            }
            set
            {
                mExtra = value;
            }
        }
        public string OtherExtraString2
        {
            get
            {
                return mExtra2;
            }
            set
            {
                mExtra2 = value;
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

        public ItemsNotCoincide(RoomItem Item, Room Room)
        {
            this.mItem = Item;
            this.mRoom = Room;
            this.mItems = new List<RoomItem>();
            this.mString = "";
            this.mExtra = "";
            this.mExtra2 = "";
        }

        public bool Execute(params object[] Stuff)
        {
            bool UseExtradata;
            bool UseRot;
            bool UsePos;

            if (String.IsNullOrWhiteSpace(mString))
            {
                return false;
            }
            else
            {
                string[] Booleans = mString.ToLower().Split(',');
                UseExtradata = Booleans[0] == "true";
                UseRot = Booleans[1] == "true";
                UsePos = Booleans[2] == "true";
            }

            bool EDApproved = true;
            bool RotApproved = true;
            bool PosApproved = true;

            RoomItem lastitem = null;

            foreach (RoomItem current in this.mItems)
            {
                if (lastitem == null)
                {
                    lastitem = current;
                    continue;
                }

                if (UseRot)
                {
                    if (current.Rot != lastitem.Rot)
                    {
                        RotApproved = false;
                    }
                }

                if (UseExtradata)
                {
                    if (current.ExtraData == "")
                    {
                        current.ExtraData = "0";
                    }

                    if (lastitem.ExtraData == "")
                    {
                        lastitem.ExtraData = "0";
                    }

                    if (current.ExtraData != lastitem.ExtraData)
                    {
                        EDApproved = false;
                    }
                }

                if (UsePos)
                {
                    if ((current.GetX != lastitem.GetY) && (current.GetY != lastitem.GetY))
                    {
                        PosApproved = false;
                    }
                }
            }

            if (!EDApproved || !PosApproved || !RotApproved)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
