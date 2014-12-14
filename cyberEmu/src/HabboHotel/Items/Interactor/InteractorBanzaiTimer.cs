using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Rooms;
using System;
namespace Cyber.HabboHotel.Items.Interactor
{
    internal class InteractorBanzaiTimer : IFurniInteractor
    {
        public void OnPlace(GameClient Session, RoomItem Item)
        {
        }

        public void OnRemove(GameClient Session, RoomItem Item)
        {
        }

        public void OnTrigger(GameClient Session, RoomItem Item, int Request, bool HasRights)
        {
            if (!HasRights)
                return;
            int result = 0;
            if (!int.TryParse(Item.ExtraData, out result))
            {
                Item.ExtraData = "0";
                result = 0;
            }
            if (Request == 0 && result == 0)
                result = 30;
            else if (Request == 2)
            {
                if (Item.GetRoom().GetBanzai().isBanzaiActive && Item.pendingReset && result > 0)
                {
                    result = 0;
                    Item.pendingReset = false;
                }
                else
                {
                    result = result >= 30 ? (result != 30 ? (result != 60 ? (result != 120 ? (result != 180 ? (result != 300 ? 0 : 600) : 300) : 180) : 120) : 60) : 30;
                    Item.UpdateNeeded = false;
                }
            }
            else if (Request == 1 || Request == 0)
            {
                if (!Item.GetRoom().GetBanzai().isBanzaiActive)
                {
                    Item.UpdateNeeded = !Item.UpdateNeeded;
                    if (Item.UpdateNeeded)
                        Item.GetRoom().GetBanzai().BanzaiStart();
                    Item.pendingReset = true;
                }
                else
                {
                    Item.UpdateNeeded = !Item.UpdateNeeded;
                    if (Item.UpdateNeeded)
                        Item.GetRoom().GetBanzai().BanzaiEnd();
                    Item.pendingReset = true;
                }
            }
            Item.ExtraData = Convert.ToString(result);
            Item.UpdateState();
        }

        public void OnWiredTrigger(RoomItem Item)
        {
            if (!Item.GetRoom().GetBanzai().isBanzaiActive)
            {
                Item.UpdateNeeded = !Item.UpdateNeeded;
                if (Item.UpdateNeeded)
                    Item.GetRoom().GetBanzai().BanzaiStart();
                Item.pendingReset = true;
            }
            Item.UpdateNeeded = !Item.UpdateNeeded;
            if (Item.UpdateNeeded)
                Item.GetRoom().GetBanzai().BanzaiEnd();
            Item.pendingReset = true;
        }

        public void OnUserWalk(GameClient Client, RoomItem Item, RoomUser User)
        {

        }
    }

}
