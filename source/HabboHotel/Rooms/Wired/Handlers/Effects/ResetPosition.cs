using Cyber.HabboHotel.Items;
using Cyber.Messages;
using Cyber.Messages.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Cyber.HabboHotel.Rooms.Wired.Handlers.Effects
{
    public class ResetPosition : WiredItem
    {
        private WiredItemType mType = WiredItemType.EffectResetPosition;
        private List<RoomItem> mItems;
        private Room mRoom;
        private RoomItem mItem;
        private string mText;
        private string mExtra;
        private bool mBool;
        private string mExtra2;
        private int mDelay;
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
                return mItems;
            }
            set
            {
                mItems = value;
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
                this.mDelay = value;
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

        public ResetPosition(RoomItem Item, Room Room)
        {
            this.mItem = Item;
            this.mRoom = Room;
            this.mText = "";
            this.mExtra = "";
            this.mExtra2 = "";
            this.mDelay = 0;
            this.mItems = new List<RoomItem>();
            this.mBanned = new List<WiredItemType>();
        }

        public bool Execute(params object[] Stuff)
        {
            RoomUser roomUser = (RoomUser)Stuff[0];
            WiredItemType item = (WiredItemType)Stuff[1];

            if (this.mBanned.Contains(item))
            {
                return false;
            }
            if (roomUser == null || roomUser.GetClient() == null || roomUser.GetClient().GetHabbo() == null)
            {
                return false;
            }

            Room Room = roomUser.GetClient().GetHabbo().CurrentRoom;

            if (Room == null)
            {
                return false;
            }

            if (String.IsNullOrWhiteSpace(mText) || String.IsNullOrWhiteSpace(mExtra))
            {
                return false;
            }

            string[] Booleans = mText.Split(',');

            if (Booleans.Length < 3)
            {
                return false;
            }


            bool ExtraData = Booleans[0] == "true";
            bool Rot = Booleans[1] == "true";
            bool Position = Booleans[2] == "true";

            foreach (string ItemData in mExtra.Split('/'))
            {
                if (String.IsNullOrWhiteSpace(ItemData))
                {
                    continue;
                }

                string[] InnerData = ItemData.Split('|');
                uint ItemId = uint.Parse(InnerData[0]);

                
                RoomItem Item = Room.GetRoomItemHandler().GetItem(ItemId);

                if (Item == null)
                {
                    continue;
                }

                string ExtraDataToSet = (ExtraData) ? InnerData[1] : Item.ExtraData;
                int RotationToSet = (Rot) ? int.Parse(InnerData[2]) : Item.Rot;

                string[] Positions = InnerData[3].Split(',');
                int XToSet = (Position) ? int.Parse(Positions[0]) : Item.GetX;
                int YToSet = (Position) ? int.Parse(Positions[1]) : Item.GetY;
                double ZToSet = (Position) ? double.Parse(Positions[2]) : Item.GetZ;

                
                ServerMessage serverMessage = new ServerMessage(Outgoing.ItemAnimationMessageComposer);
                serverMessage.AppendInt32(Item.GetX);
                serverMessage.AppendInt32(Item.GetY);
                serverMessage.AppendInt32(XToSet);
                serverMessage.AppendInt32(YToSet);
                serverMessage.AppendInt32(1);
                serverMessage.AppendUInt(Item.Id);
                serverMessage.AppendString(Item.GetZ.ToString(CyberEnvironment.cultureInfo));
                serverMessage.AppendString(ZToSet.ToString(CyberEnvironment.cultureInfo));
                serverMessage.AppendInt32(0);
                Room.SendMessage(serverMessage);

                Room.GetRoomItemHandler().SetFloorItem(roomUser.GetClient(), Item, XToSet, YToSet, RotationToSet, false, false, false, false);
                Item.ExtraData = ExtraDataToSet;
                Item.UpdateState();
            }
            return true;
        }
    }
}