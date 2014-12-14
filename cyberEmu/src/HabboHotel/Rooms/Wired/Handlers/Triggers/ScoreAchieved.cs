using Cyber.HabboHotel.Items;
using System;
using System.Collections.Generic;
namespace Cyber.HabboHotel.Rooms.Wired.Handlers.Triggers
{
    public class ScoreAchieved : WiredItem
    {
        private WiredItemType mType = WiredItemType.TriggerScoreAchieved;
        private Room mRoom;
        private RoomItem mItem;

        private string mString;
        private bool mBool;
        private string mExtra;
        private string mExtra2;
        private int mDelay;

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
                return mDelay;
            }
            set
            {
                mDelay = value;
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
                this.mString = value;
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
                return mBool;
            }
            set
            {
                mBool = value;
            }
        }

        public ScoreAchieved(RoomItem Item, Room Room)
        {
            this.mItem = Item;
            this.mRoom = Room;
            this.mString = "";
            this.mDelay = 0;
            this.mBool = true;
            this.mExtra = "";
            this.mExtra2 = "";
        }

        public bool Execute(params object[] Stuff)
        {
            RoomUser roomUser = (RoomUser)Stuff[0];
            if (roomUser == null)
            {
                return false;
            }

            int ScoreToGet = 100;
            int.TryParse(mString, out ScoreToGet);

            if (mRoom.GetGameManager().TeamPoints[(int)roomUser.team] < ScoreToGet)
            {
                return false;
            }

            List<WiredItem> conditions = this.mRoom.GetWiredHandler().GetConditions(this);
            List<WiredItem> effects = this.mRoom.GetWiredHandler().GetEffects(this);
            if (conditions.Count > 0)
            {
                foreach (WiredItem current in conditions)
                {
                    if (!current.Execute(new object[]
					{
						roomUser
					}))
                    {
                        return false;
                    }
                    this.mRoom.GetWiredHandler().OnEvent(current);
                }
            }
            if (effects.Count > 0)
            {
                foreach (WiredItem current2 in effects)
                {
                    if (current2.Execute(new object[]
					{
						roomUser,
						this.mType
					}))
                    {
                        this.mRoom.GetWiredHandler().OnEvent(current2);
                    }
                }
            }
            this.Room.GetWiredHandler().OnEvent(this);
            return true;
        }
    }
}
