using Cyber.HabboHotel.Items;
using System;
using System.Collections;
using System.Collections.Generic;
namespace Cyber.HabboHotel.Rooms.Wired.Handlers.Triggers
{
    internal class LongRepeater : WiredItem, WiredCycler
    {
        private WiredItemType mType = WiredItemType.TriggerLongRepeater;
        private Room mRoom;
        private RoomItem mItem;
        private int mDelay;
        private long mNext;
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
        public Queue ToWork
        {
            get
            {
                return new Queue();
            }
            set
            {
            }
        }
        public LongRepeater(RoomItem Item, Room Room)
        {
            this.mItem = Item;
            this.mRoom = Room;
            this.mDelay = 10000;
            this.mRoom.GetWiredHandler().EnqueueCycle(this);
            if (this.mNext == 0L || this.mNext < CyberEnvironment.Now())
            {
                this.mNext = checked(CyberEnvironment.Now() + unchecked((long)this.mDelay));
            }
        }
        public bool Execute(params object[] Stuff)
        {
            if (this.mNext == 0L || this.mNext < CyberEnvironment.Now())
            {
                this.mNext = checked(CyberEnvironment.Now() + unchecked((long)this.mDelay));
            }
            if (!this.mRoom.GetWiredHandler().IsCycleQueued(this))
            {
                this.mRoom.GetWiredHandler().EnqueueCycle(this);
            }
            return true;
        }
        public bool OnCycle()
        {
            long num = CyberEnvironment.Now();
            if (this.mNext < num)
            {
                List<WiredItem> conditions = this.mRoom.GetWiredHandler().GetConditions(this);
                List<WiredItem> effects = this.mRoom.GetWiredHandler().GetEffects(this);
                if (conditions.Count > 0)
                {
                    foreach (WiredItem current in conditions)
                    {
                        if (!current.Execute(null))
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
                        foreach (RoomUser current3 in this.Room.GetRoomUserManager().UserList.Values)
                        {
                            current2.Execute(new object[]
							{
								current3,
								this.mType
							});
                        }
                        this.mRoom.GetWiredHandler().OnEvent(current2);
                    }
                }
                this.mNext = checked(CyberEnvironment.Now() + unchecked((long)this.mDelay));
                return false;
            }
            return false;
        }
    }
}
