using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Rooms;
using Cyber.HabboHotel.Rooms.Games;
using System;
namespace Cyber.HabboHotel.Items.Interactor
{
    internal class InteractorFreezeTile : IFurniInteractor
    {
        public void OnPlace(GameClient Session, RoomItem Item)
        {
        }

        public void OnRemove(GameClient Session, RoomItem Item)
        {
        }

        public void OnTrigger(GameClient Session, RoomItem Item, int Request, bool HasRights)
        {
            if (Session == null || Session.GetHabbo() == null || Item.InteractingUser > 0U)
                return;
            string pName = Session.GetHabbo().Username;
            RoomUser roomUserByHabbo = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(pName);
            roomUserByHabbo.GoalX = Item.GetX;
            roomUserByHabbo.GoalY = Item.GetY;
            if (roomUserByHabbo.team != Team.none)
                roomUserByHabbo.throwBallAtGoal = true;
        }

        public void OnWiredTrigger(RoomItem Item)
        {
        }

        public void OnUserWalk(GameClient Client, RoomItem Item, RoomUser User)
        {

        }
    }

}
