using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Rooms;
using Cyber.HabboHotel.Rooms.Games;
using System;
namespace Cyber.HabboHotel.Items.Interactor
{
    internal class InteractorBanzaiScoreCounter : IFurniInteractor
    {
        public void OnPlace(GameClient Session, RoomItem Item)
        {
            if (Item.team == Team.none)
                return;
            Item.ExtraData = Item.GetRoom().GetGameManager().Points[(int)Item.team].ToString();
            Item.UpdateState(false, true);
        }

        public void OnRemove(GameClient Session, RoomItem Item)
        {
        }

        public void OnTrigger(GameClient Session, RoomItem Item, int Request, bool HasRights)
        {
            if (!HasRights)
                return;
            Item.GetRoom().GetGameManager().Points[(int)Item.team] = 0;
            Item.ExtraData = "0";
            Item.UpdateState();
        }

        public void OnWiredTrigger(RoomItem Item)
        {
        }

        public void OnUserWalk(GameClient Client, RoomItem Item, RoomUser User)
        {

        }
    }

}
