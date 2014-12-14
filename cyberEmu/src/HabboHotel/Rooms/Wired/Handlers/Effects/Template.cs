using Cyber.HabboHotel.Items;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cyber.HabboHotel.Rooms.Wired.Handlers.Effects
{
    public class Template : WiredItem
    {
        private WiredItemType mType = WiredItemType.EffectGiveScore;
        private Room mRoom;
        private RoomItem mItem;
        private string mText;
        private string mExtra;
        private bool mBool;
        private string mExtra2;
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

        public Template(RoomItem Item, Room Room)
        {
            this.mItem = Item;
            this.mRoom = Room;
            this.mText = "";
            this.mExtra = "";
            this.mExtra2 = "";
            this.mBanned = new List<WiredItemType>();
        }

        public bool Execute(params object[] Stuff)
        {
            RoomUser roomUser = (RoomUser)Stuff[0];
            WiredItemType item = (WiredItemType)Stuff[1];
            return true;
        }

    }
}