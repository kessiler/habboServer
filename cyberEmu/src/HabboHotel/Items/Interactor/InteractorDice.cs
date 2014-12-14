using Cyber.HabboHotel.GameClients;
using Cyber.HabboHotel.Rooms;
using System;
namespace Cyber.HabboHotel.Items.Interactor
{
	internal class InteractorDice : IFurniInteractor
	{
		public void OnPlace(GameClient Session, RoomItem Item)
		{
			if (Item.ExtraData == "-1")
			{
				Item.ExtraData = "0";
				Item.UpdateNeeded = true;
			}
		}
		public void OnRemove(GameClient Session, RoomItem Item)
		{
			if (Item.ExtraData == "-1")
			{
				Item.ExtraData = "0";
			}
		}
		public void OnTrigger(GameClient Session, RoomItem Item, int Request, bool HasRights)
		{
			RoomUser roomUser = null;
			if (Session != null)
			{
				roomUser = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
			}
			if (roomUser == null)
			{
				return;
			}
			if (Gamemap.TilesTouching(Item.GetX, Item.GetY, roomUser.X, roomUser.Y))
			{
				if (Item.ExtraData != "-1")
				{
					if (Request == -1)
					{
						Item.ExtraData = "0";
						Item.UpdateState();
						return;
					}
					Item.ExtraData = "-1";
					Item.UpdateState(false, true);
					Item.ReqUpdate(4, true);
					return;
				}
			}
			else
			{
				roomUser.MoveTo(Item.SquareInFront);
			}
		}
		public void OnUserWalk(GameClient Session, RoomItem Item, RoomUser User)
		{
		}
		public void OnWiredTrigger(RoomItem Item)
		{
			Item.ExtraData = "-1";
			Item.UpdateState(false, true);
			Item.ReqUpdate(4, true);
		}
	}
}
