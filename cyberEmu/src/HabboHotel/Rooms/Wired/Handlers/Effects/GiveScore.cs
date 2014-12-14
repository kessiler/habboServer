using Cyber.HabboHotel.Items;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cyber.HabboHotel.Rooms.Wired.Handlers.Effects
{
    public class GiveScore : WiredItem
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

        public GiveScore(RoomItem Item, Room Room)
        {
            this.mItem = Item;
            this.mRoom = Room;
            this.mText = "10,1";
            this.mExtra = "0";
            this.mExtra2 = "";
            this.mBanned = new List<WiredItemType>();
        }

        public bool Execute(params object[] Stuff)
        {
            RoomUser roomUser = (RoomUser)Stuff[0];
            WiredItemType item = (WiredItemType)Stuff[1];

            if (roomUser == null)
            {
                return false;
            }
            else if (roomUser.team == Games.Team.none)
            {
                return false;
            }

            int TimesDone = 0;
            int.TryParse(mExtra, out TimesDone);

            int ScoreToAchieve = 10;
            int MaxTimes = 1;

            if (!String.IsNullOrWhiteSpace(mText))
            {
                string[] Integers = mText.Split(',');
                ScoreToAchieve = int.Parse(Integers[0]);
                MaxTimes = int.Parse(Integers[1]);
            }

            if (TimesDone >= MaxTimes)
            {
                return false;
            }

            Room.GetGameManager().AddPointToTeam(roomUser.team, ScoreToAchieve, roomUser);
            TimesDone++;

            mExtra = TimesDone.ToString();
            return true;
        }

    }
}